// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidByRefParametersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidByRefParametersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidByRefParametersTests
    {
        const string AvoidRef = AvoidByRefParametersAnalyzer.AvoidRefRuleId;
        const string AvoidOut = AvoidByRefParametersAnalyzer.AvoidOutRuleId;

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task OutParameter_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} void GetObj(out object {ctx.Left(true, AvoidOut)}o{ctx.Right(true)}) => o = null;
                }}");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task RefParameter_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} void GetObj(ref object {ctx.Left(true, AvoidRef)}o{ctx.Right(true)}) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} Sub GetObj(ByRef {ctx.Left(true, AvoidRef)}o{ctx.Right(true)} As Object)
                    End Sub
                End Class");
        }


        [Fact]
        public async Task PublicVal_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    protected void GiveObj(object o) { o = null; }
                }");
        }
    }
}