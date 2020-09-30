// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AssigningSymbolAndItsMemberInSameStatement,
    Microsoft.CodeQuality.Analyzers.QualityGuidelines.AssigningSymbolAndItsMemberInSameStatementFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    public class AssigningSymbolAndItsMemberInSameStatementFixerTests
    {
        [Theory]
        [InlineData(0, "a.Field = b;")]
        [InlineData(1, "a.Field = a;")]
        public async Task CA2246CSharpCodeFixTestSplitUnobviousAssignment(int codeActionIndex, string fix)
        {
            var code = @"
public class C
{
    public C Field;
}

public class Test
{
    public void Method()
    {
        C a = new C();
        C b = new C();
        [|a.Field|] = a = b;
    }
}
";
            var fixedCode = $@"
public class C
{{
    public C Field;
}}

public class Test
{{
    public void Method()
    {{
        C a = new C();
        C b = new C();
        a = b;
        {fix}
    }}
}}
";
            await new VerifyCS.Test
            {
                TestState = { Sources = { code } },
                FixedState = { Sources = { fixedCode } },
                CodeActionIndex = codeActionIndex,
            }.RunAsync();
        }
    }
}
