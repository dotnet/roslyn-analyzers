// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.Maintainability;
using Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Maintainability.UnitTests
{
    public class RemoveUnusedLocalsFixerTests : CodeFixTestBase
    {
        private const string CA1804RuleId = "CA1804";

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRemoveUnusedLocalsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return null;
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicRemoveUnusedLocalsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpRemoveUnusedLocalsFixer();
        }

        private const string CSharpOriginalCode = @"
using System;

public class Tester
{
    public void Testing()
    {
        double localRate; // inline comment also to be deleted.
        int c;
        c = 0;
        void localFunction()
        {
            var lambdaA = 0;
            lambdaA = 3;
        }

        var rateIt = 3;
        unsafe
        {
            int* p;
            p = & rateIt;
        }

        double debitIt = 4;
        Calculate(rateIt, ref debitIt);
    }

    void Calculate(double rate, ref double debt)
    {
        double GetRate(double rateParam)
        {
            double rateIt;
            int AnotherLocal(int anotherRateParam) => 1;
            return rateParam;
        }

        int a = 2, b = 100;
        double c, localRate;
        localRate = GetRate(rate);
        Func<int> lambda = () =>
        {
            int bb = 4;
            Func<int> internalLambda = () => { int bbb = 4; return 2; };
            return 1;
        };
        debt = debt + (debt * localRate / b);
    }
}
";
        private const string CSharpFix = @"
using System;

public class Tester
{
    public void Testing()
    {
        var rateIt = 3;
        unsafe
        {
            int* p;
            p = & rateIt;
        }

        double debitIt = 4;
        Calculate(rateIt, ref debitIt);
    }

    void Calculate(double rate, ref double debt)
    {
        double GetRate(double rateParam)
        {
            return rateParam;
        }

        int b = 100;
        double localRate;
        localRate = GetRate(rate);
        Func<int> lambda = () =>
        {
            Func<int> internalLambda = () => { return 2; };
            return 1;
        };
        debt = debt + (debt * localRate / b);
    }
}
";

        private const string BasicOriginalCode = @"
Imports System

Public Class Tester

    Public Sub Testing()
        Dim localRate As Double ' inline comment also to be deleted. 
        Dim c As Integer
        c = 0
        Dim rateIt = 3
        Dim debitIt As Double = 4
        Calculate(rateIt, debitIt)
    End Sub

    Sub Calculate(rate As Double, ByRef debt As Double)
        Dim a As Integer = 2, b As Integer = 100
        Dim c As Double, localRate As Double
        localRate = rate
        Dim lambda As Func(Of Integer) = Function()
            Dim bb As Integer = 4
            Dim internalLambda As Func(Of Integer) = Function()
                Dim bbb As Integer = 4
                Return 2
            End Function
            Return 1
        End Function
        debt = debt + (debt * localRate / b * lambda())
    End Sub
End Class
";

        private const string BasicFix = @"
Imports System

Public Class Tester

    Public Sub Testing()
        Dim rateIt = 3
        Dim debitIt As Double = 4
        Calculate(rateIt, debitIt)
    End Sub

    Sub Calculate(rate As Double, ByRef debt As Double)
        Dim b As Integer = 100
        Dim localRate As Double
        localRate = rate
        Dim lambda As Func(Of Integer) = Function()
                                             Return 1
        End Function
        debt = debt + (debt * localRate / b * lambda())
    End Sub
End Class
";
        
        [Fact, WorkItem(22921, "https://github.com/dotnet/roslyn/issues/22921")]
        public void UnusedLocal_CSharp()
        {
            VerifyCSharpFix(CSharpOriginalCode, CSharpFix, allowNewCompilerDiagnostics: true);
        }

        [Fact, WorkItem(22921, "https://github.com/dotnet/roslyn/issues/22921")]
        public void UnusedLocal_FixAll_CSharp()
        {
            VerifyCSharpFixAll(CSharpOriginalCode, CSharpFix, allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void UnusedLocal_VisualBasic()
        {
            VerifyBasicFix(BasicOriginalCode, BasicFix, allowNewCompilerDiagnostics: true);
        }


        [Fact]
        public void UnusedLocal_FixAll_VisualBasic()
        {
            VerifyBasicFixAll(BasicOriginalCode, BasicFix, allowNewCompilerDiagnostics: true);
        }
    }
}