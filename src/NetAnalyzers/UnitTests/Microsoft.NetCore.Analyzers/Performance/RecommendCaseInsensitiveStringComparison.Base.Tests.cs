// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System;
using System.Linq;

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
                foreach (string method in new[] { "Contains", "IndexOf", "StartsWith" })
                {
                    yield return new object[] { $"a.{caseChanging}().{method}(b)", $"a.{method}(b, StringComparison.{replacement})" };
                }

                yield return new object[] { $"a.{caseChanging}().CompareTo(b)", $"StringComparer.{replacement}.Compare(a, b)" };
            }
        }

        public static IEnumerable<object[]> DiagnosedAndFixedWithEqualsToData()
        {
            foreach ((string caseChanging, string replacement) in Cultures)
            {
                foreach (string method in new[] { "Contains", "StartsWith" })
                {
                    yield return new object[] { $"a.{caseChanging}().{method}(b)", $"a.{method}(b, StringComparison.{replacement})", "" };
                }

                yield return new object[] { $"a.{caseChanging}().IndexOf(b)", $"a.IndexOf(b, StringComparison.{replacement})", " != -1" };
                yield return new object[] { $"a.{caseChanging}().CompareTo(b)", $"StringComparer.{replacement}.Compare(a, b)", " != -1" };
            }
        }
    }
}