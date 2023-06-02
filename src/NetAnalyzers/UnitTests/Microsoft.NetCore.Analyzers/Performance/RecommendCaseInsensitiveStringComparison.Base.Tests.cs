// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public abstract class RecommendCaseInsensitiveStringComparison_Base_Tests
    {
        private static readonly Tuple<string, string>[] Cultures = new[] {
            Tuple.Create("ToLower", "CurrentCultureIgnoreCase"),
            Tuple.Create("ToUpper", "CurrentCultureIgnoreCase"),
            Tuple.Create("ToLowerInvariant", "InvariantCultureIgnoreCase"),
            Tuple.Create("ToUpperInvariant", "InvariantCultureIgnoreCase")
        };

        public static IEnumerable<object[]> DiagnosedAndFixedData()
        {
            foreach ((string caseChanging, string replacement) in Cultures)
            {
                foreach (string method in new[] { "Contains", "StartsWith" })
                {
                    yield return new object[] { $"a.{caseChanging}().{method}(b)", $"a.{method}(b, StringComparison.{replacement})" };
                }

                // IndexOf overloads
                foreach (string arguments in new[] { "b", "b, 1", "b, 1, 1" })
                {
                    yield return new object[] { $"a.{caseChanging}().IndexOf({arguments})", $"a.IndexOf({arguments}, StringComparison.{replacement})" };
                }

                // Fixer converts to a different class
                yield return new object[] { $"a.{caseChanging}().CompareTo(b)", $"StringComparer.{replacement}.Compare(a, b)" };
            }
        }

        public static IEnumerable<object[]> DiagnosedAndFixedWithEqualsToData()
        {
            foreach ((string caseChanging, string replacement) in Cultures)
            {
                foreach (string method in new[] { "Contains", "StartsWith" })
                {
                    // Tests implicit boolean check
                    yield return new object[] { $"a.{caseChanging}().{method}(b)", $"a.{method}(b, StringComparison.{replacement})", "" };
                }

                // Tests having an appended method invocation at the end

                // IndexOf overloads
                foreach (string arguments in new[] { "b", "b, 1", "b, 1, 1" })
                {
                    yield return new object[] { $"a.{caseChanging}().IndexOf({arguments})", $"a.IndexOf({arguments}, StringComparison.{replacement})", ".Equals(-1)" };
                }

                // Tests equality comparison (for VB the test should edit it)
                yield return new object[] { $"a.{caseChanging}().CompareTo(b)", $"StringComparer.{replacement}.Compare(a, b)", " == -1" };
            }
        }

        public static IEnumerable<object[]> DiagnosedAndFixedStringLiteralsData()
        {
            foreach ((string caseChanging, string replacement) in Cultures)
            {
                foreach (string method in new[] { "Contains", "StartsWith" })
                {
                    yield return new object[] { $"\"aBc\".{caseChanging}().{method}(\"CdE\")", $"\"aBc\".{method}(\"CdE\", StringComparison.{replacement})" };
                }

                // IndexOf overloads
                foreach (string arguments in new[] { "\"CdE\"", "\"CdE\", 1", "\"CdE\", 1, 1" })
                {
                    yield return new object[] { $"\"aBc\".{caseChanging}().IndexOf({arguments})", $"\"aBc\".IndexOf({arguments}, StringComparison.{replacement})" };
                }

                yield return new object[] { $"\"aBc\".{caseChanging}().CompareTo(\"CdE\")", $"StringComparer.{replacement}.Compare(\"aBc\", \"CdE\")" };
            }
        }

        public static IEnumerable<object[]> DiagnosedAndFixedStringReturningMethodsData()
        {
            foreach ((string caseChanging, string replacement) in Cultures)
            {
                foreach (string method in new[] { "Contains", "StartsWith" })
                {
                    yield return new object[] { $"GetStringA().{caseChanging}().{method}(GetStringB())", $"GetStringA().{method}(GetStringB(), StringComparison.{replacement})" };
                }

                // IndexOf overloads
                foreach (string arguments in new[] { "GetStringB()", "GetStringB(), 1", "GetStringB(), 1, 1" })
                {
                    yield return new object[] { $"GetStringA().{caseChanging}().IndexOf({arguments})", $"GetStringA().IndexOf({arguments}, StringComparison.{replacement})" };
                }

                yield return new object[] { $"GetStringA().{caseChanging}().CompareTo(GetStringB())", $"StringComparer.{replacement}.Compare(GetStringA(), GetStringB())" };
            }
        }

        public static IEnumerable<object[]> DiagnosedAndFixedParenthesizedData()
        {
            foreach ((string caseChanging, string replacement) in Cultures)
            {
                foreach (string method in new[] { "Contains", "StartsWith" })
                {
                    yield return new object[] { $"(\"aBc\".{caseChanging}()).{method}(\"CdE\")", $"\"aBc\".{method}(\"CdE\", StringComparison.{replacement})" };
                }

                // IndexOf overloads
                foreach (string arguments in new[] { "\"CdE\"", "\"CdE\", 1", "\"CdE\", 1, 1" })
                {
                    yield return new object[] { $"(\"aBc\".{caseChanging}()).IndexOf({arguments})", $"\"aBc\".IndexOf({arguments}, StringComparison.{replacement})" };
                }

                yield return new object[] { $"(\"aBc\".{caseChanging}()).CompareTo(\"CdE\")", $"StringComparer.{replacement}.Compare(\"aBc\", \"CdE\")" };
            }
        }

        public static IEnumerable<object[]> NoDiagnosticContainsData()
        {
            // Test needs to define a char ch and an object obj
            foreach (string method in new[] { "Contains", "IndexOf", "StartsWith" })
            {
                yield return new object[] { $"\"aBc\".{method}(\"cDe\")" };
                yield return new object[] { $"\"aBc\".{method}(\"cDe\", StringComparison.CurrentCultureIgnoreCase)" };
                yield return new object[] { $"\"aBc\".ToUpper().{method}(\"cDe\", StringComparison.InvariantCulture)" };
                yield return new object[] { $"\"aBc\".{method}(ch)" };
            }

            // StarstWith does not have a (char, StringComparison) overload
            foreach (string method in new[] { "Contains", "IndexOf" })
            {
                yield return new object[] { $"\"aBc\".{method}(ch, StringComparison.Ordinal)" };
                yield return new object[] { $"\"aBc\".ToLowerInvariant().{method}(ch, StringComparison.CurrentCulture)" };
            }

            yield return new object[] { "\"aBc\".CompareTo(obj)" };
            yield return new object[] { "\"aBc\".ToLower().CompareTo(obj)" };
            yield return new object[] { "\"aBc\".CompareTo(\"cDe\")" };
        }
    }
}