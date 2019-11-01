﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class CancellationTokenParametersMustComeLast : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void NoDiagnosticInEmptyFile()
        {
            var test = @"";

            VerifyCSharp(test);
        }

        [Fact]
        public void DiagnosticForMethod()
        {
            var source = @"
using System.Threading;
class T
{
    void M(CancellationToken t, int i)
    {
    }
}";
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("T.M(System.Threading.CancellationToken, int)");
            VerifyCSharp(source, expected);
        }

        [Fact]
        public void DiagnosticWhenFirstAndLastByOtherInBetween()
        {
            var source = @"
using System.Threading;
class T
{
    void M(CancellationToken t1, int i, CancellationToken t2)
    {
    }
}";
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("T.M(System.Threading.CancellationToken, int, System.Threading.CancellationToken)");
            VerifyCSharp(source, expected);
        }

        [Fact]
        public void NoDiagnosticWhenLastParam()
        {
            var test = @"
using System.Threading;
class T
{
    void M(int i, CancellationToken t)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenOnlyParam()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenParamsComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, params object[] args)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenOutComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, out int i)
    {
        i = 2;
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenRefComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, ref int x, ref int y)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticWhenOptionalParameterComesAfterNonOptionalCancellationToken()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, int x = 0)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void NoDiagnosticOnOverride()
        {
            var test = @"
using System.Threading;
class B
{
    protected virtual void M(CancellationToken t, int i) { }
}

class T : B
{
    protected override void M(CancellationToken t, int i) { }
}";

            // One diagnostic for the virtual, but none for the override.
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 28).WithArguments("B.M(System.Threading.CancellationToken, int)");
            VerifyCSharp(test, expected);
        }

        [Fact]
        public void NoDiagnosticOnImplicitInterfaceImplementation()
        {
            var test = @"
using System.Threading;
interface I
{
    void M(CancellationToken t, int i);
}

class T : I
{
    public void M(CancellationToken t, int i) { }
}";

            // One diagnostic for the interface, but none for the implementation.
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("I.M(System.Threading.CancellationToken, int)");
            VerifyCSharp(test, expected);
        }

        [Fact]
        public void NoDiagnosticOnExplicitInterfaceImplementation()
        {
            var test = @"
using System.Threading;
interface I
{
    void M(CancellationToken t, int i);
}

class T : I
{
    void I.M(CancellationToken t, int i) { }
}";

            // One diagnostic for the interface, but none for the implementation.
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("I.M(System.Threading.CancellationToken, int)");
            VerifyCSharp(test, expected);
        }

        [Fact, WorkItem(1491, "https://github.com/dotnet/roslyn-analyzers/issues/1491")]
        public void NoDiagnosticOnCancellationTokenExtensionMethod()
        {
            var test = @"
using System.Threading;
static class C1
{
    public static void M1(this CancellationToken p1, object p2)
    {
    }
}";
            VerifyCSharp(test);
        }

        [Fact, WorkItem(1816, "https://github.com/dotnet/roslyn-analyzers/issues/1816")]
        public void NoDiagnosticWhenMultipleAtEndOfParameterList()
        {
            var test = @"
using System.Threading;
static class C1
{
    public static void M1(object p1, CancellationToken token1, CancellationToken token2) { }
    public static void M2(object p1, CancellationToken token1, CancellationToken token2, CancellationToken token3) { }
    public static void M3(CancellationToken token1, CancellationToken token2, CancellationToken token3) { }
    public static void M4(CancellationToken token1, CancellationToken token2 = default(CancellationToken)) { }
    public static void M5(CancellationToken token1 = default(CancellationToken), CancellationToken token2 = default(CancellationToken)) { }
}";
            VerifyCSharp(test);
        }

        [Fact]
        public void DiagnosticOnExtensionMethodWhenCancellationTokenIsNotFirstParameter()
        {
            var test = @"
using System.Threading;
static class C1
{
    public static void M1(this object p1, CancellationToken p2, object p3)
    {
    }
}";

            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 24).WithArguments("C1.M1(object, System.Threading.CancellationToken, object)");
            VerifyCSharp(test, expected);
        }

        [Fact, WorkItem(2281, "https://github.com/dotnet/roslyn-analyzers/issues/2281")]
        public void CA1068_DoNotReportOnIProgressLastAndCancellationTokenBeforeLast()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Threading.Tasks;

public class C
{
    public Task SomeAsync(object o, CancellationToken cancellationToken, IProgress<int> progress)
    {
        throw new NotImplementedException();
    }
}");

            VerifyBasic(@"
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class C
    Public Function SomeAsync(ByVal o As Object, ByVal cancellationToken As CancellationToken, ByVal progress As IProgress(Of Integer)) As Task
        Throw New NotImplementedException()
    End Function
End Class");
        }

        [Fact, WorkItem(2281, "https://github.com/dotnet/roslyn-analyzers/issues/2281")]
        public void CA1068_ReportOnIProgressLastAndCancellationTokenNotBeforeLast()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Threading.Tasks;

public class C
{
    public Task SomeAsync(CancellationToken cancellationToken, object o, IProgress<int> progress)
    {
        throw new NotImplementedException();
    }
}",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(8, 17)
                .WithArguments("C.SomeAsync(System.Threading.CancellationToken, object, System.IProgress<int>)"));

            VerifyBasic(@"
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class C
    Public Function SomeAsync(ByVal cancellationToken As CancellationToken, ByVal o As Object, ByVal progress As IProgress(Of Integer)) As Task
        Throw New NotImplementedException()
    End Function
End Class",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(7, 21)
                .WithArguments("Public Function SomeAsync(cancellationToken As System.Threading.CancellationToken, o As Object, progress As System.IProgress(Of Integer)) As System.Threading.Tasks.Task"));
        }

        [Fact, WorkItem(2281, "https://github.com/dotnet/roslyn-analyzers/issues/2281")]
        public void CA1068_OnlyExcludeOneIProgressAtTheEnd()
        {
            VerifyCSharp(@"
using System;
using System.Threading;
using System.Threading.Tasks;

public class C
{
    public Task SomeAsync(CancellationToken cancellationToken, IProgress<int> progress1, IProgress<int> progress2)
    {
        throw new NotImplementedException();
    }
}",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(8, 17)
                .WithArguments("C.SomeAsync(System.Threading.CancellationToken, System.IProgress<int>, System.IProgress<int>)"));

            VerifyBasic(@"
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class C
    Public Function SomeAsync(ByVal cancellationToken As CancellationToken, ByVal progress1 As IProgress(Of Integer), ByVal progress2 As IProgress(Of Integer)) As Task
        Throw New NotImplementedException()
    End Function
End Class",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(7, 21)
                .WithArguments("Public Function SomeAsync(cancellationToken As System.Threading.CancellationToken, progress1 As System.IProgress(Of Integer), progress2 As System.IProgress(Of Integer)) As System.Threading.Tasks.Task"));
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CancellationTokenParametersMustComeLastAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new CancellationTokenParametersMustComeLastAnalyzer();
        }
    }
}
