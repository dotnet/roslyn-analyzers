﻿using System;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class StringBuilderAppendShouldNotTakeSubstringFixerTestsCsharp : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StringBuilderAppendShouldNotTakeSubstring();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new StringBuilderAppendShouldNotTakeSubstring();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new StringBuilderAppendShouldNotTakeSubstringCsharpFixer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            throw new NotSupportedException();
        }

        [Fact]
        public void FixesExample1FromTicket()
        {
            const string Code = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(0, 6));
        return sb.ToString();
    }
}";
            const string FixedCode = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text, 0, 6);
        return sb.ToString();
    }
}";
            VerifyCSharpFix(Code, FixedCode);
        }

        [Fact]
        public void FixesExample2FromTicket()
        {
            const string Code = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(2));
        return sb.ToString();
    }
}";
            const string FixedCode = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text, 2, text.Length - 2);
        return sb.ToString();
    }
}";
            VerifyCSharpFix(Code, FixedCode);
        }

        [Fact(Skip = "not working yet")]
        public void FixesAllExamplesFromTicket()
        {

            const string Code = @"
using System.Text;

public class C
{
    public string Append (string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring (0, 6));
        sb.Append(text.Substring (2));
        return sb.ToString ();
    }
}";
            const string FixedCode = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text, 0, 6);
        sb.Append(text, 2, text.Length - 2);
        return sb.ToString ();
    }
}";
            VerifyCSharpFixAll(Code, FixedCode);
        }
    }

    public class StringBuilderAppendShouldNotTakeSubstringFixerTestsBasic : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StringBuilderAppendShouldNotTakeSubstring();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new StringBuilderAppendShouldNotTakeSubstring();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            throw new NotSupportedException();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new StringBuilderAppendShouldNotTakeSubstringBasicFixer();
        }

        [Fact]
        public void FixesExample1FromTicket()
        {
            const string Code = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text.Substring(0, 6))
        Return sb.ToString()
    End Function
End Class
";
            const string FixedCode = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text, 0, 6)
        Return sb.ToString()
    End Function
End Class
";
            VerifyBasicFix(Code, FixedCode);
        }

        [Fact]
        public void FixesExample2FromTicketBasic()
        {
            const string Code = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text.Substring(2))
        Return sb.ToString()
    End Function
End Class
";
            const string FixedCode = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text, 2, text.Length - 2)
        Return sb.ToString()
    End Function
End Class
";
            VerifyBasicFix(Code, FixedCode);
        }

        [Fact(Skip = "not working yet")]
        public void FixesAllExamplesFromTicketBasic()
        {

            const string Code = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text.Substring(0, 6))
        sb.Append(text.Substring(2))
        Return sb.ToString()
    End Function
End Class
";
            const string FixedCode = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text, 0, 6)
        sb.Append(text, 2, text.Length - 2)
        Return sb.ToString()
    End Function
End Class
";
            VerifyBasicFixAll(Code, FixedCode);
        }
    }

}
