// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpAddMissingInterpolationToken,
    Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines.CSharpAddMissingInterpolationTokeFixer>;

namespace Microsoft.CodeQuality.Analyzers.UnitTests.QualityGuidelines
{
    public class AddMissingInterpolationTokenTests
    {
        [Fact]
        public async Task HasValidVariableInScope_Diagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine([|""{x}""|]);
    }
}";

            var fixedCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine($""{x}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task DoesNotHaveValidVariableInScope_NoDiagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine(""{y}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task ContainsOnlyLiterals_NoDiagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        Console.WriteLine(""{0}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [Fact]
        public async Task ContainsLiteralAndBindableExpression_Diagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine([|""{x}, {0}""|]);
    }
}";

            var fixedCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine($""{x}, {0}"");
    }
}";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task ContainsBindableExpression_Diagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine([|""{M(x)}""|]);
    }

    private static string M(int x) => x.ToString();
}";

            var fixedCode = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine($""{M(x)}"");
    }

    private static string M(int x) => x.ToString();
}";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [Fact]
        public async Task ContainsNonBindableExpression_NoDiagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine(""{N(x)}"");
    }

    private static string M(int x) => x.ToString();
}";

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }
    }
}
