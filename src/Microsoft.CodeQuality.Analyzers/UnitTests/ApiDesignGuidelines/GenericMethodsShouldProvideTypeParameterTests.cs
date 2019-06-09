// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.GenericMethodsShouldProvideTypeParameterAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.GenericMethodsShouldProvideTypeParameterAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class GenericMethodsShouldProvideTypeParameterTests
    {
        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task NoParameters_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} void {ctx.Left()}Method{ctx.Right()}<T>() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} Sub {ctx.Left()}Method{ctx.Right()}(Of T)
                    End Sub
                End Class");
        }


        [Fact]
        public async Task Parameters_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public void Method<T>(T o) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Sub Method(Of T)(o As T)
                    End Sub
                End Class");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task SomeParameters_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Class
                {{
                    {ctx.AccessCS} void {ctx.Left()}Method{ctx.Right()}<T1, T2>(T1 o) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class [MyClass]
                    {ctx.AccessVB} Sub {ctx.Left()}Method{ctx.Right()}(Of T1, T2)(o As T1)
                    End Sub
                End Class");
        }
    }
}