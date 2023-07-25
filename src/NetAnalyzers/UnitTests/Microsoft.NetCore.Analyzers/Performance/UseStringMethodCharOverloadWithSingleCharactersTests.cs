// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.CSharp.NetAnalyzers.Microsoft.NetCore.Analyzers.Performance.CSharpUseStringMethodCharOverloadWithSingleCharacters,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpUseStringMethodCharOverloadWithSingleCharactersFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicUseStringMethodCharOverloadWithSingleCharacters,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicUseStringMethodCharOverloadWithSingleCharactersFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class UseStringMethodCharOverloadWithSingleCharactersTests
    {
        public static IEnumerable<object[]> GetMethods()
        {
            yield return new object[] { nameof(string.StartsWith) };
            yield return new object[] { nameof(string.EndsWith) };
            yield return new object[] { nameof(string.IndexOf) };
            yield return new object[] { nameof(string.LastIndexOf) };
        }

        public static IEnumerable<object[]> GetStartsEndsWithMethods()
        {
            yield return new object[] { nameof(string.StartsWith) };
            yield return new object[] { nameof(string.EndsWith) };
        }

        public static IEnumerable<object[]> GetIndexLastIndexOfMethods()
        {
            yield return new object[] { nameof(string.IndexOf) };
            yield return new object[] { nameof(string.LastIndexOf) };
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task CS_NotSingleChar(string method)
        {
            var testCode = $$"""
                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}("abc");
                    }
                }
                """;

            await VerifyCSAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task CS_RegularStringLiteral(string method)
        {
            var testCode = $$"""
                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}{|CA1866:("a")|};
                    }
                }
                """;

            await VerifyCSAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task CS_StringComparisonOrdinal(string method)
        {
            var testCode = $$"""
                using System;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}{|CA1865:("a", StringComparison.Ordinal)|};
                    }
                }
                """;

            var fixedCode = $$"""
                using System;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}('a');
                    }
                }
                """;

            await VerifyCSAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21, fixedCode);
        }

        [Theory]
        [MemberData(nameof(GetStartsEndsWithMethods))]
        public async Task CS_InvariantCultureAndAsciiChar(string method)
        {
            var testCode = $$"""
                using System.Globalization;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}{|CA1865:("a", false, CultureInfo.InvariantCulture)|};
                    }
                }
                """;

            var fixedCode = $$"""
                using System.Globalization;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}('a');
                    }
                }
                """;

            await VerifyCSAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21, fixedCode);
        }

        [Theory]
        [MemberData(nameof(GetStartsEndsWithMethods))]
        public async Task CS_InvariantCultureAndNonAsciiChar(string method)
        {
            var testCode = $$"""
                using System.Globalization;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}{|CA1866:("あ", false, CultureInfo.InvariantCulture)|};
                    }
                }
                """;

            await VerifyCSAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task CS_StringComparisonAnythingElse(string method)
        {
            var testCode = $$"""
                using System;

                public class TestClass
                {
                    public void TestMethod()
                    {
                        "test".{{method}}{|CA1867:("a", StringComparison.CurrentCulture)|};
                    }
                }
                """;

            await VerifyCSAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task VB_NotSingleChar(string method)
        {
            var testCode = $$"""
                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}("abc")
                    End Sub
                End Class
                """;

            await VerifyVBAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task VB_RegularStringLiteral(string method)
        {
            var testCode = $$"""
                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}{|CA1866:("a")|}
                    End Sub
                End Class
                """;

            await VerifyVBAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task VB_StringComparisonOrdinal(string method)
        {
            var testCode = $$"""
                Imports System

                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}{|CA1865:("a", StringComparison.Ordinal)|}
                    End Sub
                End Class
                """;

            var fixedCode = $$"""
                Imports System

                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}("a"c)
                    End Sub
                End Class
                """;

            await VerifyVBAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21, fixedCode);
        }

        [Theory]
        [MemberData(nameof(GetStartsEndsWithMethods))]
        public async Task VB_InvariantCultureAndAsciiChar(string method)
        {
            var testCode = $$"""
                Imports System.Globalization

                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}{|CA1865:("a", false, CultureInfo.InvariantCulture)|}
                    End Sub
                End Class
                """;

            var fixedCode = $$"""
                Imports System.Globalization

                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}{|CA1865:("a"c)|}
                    End Sub
                End Class
                """;

            await VerifyVBAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21, fixedCode);
        }

        [Theory]
        [MemberData(nameof(GetStartsEndsWithMethods))]
        public async Task VB_InvariantCultureAndNonAsciiChar(string method)
        {
            var testCode = $$"""
                Imports System.Globalization

                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}{|CA1866:("あ", false, CultureInfo.InvariantCulture)|}
                    End Sub
                End Class
                """;

            await VerifyVBAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        [Theory]
        [MemberData(nameof(GetMethods))]
        public async Task VB_StringComparisonAnythingElse(string method)
        {
            var testCode = $$"""
                Imports System

                Public Class TestClass
                    Public Sub TestMethod()
                        Dim a = "test".{{method}}{|CA1867:("a", StringComparison.CurrentCulture)|}
                    End Sub
                End Class
                """;

            await VerifyVBAsync(testCode, ReferenceAssemblies.NetStandard.NetStandard21);
        }

        private static async Task VerifyCSAsync(string source, ReferenceAssemblies referenceAssemblies, string fixedSource = null)
        {
            await new VerifyCS.Test()
            {
                TestCode = source,
                ReferenceAssemblies = referenceAssemblies,
                FixedCode = fixedSource,
            }.RunAsync();
        }

        private static async Task VerifyVBAsync(string source, ReferenceAssemblies referenceAssemblies, string fixedSource = null)
        {
            await new VerifyVB.Test()
            {
                TestCode = source,
                ReferenceAssemblies = referenceAssemblies,
                FixedCode = fixedSource,
            }.RunAsync();
        }
    }
}
