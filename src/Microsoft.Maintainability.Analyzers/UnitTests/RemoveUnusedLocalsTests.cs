// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class RemoveUnusedLocalsTests : DiagnosticAnalyzerTestBase
    {
        private const string CA1804RuleId = "CA1804";
        private readonly string _CA1804RemoveUnusedLocalMessage = MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRemoveUnusedLocalsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void UnusedLocal_NoDiagnostics_VisualBasic()
        {
            VerifyBasic(@"
Public Class Tester
    Dim aa As Integer
    Public Sub Testing()
        Dim b as Integer
        Dim c as Integer
        b += 1
        c = 0
        aa = c
    End Sub
End Class");
        }

        [Fact]
        public void UnusedLocal_NoDiagnostics_LocalsCalledInInvocationExpression_VisualBasic()
        {
            VerifyBasic(@"
Public Class Tester
    Public Sub Testing()
        Dim c as Integer
        c = 0
        System.Console.WriteLine(c)
    End Sub

    Public Sub TakeArg(arg As Integer)
    End Sub
End Class");
        }

        [Fact]
        public void UnusedLocal_VisualBasic()
        {
            VerifyBasic(@"
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
End Class", 
            GetBasicResultAt(6, 13, CA1804RuleId, _CA1804RemoveUnusedLocalMessage),
            GetBasicResultAt(8, 13, CA1804RuleId, _CA1804RemoveUnusedLocalMessage),
            GetBasicResultAt(9, 40, CA1804RuleId, _CA1804RemoveUnusedLocalMessage));
        }
    }
}