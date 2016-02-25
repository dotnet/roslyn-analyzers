// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.Maintainability.Analyzers.UnitTests
{
    public class AvoidUnusedPrivateFieldsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new AvoidUnusedPrivateFieldsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AvoidUnusedPrivateFieldsAnalyzer();
        }

        [Fact]
        public void CA1823_CSharp_DiagnosticCases()
        {
            VerifyCSharp(@"
public class Class
{
    private string fileName = ""data.txt"";
    private int Used1 = 10;
    private int Used2;
    private int Unused1 = 20;
    private int Unused2;
    public int Unused3;

    public string FileName()
    {
        return fileName;
    }

    private int Value => Used1 + Used2;
}
",
            GetCA1823CSharpResultAt(7, 17, "Unused1"),
            GetCA1823CSharpResultAt(8, 17, "Unused2"));
        }

        [Fact]
        public void CA1823_VisualBasic_DiagnosticCases()
        {
            VerifyBasic(@"
Public Class Class1
	Private fileName As String
    Private Used1 As Integer = 10
    Private Used2 As Integer
    Private Unused1 As Integer = 20
    Private Unused2 As Integer
    Public Unused3 As Integer

    Public Function MyFileName() As String
        Return filename
    End Function

    Public Property MyValue As Integer
        Get
            Return Used1 + Used2
        End Get
    End Function
End Class
",
            GetCA1823BasicResultAt(6, 13, "Unused1"),
            GetCA1823BasicResultAt(7, 13, "Unused2"));
        }

        private static DiagnosticResult GetCA1823CSharpResultAt(int line, int column, string fieldName)
        {
            return GetCSharpResultAt(line, column, AvoidUnusedPrivateFieldsAnalyzer.RuleId, string.Format(MicrosoftMaintainabilityAnalyzersResources.AvoidUnusedPrivateFieldsMessage, fieldName));
        }

        private static DiagnosticResult GetCA1823BasicResultAt(int line, int column, string fieldName)
        {
            return GetBasicResultAt(line, column, AvoidUnusedPrivateFieldsAnalyzer.RuleId, string.Format(MicrosoftMaintainabilityAnalyzersResources.AvoidUnusedPrivateFieldsMessage, fieldName));
        }
    }
}