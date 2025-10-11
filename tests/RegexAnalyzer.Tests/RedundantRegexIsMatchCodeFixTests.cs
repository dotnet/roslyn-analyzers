using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    RegexAnalyzer.RedundantRegexIsMatchAnalyzer,
    RegexAnalyzer.RedundantRegexIsMatchCodeFixProvider>;

namespace RegexAnalyzer.Tests;

public class RedundantRegexIsMatchCodeFixTests
{
    [Fact]
    public async Task TestCodeFix_BasicCase()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if ([|Regex.IsMatch(input, pattern)|])
        {
            Match m = Regex.Match(input, pattern);
            System.Console.WriteLine(m.Value);
        }
    }
}
";

        var fixedCode = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if (Regex.Match(input, pattern) is
            {
                Success: true
            } m)
        {
            System.Console.WriteLine(m.Value);
        }
    }
}
";

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode);
    }

    [Fact]
    public async Task TestCodeFix_WithVar()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if ([|Regex.IsMatch(input, pattern)|])
        {
            var m = Regex.Match(input, pattern);
            System.Console.WriteLine(m.Value);
        }
    }
}
";

        var fixedCode = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if (Regex.Match(input, pattern) is
            {
                Success: true
            } m)
        {
            System.Console.WriteLine(m.Value);
        }
    }
}
";

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode);
    }



    [Fact]
    public async Task TestCodeFix_WithMultipleStatementsInBlock()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if ([|Regex.IsMatch(input, pattern)|])
        {
            Match m = Regex.Match(input, pattern);
            System.Console.WriteLine(m.Value);
            System.Console.WriteLine(m.Index);
        }
    }
}
";

        var fixedCode = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if (Regex.Match(input, pattern) is
            {
                Success: true
            } m)
        {
            System.Console.WriteLine(m.Value);
            System.Console.WriteLine(m.Index);
        }
    }
}
";

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode);
    }




}
