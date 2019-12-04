// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.MarkAssembliesWithComVisibleAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.MarkAssembliesWithComVisibleFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class MarkAllAssembliesWithComVisibleTests
    {
        [Fact]
        public async Task NoTypesComVisibleMissing()
        {
            await VerifyCS.VerifyAnalyzerAsync("");
        }

        [Fact]
        public async Task NoTypesComVisibleTrue()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]");
        }

        [Fact]
        public async Task NoTypesComVisibleFalse()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]");
        }

        [Fact]
        public async Task PublicTypeComVisibleMissing()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class C
{
}",
                GetAddComVisibleFalseResult());
        }

        [Fact]
        public async Task PublicTypeComVisibleTrue()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]

public class C
{
}",
                GetExposeIndividualTypesResult());
        }

        [Fact]
        public async Task PublicTypeComVisibleFalse()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

public class C
{
}");
        }

        [Fact]
        public async Task InternalTypeComVisibleMissing()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
internal class C
{
}");
        }

        [Fact]
        public async Task InternalTypeComVisibleTrue()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]

internal class C
{
}");
        }

        [Fact]
        public async Task InternalTypeComVisibleFalse()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

internal class C
{
}");
        }

        private static DiagnosticResult GetExposeIndividualTypesResult()
            => VerifyCS.Diagnostic(MarkAssembliesWithComVisibleAnalyzer.RuleA)
                .WithArguments("TestProject");

        private static DiagnosticResult GetAddComVisibleFalseResult()
            => VerifyCS.Diagnostic(MarkAssembliesWithComVisibleAnalyzer.RuleB)
                .WithArguments("TestProject");
    }
}
