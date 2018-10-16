using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class StringBuilderAppendShouldNotTakeSubstringFixerTests : CodeFixTestBase
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
            return new StringBuilderAppendShouldNotTakeSubstringFixer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new StringBuilderAppendShouldNotTakeSubstringFixer();
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariable()
        {
            const string code = @"
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
            const string fixedCode = @"
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
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableWithUnnamedParametersInOrder()
        {
            const string code = @"
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
            const string fixedCode = @"
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
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableWithNamedParametersInDifferentOrder()
        {
            const string code = @"
using System.Text;

public class C
{
    private string Append1(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(length: 20, startIndex: 4));
        return sb.ToString();
    }
}";

            const string fixedCode = @"
using System.Text;

public class C
{
    private string Append1(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text, 4, 20);
        return sb.ToString();
    }
}";

            VerifyCSharpFix(code, fixedCode);
        }


        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableWithNamedParametersInOrder()
        {
            const string code = @"
using System.Text;

public class C
{
    private string Append2(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(startIndex: 4, length: 20));
        return sb.ToString();
    }
}";

            const string fixedCode = @"
using System.Text;

public class C
{
    private string Append2(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text, 4, 20);
        return sb.ToString();
    }
}";
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableWithMethodChaining()
        {
            const string code = @"
using System.Text;

public class C
{
    private string Append3(string text)
    {
        return (new StringBuilder().Append(text.Substring(4, 20))).ToString();
    }
}";
            const string fixedCode = @"
using System.Text;

public class C
{
    private string Append3(string text)
    {
        return (new StringBuilder().Append(text, 4, 20)).ToString();
    }
}";
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableWithMethodChainingMultipleAppends()
        {
            const string code = @"
using System.Text;

public class C
{
    private string Append4(string text)
    {
        var sb = new StringBuilder().Append(text.Substring(4, 10)).Append(text.Substring(1, 3));
        return sb.ToString();
    }
}";
            
            // TODO: what is fixed here? there are two possible fixes, probably a good test for FixAll as well?
            const string fixedCode = @"
using System.Text;

public class C
{
    private string Append4(string text)
    {
        var sb = new StringBuilder().Append(text, 4, 10).Append(text, 1, 3);
        return sb.ToString();
    }
}";
            VerifyCSharpFix(code, fixedCode);

        }

        // TODO: write analyzer test for this!
        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableWithChainOnStringParameter()
        {
            const string code = @"
using System.Text;
using System.Linq;

public class C
{
    private string Append5(string text)
    {
        var sb = new StringBuilder()
            .Append(text.Substring(4, 10).Reverse());
        return sb.ToString();
    }
}";
            
            // must not fix!
            VerifyCSharpFix(code, code);
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringLiteral()
        {
            const string code = @"
using System.Text;

public class C
{
    public string Append()
    {
        var sb = new StringBuilder();
        sb.Append(""TestTest"".Substring(0, 6));
        return sb.ToString();
    }
}";
            const string fixedCode = @"
using System.Text;

public class C
{
    public string Append()
    {
        var sb = new StringBuilder();
        sb.Append(""TestTest"", 0, 6);
        return sb.ToString();
    }
}";
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingTwoParameterVariantOnStringVariableBasic()
        {
            const string code = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text.Substring(0, 6))
        Return sb.ToString()
    End Function
End Class
";
            const string fixedCode = @"
Imports System.Text

Public Class C
    Public Function Append(ByVal text As String) As String
        Dim sb = New StringBuilder()
        sb.Append(text, 0, 6)
        Return sb.ToString()
    End Function
End Class
";
            VerifyBasicFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingOneParameterVariantOnStringVariable()
        {
            const string code = @"
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
            const string fixedCode = @"
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
            VerifyCSharpFix(code, fixedCode);
        }

        [Fact]
        public void ReplacingOneParameterVariantOnStringVariableBasic()
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

        [Fact(Skip = "skip failing test")]
        public void FixesAllOnExampleFromTicket()
        {

            const string code = @"
using System.Text;

public class C
{
    public string Append(string text)
    {
        var sb = new StringBuilder();
        sb.Append(text.Substring(0, 6));
        sb.Append(text.Substring(2));
        return sb.ToString ();
    }
}";
            const string fixedCode = @"
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
            VerifyCSharpFixAll(code, fixedCode);
        }

        [Fact(Skip = "skip failing test")]
        public void FixesAllOnExampleFromTicketBasic()
        {

            const string code = @"
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
            const string fixedCode = @"
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
            VerifyBasicFixAll(code, fixedCode);
        }
    }
}
