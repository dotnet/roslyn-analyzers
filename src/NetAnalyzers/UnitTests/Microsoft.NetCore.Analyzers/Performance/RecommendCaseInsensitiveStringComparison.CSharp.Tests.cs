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
            await VerifyCSharpAsync(originalCode, fixedCode);
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
            await VerifyCSharpAsync(originalCode, fixedCode);
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
            await VerifyCSharpAsync(originalCode, fixedCode);
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
            await VerifyCSharpAsync(originalCode, fixedCode);
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
            await VerifyCSharpAsync(originalCode, fixedCode);
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
            await VerifyCSharpAsync(originalCode, fixedCode);
        }

        private async Task VerifyCSharpAsync(string originalSource, string fixedSource)
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