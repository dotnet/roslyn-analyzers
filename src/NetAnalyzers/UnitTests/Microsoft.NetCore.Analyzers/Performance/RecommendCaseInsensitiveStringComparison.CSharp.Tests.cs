// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonAnalyzer,
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class RecommendCaseInsensitiveStringComparison_CSharp_Tests : RecommendCaseInsensitiveStringComparison_Base_Tests
    {
        [Fact]
        public Task Diagnostic_Contains_ToLower()
        {
            string originalCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.ToLower().Contains(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.Contains(b, StringComparison.CurrentCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 9, 8, 32, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_Contains_ToLowerInvariant()
        {
            string originalCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.ToLowerInvariant().Contains(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 9, 8, 41, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_Contains_ToUpper()
        {
            string originalCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.ToUpper().Contains(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.Contains(b, StringComparison.CurrentCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 9, 8, 32, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_Contains_ToUpperInvariant()
        {
            string originalCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.ToUpperInvariant().Contains(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    void M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 9, 8, 41, StringComparisonRule, ContainsName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToLower_Return()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.ToLower().IndexOf(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.IndexOf(b, StringComparison.CurrentCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 16, 8, 38, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToLowerInvariant_Return()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.ToLowerInvariant().IndexOf(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.IndexOf(b, StringComparison.InvariantCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 16, 8, 47, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToUpper_Return()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.ToUpper().IndexOf(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.IndexOf(b, StringComparison.CurrentCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 16, 8, 38, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_IndexOf_ToUpperInvariant_Return()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.ToUpperInvariant().IndexOf(b);
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        return a.IndexOf(b, StringComparison.InvariantCultureIgnoreCase);
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 16, 8, 47, StringComparisonRule, IndexOfName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToLower_If()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.ToLower().StartsWith(b))
        {
            return 5;
        }
        return 4;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.StartsWith(b, StringComparison.CurrentCultureIgnoreCase))
        {
            return 5;
        }
        return 4;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 13, 8, 38, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToLowerInvariant_If()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.ToLowerInvariant().StartsWith(b))
        {
            return 5;
        }
        return 4;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase))
        {
            return 5;
        }
        return 4;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 13, 8, 47, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToUpper_If()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.ToUpper().StartsWith(b))
        {
            return 5;
        }
        return 4;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.StartsWith(b, StringComparison.CurrentCultureIgnoreCase))
        {
            return 5;
        }
        return 4;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 13, 8, 38, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_StartsWith_ToUpperInvariant_If()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.ToUpperInvariant().StartsWith(b))
        {
            return 5;
        }
        return 4;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        if (a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase))
        {
            return 5;
        }
        return 4;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 13, 8, 47, StringComparisonRule, StartsWithName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToLower_Assign()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = a.ToLower().CompareTo(b);
        return r;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = StringComparer.CurrentCultureIgnoreCase.Compare(a, b);
        return r;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 17, 8, 41, StringComparerRule, CompareToName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToLowerInvariant_Assign()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = a.ToLowerInvariant().CompareTo(b);
        return r;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = StringComparer.InvariantCultureIgnoreCase.Compare(a, b);
        return r;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 17, 8, 50, StringComparerRule, CompareToName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToUpper_Assign()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = a.ToUpper().CompareTo(b);
        return r;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = StringComparer.CurrentCultureIgnoreCase.Compare(a, b);
        return r;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 17, 8, 41, StringComparerRule, CompareToName);
        }

        [Fact]
        public Task Diagnostic_CompareTo_ToUpperInvariant_Assign()
        {
            string originalCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = a.ToUpperInvariant().CompareTo(b);
        return r;
    }
}";
            string fixedCode = @"using System;
class C
{
    int M()
    {
        string a = ""aBc"";
        string b = ""bc"";
        int r = StringComparer.InvariantCultureIgnoreCase.Compare(a, b);
        return r;
    }
}";
            return VerifyCSharpAsync(originalCode, fixedCode, 8, 17, 8, 50, StringComparerRule, CompareToName);
        }

        private Task VerifyCSharpAsync(string originalSource, string fixedSource,
            int startLine, int startColumn, int endLine, int endColumn, DiagnosticDescriptor rule, string argument)
        {
            DiagnosticResult diagnosticResult = VerifyCS
                .Diagnostic(rule)
                .WithArguments(argument)
                .WithSpan(startLine, startColumn, endLine, endColumn);

            VerifyCS.Test test = new()
            {
                TestCode = originalSource,
                FixedCode = fixedSource
            };
            test.ExpectedDiagnostics.Add(diagnosticResult);
            return test.RunAsync();
        }
    }
}