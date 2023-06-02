// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonAnalyzer,
    Microsoft.NetCore.Analyzers.Performance.RecommendCaseInsensitiveStringComparisonFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class RecommendCaseInsensitiveStringComparison_CSharp_Tests : RecommendCaseInsensitiveStringComparison_Base_Tests
    {
        [Theory]
        [MemberData(nameof(DiagnosedAndFixedData))]
        public async Task Diagnostic_Assign(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"using System;
class C
{{
    void M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        var result = [|{diagnosedLine}|];
    }}
}}";
            string fixedCode = $@"using System;
class C
{{
    void M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        var result = {fixedLine};
    }}
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedData))]
        public async Task Diagnostic_Return(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"using System;
class C
{{
    object M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        return [|{diagnosedLine}|];
    }}
}}";
            string fixedCode = $@"using System;
class C
{{
    object M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        return {fixedLine};
    }}
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedWithEqualsToData))]
        public async Task Diagnostic_If(string diagnosedLine, string fixedLine, string equalsTo)
        {
            string originalCode = $@"using System;
class C
{{
    int M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        if ([|{diagnosedLine}|]{equalsTo})
        {{
            return 5;
        }}
        return 4;
    }}
}}";
            string fixedCode = $@"using System;
class C
{{
    int M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        if ({fixedLine}{equalsTo})
        {{
            return 5;
        }}
        return 4;
    }}
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedData))]
        public async Task Diagnostic_IgnoreResult(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"using System;
class C
{{
    void M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        [|{diagnosedLine}|];
    }}
}}";
            string fixedCode = $@"using System;
class C
{{
    void M()
    {{
        string a = ""aBc"";
        string b = ""bc"";
        {fixedLine};
    }}
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedStringLiteralsData))]
        public async Task Diagnostic_StringLiterals_ReturnExpressionBody(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"using System;
class C
{{
    object M() => [|{diagnosedLine}|];
}}";
            string fixedCode = $@"using System;
class C
{{
    object M() => {fixedLine};
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedStringReturningMethodsData))]
        public async Task Diagnostic_StringReturningMethods_Discard(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"using System;
class C
{{
    public string GetStringA() => ""aBc"";
    public string GetStringB() => ""CdE"";
    void M()
    {{
        _ = [|{diagnosedLine}|];
    }}
}}";
            string fixedCode = $@"using System;
class C
{{
    public string GetStringA() => ""aBc"";
    public string GetStringB() => ""CdE"";
    void M()
    {{
        _ = {fixedLine};
    }}
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosedAndFixedParenthesizedData))]
        public async Task Diagnostic_Parenthesized_ReturnCastedToString(string diagnosedLine, string fixedLine)
        {
            string originalCode = $@"using System;
class C
{{
    string M()
    {{
        return ([|{diagnosedLine}|]).ToString();
    }}
}}";
            string fixedCode = $@"using System;
class C
{{
    string M()
    {{
        return ({fixedLine}).ToString();
    }}
}}";
            await VerifyFixCSharpAsync(originalCode, fixedCode);
        }

        [Theory]
        [MemberData(nameof(NoDiagnosticContainsData))]
        [InlineData("\"aBc\".CompareTo(null)")]
        [InlineData("\"aBc\".ToUpperInvariant().CompareTo((object)null)")]
        public async Task NoDiagnostic_All(string ignoredLine)
        {
            string originalCode = $@"using System;
class C
{{
    object M()
    {{
        char ch = 'c';
        object obj = 3;
        return {ignoredLine};
    }}
}}";

            await VerifyNoDiagnosticCSharpAsync(originalCode);
        }

        [Theory]
        [MemberData(nameof(DiagnosticNoFixCompareToData))]
        public async Task Diagnostic_NoFix_CompareTo(string diagnosedLine)
        {
            string originalCode = $@"using System;
class C
{{
    string GetStringA() => ""aBc"";
    string GetStringB() => ""cDe"";
    int M()
    {{
        string a = ""AbC"";
        string b = ""CdE"";
        return [|{diagnosedLine}|];
    }}
}}";
            await VerifyDiagnosticOnlyCSharpAsync(originalCode);
        }

        private async Task VerifyNoDiagnosticCSharpAsync(string originalSource)
        {
            VerifyCS.Test test = new()
            {
                TestCode = originalSource,
                FixedCode = originalSource
            };

            await test.RunAsync();
        }

        private async Task VerifyDiagnosticOnlyCSharpAsync(string originalSource)
        {
            VerifyCS.Test test = new()
            {
                TestCode = originalSource,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            };

            await test.RunAsync();
        }

        private async Task VerifyFixCSharpAsync(string originalSource, string fixedSource)
        {
            VerifyCS.Test test = new()
            {
                TestCode = originalSource,
                FixedCode = fixedSource,
                MarkupOptions = MarkupOptions.UseFirstDescriptor
            };

            await test.RunAsync();
        }
    }
}