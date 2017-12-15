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
        public void CSharp_CancellationTokenOverloadAvailable()
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
            VerifyCSharp(source, GetCSharpResultAt(11, 38, "p1"));
        }

        [Fact]
        public void CSharp_OptionalCancellationTokenParameter()
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
            VerifyCSharp(source, GetCSharpResultAt(9, 38, "p1"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1476")]
        public void CSharp_UninitializedCancellationTokenOutParameter_NoDiagnostics()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1 = default(CancellationToken)) => throw null;

    Task M2(out CancellationToken p1)
    {
        var l1 = M1();
        p1 = default(CancellationToken);
        return l1;
    }
}
";
            VerifyCSharp(source);
        }

        [Fact]
        public void CSharp_DefaultCancellationTokenPassed()
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
            VerifyCSharp(source, GetCSharpResultAt(9, 38, "p1"));
        }

        [Fact]
        public void CSharp_CancellationTokenNonePassed()
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
            VerifyCSharp(source, GetCSharpResultAt(9, 38, "p1"));
        }

        [Fact]
        public void CSharp_CancellationTokenInScope_InstanceField_NoDiagnostics()
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
            VerifyCSharp(source);
        }

        [Fact]
        public void CSharp_CancellationTokenInScope_StaticField_NoDiagnostics()
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
            VerifyCSharp(source);
        }

        [Fact]
        public void CSharp_CancellationTokenInScope_LocalVariable()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1() => throw null;

    Task M1(CancellationToken p1) => throw null;

    Task M2()
    {
        var l1 = CancellationToken.None; // No dataflow
        return M1();
    }
}
";
            VerifyCSharp(source, GetCSharpResultAt(14, 16, "l1"));
        }

        [Fact]
        public void CSharp_CancellationTokenIsPropagated_NoDiagnostics()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1() => throw null;

    Task M1(CancellationToken p1) => throw null;

    Task M2(CancellationToken p1) => M1(p1);
}
";
            VerifyCSharp(source);
        }

        [Fact]
        public void CSharp_NoCancellationTokenOverloadAvailable_NoDiagnostics()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1() => throw null;

    Task M2(CancellationToken p1) => M1();
}
";
            VerifyCSharp(source);
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1476")]
        public void CSharp_CancellationTokenInScope_UninitializedLocalVariable_NoDiagnostics()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1 = default(CancellationToken)) => throw null;

    Task M2()
    {
        CancellationToken l1;
        var l2 = M1();
        return l2;
    }
}
";
            VerifyCSharp(source);
        }

        [Fact]
        public void CSharp_CancellationTokenInScope_LocalVariableDeclaredAfterwards_NoDiagnostics()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1 = default(CancellationToken)) => throw null;

    Task M2()
    {
        var l1 = M1();
        var l2 = CancellationToken.None;
        return l1;
    }
}
";
            VerifyCSharp(source);
        }

        [Fact]
        public void CSharp_RequiredCancellationTokenParameterIsNotPassed_NoCrash()
        {
            var source = @"
using System.Threading;
using System.Threading.Tasks;

class C
{
    Task M1(CancellationToken p1) => throw null;

    Task M2() => M1();
}
";
            VerifyCSharp(source, TestValidationMode.AllowCompileErrors);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicPropagateCancellationTokensWhenPossibleAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpPropagateCancellationTokensWhenPossibleAnalyzer();
        }

        private DiagnosticResult GetBasicResultAt(int line, int column, string variableName)
        {
            return GetBasicResultAt(line, column, PropagateCancellationTokensWhenPossibleAnalyzer.Rule, $"'{variableName}'");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string variableName)
        {
            return GetCSharpResultAt(line, column, PropagateCancellationTokensWhenPossibleAnalyzer.Rule, $"'{variableName}'");
        }
    }
}