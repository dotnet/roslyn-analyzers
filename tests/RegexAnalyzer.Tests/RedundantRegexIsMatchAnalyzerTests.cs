using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<RegexAnalyzer.RedundantRegexIsMatchAnalyzer>;

namespace RegexAnalyzer.Tests;

public class RedundantRegexIsMatchAnalyzerTests
{
    [Fact]
    public async Task TestIsMatchFollowedByMatch_DetectsDiagnostic()
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
            // Use m
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TestIsMatchFollowedByMatch_WithStaticMethod_DetectsDiagnostic()
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
            // Use m
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TestIsMatchFollowedByMatch_WithThreeArguments_DetectsDiagnostic()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if ([|Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase)|])
        {
            Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            // Use m
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TestIsMatchWithoutFollowingMatch_NoDiagnostic()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        if (Regex.IsMatch(input, pattern))
        {
            // Do something else
            System.Console.WriteLine(""Matched"");
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TestIsMatchWithDifferentArguments_NoDiagnostic()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern, string pattern2)
    {
        if (Regex.IsMatch(input, pattern))
        {
            Match m = Regex.Match(input, pattern2);
            // Use m
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TestMatchWithoutIsMatch_NoDiagnostic()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    void TestMethod(string input, string pattern)
    {
        Match m = Regex.Match(input, pattern);
        if (m.Success)
        {
            // Use m
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }



    [Fact]
    public async Task TestIsMatchFollowedByAssignment_DetectsDiagnostic()
    {
        var test = @"
using System.Text.RegularExpressions;

class TestClass
{
    Match m;
    
    void TestMethod(string input, string pattern)
    {
        if ([|Regex.IsMatch(input, pattern)|])
        {
            m = Regex.Match(input, pattern);
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
