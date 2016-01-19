// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class RemoveUnusedLocalsTests : DiagnosticAnalyzerTestBase
    {
        private const string CA1804RuleId = RemoveUnusedLocalsAnalyzer.RuleId;
        private readonly string _CA1804RemoveUnusedLocalMessage = MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicRemoveUnusedLocalsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpRemoveUnusedLocalsAnalyzer();
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
Public Class Tester
    Public Sub Testing()
        Dim c as Integer
        c = 0
    End Sub
End Class", 
            GetBasicResultAt(4, 13, CA1804RuleId, _CA1804RemoveUnusedLocalMessage));
        }


    }
}