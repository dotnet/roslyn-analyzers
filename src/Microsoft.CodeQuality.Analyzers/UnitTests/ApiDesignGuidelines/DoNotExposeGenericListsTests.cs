// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotExposeGenericListsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotExposeGenericListsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotExposeGenericListsTests
    {
        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task ListProperty_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} System.Collections.Generic.List<int> {ctx.Left}Ints{ctx.Right} => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} Property {ctx.Left}Actions{ctx.Right} As System.Collections.Generic.List(Of Integer)
                End Class");
        }

        [Fact]
        public async Task NonNestedProperty_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public System.Action Action => null;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Property Actions As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task ListField_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} System.Collections.Generic.List<int> {ctx.Left}Ints{ctx.Right};
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} {ctx.Left}Actions{ctx.Right} As System.Collections.Generic.List(Of Integer)
                End Class");
        }
            
        [Fact]
        public async Task NonListField_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public System.Action Action;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Action As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task ListReturn_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} System.Collections.Generic.List<int> {ctx.Left}Ints{ctx.Right}() => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} Function {ctx.Left}Ints{ctx.Right} As System.Collections.Generic.List(Of Integer)
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task NonListReturn_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public System.Action Action() => null;
                }");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Function Action As System.Action(Of System.Action(Of Integer))
                        Return Nothing
                    End Function
                End Class");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task ListParameter_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} void Ints(System.Collections.Generic.List<int> {ctx.Left}ints{ctx.Right}) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} Sub Ints({ctx.Left}ints{ctx.Right} As System.Collections.Generic.List(Of Integer))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task NonListParameter_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public void Action(System.Action a) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Sub Ints(a As System.Action)
                    End Sub
                End Class");
        }
    }
}