// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UseGenericsWhereAppropriateAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UseGenericsWhereAppropriateAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class UseGenericsWhereAppropriateTests
    {
        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task RefObject_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} void {ctx.Left()}Swap{ctx.Right()}(ref object o1, ref object o2) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} Sub {ctx.Left()}Swap{ctx.Right()}(ByRef o1 As Object, ByRef o2 As Object)
                    End Sub
                End Class");

        }

        [Fact]
        public async Task PublicRefInt_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public void Swap(ref int o1, ref int o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Sub Swap(ByRef o1 As Integer, ByRef o2 As Integer)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task PublicValObject_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public void Swap(object o1, object o2) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Sub Swap(o1 As Object, o2 As Object)
                    End Sub
                End Class");
        }
    }
}