// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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

        [Fact]
        public void UnusedLocal_CSharp()
        {
            var code = @"
using System;

public class Tester
{
    public void Testing()
    {
        int c;
        c = 0;
        void lambda() 
        {
            var lambdaA = 0;
            lambdaA = 3;
        }

        var rateIt = 3;
        double debitIt = 4;
        Calculate(rateIt, ref debitIt);
    }


    void Calculate(double rate, ref double debt)
    {
        debt = debt + (debt * rate / 100);
    }
}
";

            var fix = @"
using System;

public class Tester
{
    public void Testing()
    {
        var rateIt = 3;
        double debitIt = 4;
        Calculate(rateIt, ref debitIt);
    }


    void Calculate(double rate, ref double debt)
    {
        debt = debt + (debt * rate / 100);
    }
}";
            VerifyCSharpFix(code, fix);
        }

        [Fact]
        public void UnusedLocal_VisualBasic()
        {
            var code = @"
Imports System

Public Class Tester
    Public Sub Testing()
        Dim c as Integer
        c = 0
        Dim lambda As Action = Sub()
                                   Dim lambdaA = 0
                                   lambdaA = 3
                               End Sub
        Dim rateIt = 3
        Dim debitIt = 4
        Calculate(rateIt, debitIt)
    End Sub

    Sub Calculate(ByVal rate As Double, ByRef debt As Double)
        debt = debt + (debt * rate / 100)
    End Sub
End Class";

            var fix = @"
Imports System

Public Class Tester
    Public Sub Testing()
        Dim rateIt = 3
        Dim debitIt = 4
        Calculate(rateIt, debitIt)
    End Sub

    Sub Calculate(ByVal rate As Double, ByRef debt As Double)
        debt = debt + (debt * rate / 100)
    End Sub
End Class";
            VerifyBasicFix(code, fix);
        }
    }
}