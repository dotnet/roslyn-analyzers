// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class PropagateCancellationTokensWhenPossibleTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void WipCSharp_CancellationTokenOverloadAvailable()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1() => throw null;

    Task M1(CancellationToken p1) => throw null;

    Task M2(CancellationToken p1) => M1();
}
";
            VerifyCSharp(source, GetCSharpResultAt(11, 38));
        }

        [Fact]
        public void WipCSharp_OptionalCancellationTokenParameter()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1 = default(CancellationToken)) => throw null;

    Task M2(CancellationToken p1) => M1();
}
";
            VerifyCSharp(source, GetCSharpResultAt(9, 38));
        }

        [Fact]
        public void WipCSharp_DefaultCancellationTokenPassed()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1) => throw null;

    Task M2(CancellationToken p1) => M1(default(CancellationToken));
}
";
            VerifyCSharp(source, GetCSharpResultAt(9, 38));
        }

        [Fact]
        public void WipCSharp_CancellationTokenNonePassed()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1) => throw null;

    Task M2(CancellationToken p1) => M1(CancellationToken.None);
}
";
            VerifyCSharp(source, GetCSharpResultAt(9, 38));
        }

        [Fact]
        public void WipCSharp_InstanceCancellationTokenFieldInScope()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    private CancellationToken _f1;

    Task M1() => throw null;

    Task M1(CancellationToken p1) => throw null;

    Task M2() => M1();
}
";
            VerifyCSharp(source, GetCSharpResultAt(13, 18));
        }

        [Fact]
        public void WipCSharp_StaticCancellationTokenFieldInScope()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    private static CancellationToken _s1;

    Task M1() => throw null;

    Task M1(CancellationToken p1) => throw null;

    Task M2() => M1();
}
";
            VerifyCSharp(source, GetCSharpResultAt(13, 18));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicPropagateCancellationTokensWhenPossibleAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpPropagateCancellationTokensWhenPossibleAnalyzer();
        }

        private DiagnosticResult GetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, PropagateCancellationTokensWhenPossibleAnalyzer.Rule);
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, PropagateCancellationTokensWhenPossibleAnalyzer.Rule);
        }
    }
}