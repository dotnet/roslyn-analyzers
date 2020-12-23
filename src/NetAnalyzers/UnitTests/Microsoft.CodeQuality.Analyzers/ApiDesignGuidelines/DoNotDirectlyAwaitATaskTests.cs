// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotDirectlyAwaitATaskTests
    {
        [Theory, WorkItem(1953, "https://github.com/dotnet/roslyn-analyzers/issues/1953")]
        [InlineData("dotnet_code_quality.exclude_async_void_methods = true")]
        [InlineData("dotnet_code_quality.CA2007.exclude_async_void_methods = true")]
        public async Task CSharpAsyncVoidMethod_AnalyzerOption_NoDiagnostic(string editorConfigText)
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    private Task t;
    public async void M()
    {
        await M1Async();
    }

    private async Task M1Async()
    {
        await t.ConfigureAwait(false);
    }
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            }.RunAsync();
        }

        [Theory, WorkItem(1953, "https://github.com/dotnet/roslyn-analyzers/issues/1953")]
        [InlineData("dotnet_code_quality.exclude_async_void_methods = false")]
        [InlineData("dotnet_code_quality.CA2007.exclude_async_void_methods = false")]
        public async Task CSharpAsyncVoidMethod_AnalyzerOption_Diagnostic(string editorConfigText)
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    private Task t;
    public async void M()
    {
        await M1Async();
    }

    private async Task M1Async()
    {
        await t.ConfigureAwait(false);
    }
}";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
                ExpectedDiagnostics = { GetCSharpResultAt(9, 15) }
            }.RunAsync();
        }

        [Theory, WorkItem(1953, "https://github.com/dotnet/roslyn-analyzers/issues/1953")]
        [InlineData("", true)]
        [InlineData("dotnet_code_quality.output_kind = ConsoleApplication", false)]
        [InlineData("dotnet_code_quality.CA2007.output_kind = ConsoleApplication, WindowsApplication", false)]
        [InlineData("dotnet_code_quality.output_kind = DynamicallyLinkedLibrary", true)]
        [InlineData("dotnet_code_quality.CA2007.output_kind = ConsoleApplication, DynamicallyLinkedLibrary", true)]
        public async Task CSharpSimpleAwaitTask_AnalyzerOption_OutputKind(string editorConfigText, bool isExpectingDiagnostic)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                         @"
using System.Threading.Tasks;

public class C
{
    public async Task M()
    {
        Task t = null;
        await t;
    }
}
"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };

            if (isExpectingDiagnostic)
            {
                csharpTest.ExpectedDiagnostics.Add(GetCSharpResultAt(9, 15));
            }

            await csharpTest.RunAsync();
        }

        [Fact, WorkItem(2393, "https://github.com/dotnet/roslyn-analyzers/issues/2393")]
        public async Task CSharpSimpleAwaitTaskInLocalFunction()
        {
            var code = @"
using System.Threading.Tasks;

public class C
{
    public void M()
    {
        async Task CoreAsync()
        {
            Task t = null;
            await t;
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, GetCSharpResultAt(11, 19));
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);

        private static DiagnosticResult GetBasicResultAt(int line, int column)
            => VerifyVB.Diagnostic()
                .WithLocation(line, column);
    }
}