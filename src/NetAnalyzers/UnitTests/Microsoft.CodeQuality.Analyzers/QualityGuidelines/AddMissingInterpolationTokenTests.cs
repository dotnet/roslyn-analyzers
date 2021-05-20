// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AddMissingInterpolationTokenAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

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
            await VerifyCS.VerifyAnalyzerAsync(code);
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
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task ContainsNumberedArguments_NoDiagnostic()
        {
            var code = @"
using System;

class Program
{
    public static void Main()
    {
        int x = 5;
        Console.WriteLine(""{x}, {0}"");
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
