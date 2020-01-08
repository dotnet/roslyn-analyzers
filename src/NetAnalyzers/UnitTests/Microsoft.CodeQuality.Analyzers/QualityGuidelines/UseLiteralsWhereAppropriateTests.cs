// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.UseLiteralsWhereAppropriateAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.UseLiteralsWhereAppropriateAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public class UseLiteralsWhereAppropriateTests
    {
        [Fact]
        public async Task CA1802_Diagnostics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task CA1802_NoDiagnostics_CSharp()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        [Fact]
        public async Task CA1802_Diagnostics_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Class1
    Shared ReadOnly f1 As String = """"
    Shared ReadOnly f2 As String = ""Nothing""
    Shared ReadOnly f3 As String, f4 As String = ""Message is shown only for f4""
    Shared ReadOnly f5 As Integer = 3
    Const f6 As Integer = 3
    Shared ReadOnly f7 As Integer = 8 + f6
    Friend Shared ReadOnly f8 As Integer = 8 + f6
End Class",
                GetBasicEmptyStringResultAt(line: 3, column: 21, symbolName: "f1"),
                GetBasicDefaultResultAt(line: 4, column: 21, symbolName: "f2"),
                GetBasicDefaultResultAt(line: 5, column: 35, symbolName: "f4"),
                GetBasicDefaultResultAt(line: 6, column: 21, symbolName: "f5"),
                GetBasicDefaultResultAt(line: 8, column: 21, symbolName: "f7"),
                GetBasicDefaultResultAt(line: 9, column: 28, symbolName: "f8"));
        }

        [Fact]
        public async Task CA1802_NoDiagnostics_VisualBasic()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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

        [Theory]
        [WorkItem(2772, "https://github.com/dotnet/roslyn-analyzers/issues/2772")]
        [InlineData("", false)]
        [InlineData("dotnet_code_quality.required_modifiers = static", false)]
        [InlineData("dotnet_code_quality.required_modifiers = none", true)]
        [InlineData("dotnet_code_quality." + UseLiteralsWhereAppropriateAnalyzer.RuleId + ".required_modifiers = none", true)]
        public async Task EditorConfigConfiguration_RequiredModifiersOption(string editorConfigText, bool reportDiagnostic)
        {
            var expected = Array.Empty<DiagnosticResult>();
            if (reportDiagnostic)
            {
                expected = new DiagnosticResult[]
                {
                    GetCSharpDefaultResultAt(4, 26, "field")
                };
            }

            var csTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
public class Test
{
    private readonly int field = 0;
}
"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            };
            csTest.ExpectedDiagnostics.AddRange(expected);
            await csTest.RunAsync();

            expected = Array.Empty<DiagnosticResult>();
            if (reportDiagnostic)
            {
                expected = new DiagnosticResult[]
                {
                    GetBasicDefaultResultAt(3, 22, "field")
                };
            }

            var vbTest = new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Public Class Test
    Private ReadOnly field As Integer = 0
End Class
"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                }
            };
            vbTest.ExpectedDiagnostics.AddRange(expected);
            await vbTest.RunAsync();
        }

        private DiagnosticResult GetCSharpDefaultResultAt(int line, int column, string symbolName)
            => new DiagnosticResult(UseLiteralsWhereAppropriateAnalyzer.DefaultRule)
                .WithLocation(line, column)
                .WithArguments(symbolName);

        private DiagnosticResult GetCSharpEmptyStringResultAt(int line, int column, string symbolName)
            => new DiagnosticResult(UseLiteralsWhereAppropriateAnalyzer.EmptyStringRule)
                .WithLocation(line, column)
                .WithArguments(symbolName);

        private DiagnosticResult GetBasicDefaultResultAt(int line, int column, string symbolName)
            => new DiagnosticResult(UseLiteralsWhereAppropriateAnalyzer.DefaultRule)
                .WithLocation(line, column)
                .WithArguments(symbolName);

        private DiagnosticResult GetBasicEmptyStringResultAt(int line, int column, string symbolName)
            => new DiagnosticResult(UseLiteralsWhereAppropriateAnalyzer.EmptyStringRule)
                .WithLocation(line, column)
                .WithArguments(symbolName);
    }
}