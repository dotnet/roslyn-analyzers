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
        [InlineData("nint", "0")]
        [InlineData("int", "(int)-0.0")]
        [InlineData("object", "null")]
        [InlineData("string", "null")]
        [InlineData("int?", "null")]
        [InlineData("int", "default")]
        [InlineData("int?", "default")]
        [InlineData("DateTime", "new DateTime()")]
        [InlineData("DateTime", "default")]
        [InlineData("DateTime", "default(DateTime)")]
        [InlineData("DayOfWeek", "DayOfWeek.Sunday")]
        [InlineData("DayOfWeek", "(DayOfWeek)0")]
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
        [InlineData("int?", "default(int)")]
        [InlineData("DateTime?", "new DateTime()")]
        [InlineData("DateTime?", "default(DateTime)")]
        [InlineData("DateTimeOffset", "new DateTime()")]
        [InlineData("DateTimeOffset", "default(DateTime)")]
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

        [Fact]
        public async Task TestGeneric_Unconstrained()
        {
            string source = @"
using System;

class C
{
    void M<T>(Span<T> span)
    {
        [|span.Fill(default)|];
    }
}
";
            await TestCS(source);
        }

        [Fact]
        public async Task TestGeneric_Reference_Null()
        {
            string source = @"
#nullable enable

using System;

class C
{
    void M<T>(Span<T?> span) where T : class
    {
        [|span.Fill(null)|];
    }
}
";
            await TestCS(source);
        }

        [Fact]
        public async Task TestGeneric_ValueType_Null()
        {
            string source = @"
using System;

class C
{
    void M<T>(Span<T?> span) where T : struct
    {
        [|span.Fill(null)|];
    }
}
";
            await TestCS(source);
        }

        [Fact]
        public async Task TestGeneric_Reference_DefaultT()
        {
            string source = @"
#nullable enable

using System;

class C
{
    void M<T>(Span<T?> span) where T : class
    {
        [|span.Fill(default(T))|];
    }
}
";
            await TestCS(source);
        }

        [Fact]
        public async Task TestGeneric_ValueType_DefaultT()
        {
            string source = @"
using System;

class C
{
    void M<T>(Span<T?> span) where T : struct
    {
        span.Fill(default(T));
    }
}
";
            await TestCS(source);
        }

        [Fact]
        public async Task TestCustomConversion()
        {

            string source = @"
using System;

struct S
{
    public static explicit operator S(int value) => throw null;
    public static explicit operator int(S value) => throw null;
}

class C
{
    void M(Span<S> span)
    {
        span.Fill((S)0);
    }

    void M(Span<int> span)
    {
        span.Fill((int)(S)0);
    }
}
";
            await TestCS(source);
        }

        private static Task TestCS(string source, string corrected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                LanguageVersion = LanguageVersion.Preview,
                FixedCode = corrected,
            };

            return test.RunAsync();
        }

        private static Task TestCS(string source)
        {
            var test = new VerifyCS.Test
            {
                TestCode = source,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
                LanguageVersion = LanguageVersion.Preview,
            };

            return test.RunAsync();
        }
    }
}
