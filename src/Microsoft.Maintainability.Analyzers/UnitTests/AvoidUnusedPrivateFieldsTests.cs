// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
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

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/933")]
        public void CA1823_CSharp_AttributeUsage_NoDiagnostic()
        {
            VerifyCSharp(@"
[System.Obsolete(Message)]
public class Class
{
    private const string Message = ""Test"";
}
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/933")]
        public void CA1823_CSharp_InterpolatedStringUsage_NoDiagnostic()
        {
            VerifyCSharp(@"
public class Class
{
    private const string Message = ""Test"";
    public string PublicMessage = $""Test: {Message}"";
}
");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/933")]
        public void CA1823_CSharp_CollectionInitializerUsage_NoDiagnostic()
        {
            VerifyCSharp(@"
using System.Collections.Generic;

public class Class
{
    private const string Message = ""Test"";
    public List<string> PublicMessage = new List<string> { Message };
}
");
        }

        [Fact]
        public void CA1823_CSharp_SimpleUsages_DiagnosticCases()
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

    Public ReadOnly Property MyValue As Integer
        Get
            Return Used1 + Used2
        End Get
    End Property
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