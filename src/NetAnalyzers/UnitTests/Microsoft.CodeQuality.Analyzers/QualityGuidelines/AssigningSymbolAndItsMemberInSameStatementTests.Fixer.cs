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
        [Fact]
        public async Task CA2246CSharpCodeFixTestSplitUnobviousAssignment()
        {
            await VerifyCS.VerifyCodeFixAsync(@"
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
",
    @"
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
        a = b;
        a.Field = b;
    }
}
");
        }
    }
}
