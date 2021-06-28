// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UseSpanClearInsteadOfFillAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpUseSpanClearInsteadOfFillFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class UseSpanClearInsteadOfFillTests
    {
        [Fact]
        public async Task TestCodeFix()
        {
            string source = @"
using System;

class C
{
    void M(Span<byte> span)
    {
        [|span.Fill(0)|];
    }
}
";
            string expected = @"
using System;

class C
{
    void M(Span<byte> span)
    {
        span.Clear();
    }
}
";
            await TestCS(source, expected);
        }

        [Theory]
        [InlineData("int", "0")]
        [InlineData("int", "1 - 1")]
        [InlineData("long", "0")]
        [InlineData("double", "0")]
        [InlineData("double", "0.0")]
        [InlineData("object", "null")]
        [InlineData("string", "null")]
        [InlineData("int?", "null")]
        public async Task TestDefaultValue(string type, string value)
        {
            string source = $@"
using System;

class C
{{
    void M(Span<{type}> span)
    {{
        [|span.Fill({value})|];
    }}
}}
";
            await TestCS(source);
        }

        [Theory]
        [InlineData("int", "1")]
        [InlineData("float", "-0.0f")]
        [InlineData("double", "-0.0")]
        [InlineData("decimal", "-0.0m")]
        [InlineData("string", "\"\"")]
        [InlineData("int?", "0")]
        public async Task TestNonDefaultValue(string type, string value)
        {
            string source = $@"
using System;

class C
{{
    void M(Span<{type}> span)
    {{
        span.Fill({value});
    }}
}}
";
            await TestCS(source);
        }

        private static Task TestCS(string source, string corrected, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                LanguageVersion = LanguageVersion.Preview,
                FixedCode = corrected,
            };

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        private static Task TestCS(string source, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                LanguageVersion = LanguageVersion.Preview,
            };

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }
    }
}
