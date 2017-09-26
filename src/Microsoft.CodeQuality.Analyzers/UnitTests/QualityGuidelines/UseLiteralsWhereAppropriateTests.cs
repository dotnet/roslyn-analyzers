// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public class UseLiteralsWhereAppropriateTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseLiteralsWhereAppropriateAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseLiteralsWhereAppropriateAnalyzer();
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1299")]
        public void CA1802_Diagnostics_CSharp()
        {
            VerifyCSharp(@"
public class Class1
{
    static readonly string f1 = """";
    static readonly string f2 = ""Nothing"";
    static readonly string f3,f4 = ""Message is shown only for f4"";
    static readonly int f5 = 3;
    const int f6 = 3;
    static readonly int f7 = 8 + f6;
    internal static readonly int f8 = 8 + f6;
}",
                GetCSharpEmptyStringResultAt(line: 4, column: 28, symbolName: "f1"),
                GetCSharpDefaultResultAt(line: 5, column: 28, symbolName: "f2"),
                GetCSharpDefaultResultAt(line: 6, column: 31, symbolName: "f4"),
                GetCSharpDefaultResultAt(line: 7, column: 25, symbolName: "f5"),
                GetCSharpDefaultResultAt(line: 9, column: 25, symbolName: "f7"),
                GetCSharpDefaultResultAt(line: 10, column: 34, symbolName: "f8"));
        }

        [Fact]
        public void CA1802_NoDiagnostics_CSharp()
        {
            VerifyCSharp(@"
public class Class1
{
    public static readonly string f1 = """"; // Not private or Internal
    static string f3, f4 = ""Message is shown only for f4""; // Not readonly
    readonly int f5 = 3; // Not static
    const int f6 = 3; // Is already const
    static int f9 = getF9();
    static readonly int f7 = 8 + f9; // f9 is not a const
    static readonly string f8 = null; // null value

    private static int getF9()
    {
        throw new System.NotImplementedException();
    }
}");
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1299")]
        public void CA1802_Diagnostics_VisualBasic()
        {
            VerifyBasic(@"
Public Class Class1
    Shared ReadOnly f1 As String = """"
    Shared ReadOnly f2 As String = ""Nothing""
    Shared ReadOnly f3, f4 As String = ""Message is shown only for f4""
    Shared ReadOnly f5 As Integer = 3
    Const f6 As Integer = 3
    Shared ReadOnly f7 As Integer = 8 + f6
    Friend Shared ReadOnly f8 As Integer = 8 + f6
End Class",
                GetBasicEmptyStringResultAt(line: 3, column: 21, symbolName: "f1"),
                GetBasicDefaultResultAt(line: 4, column: 21, symbolName: "f2"),
                GetBasicDefaultResultAt(line: 5, column: 25, symbolName: "f3"), // Reporting on f4 is blocked by https://github.com/dotnet/roslyn/issues/22252
                GetBasicDefaultResultAt(line: 6, column: 21, symbolName: "f5"),
                GetBasicDefaultResultAt(line: 8, column: 21, symbolName: "f7"),
                GetBasicDefaultResultAt(line: 9, column: 28, symbolName: "f8"));
        }

        [Fact]
        public void CA1802_NoDiagnostics_VisualBasic()
        {
            VerifyBasic(@"
Public Class Class1
    ' Not Private or Friend
    Public Shared ReadOnly f1 As String = """"
    ' Not Readonly
    Shared f3 As String, f4 As String = ""Message is shown only for f4""
    ' Not Shared
    ReadOnly f5 As Integer = 3
    ' Is already Const
    Const f6 As Integer = 3
    Shared f9 As Integer = getF9()
    ' f9 is not a Const
    Shared ReadOnly f7 As Integer = 8 + f9
    ' null value
    Shared ReadOnly f8 As String = Nothing

    Private Shared Function getF9() As Integer
        Throw New System.NotImplementedException()
    End Function
End Class");
        }

        private DiagnosticResult GetCSharpDefaultResultAt(int line, int column, string symbolName)
        {
            return GetCSharpResultAt(line, column, UseLiteralsWhereAppropriateAnalyzer.DefaultRule, symbolName);
        }

        private DiagnosticResult GetCSharpEmptyStringResultAt(int line, int column, string symbolName)
        {
            return GetCSharpResultAt(line, column, UseLiteralsWhereAppropriateAnalyzer.EmptyStringRule, symbolName);
        }

        private DiagnosticResult GetBasicDefaultResultAt(int line, int column, string symbolName)
        {
            return GetBasicResultAt(line, column, UseLiteralsWhereAppropriateAnalyzer.DefaultRule, symbolName);
        }

        private DiagnosticResult GetBasicEmptyStringResultAt(int line, int column, string symbolName)
        {
            return GetBasicResultAt(line, column, UseLiteralsWhereAppropriateAnalyzer.EmptyStringRule, symbolName);
        }
    }
}