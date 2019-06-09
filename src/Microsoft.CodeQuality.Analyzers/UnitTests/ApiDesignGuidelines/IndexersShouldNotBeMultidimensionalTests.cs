// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.IndexersShouldNotBeMultidimensionalAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.IndexersShouldNotBeMultidimensionalAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class IndexersShouldNotBeMultidimensionalTests
    {
        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task MultidimensionalIndexer_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {ctx.AccessCS} int {ctx.Left()}this{ctx.Right()}[int x, int y] => 0;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {ctx.AccessVB} ReadOnly Property {ctx.Left()}Int{ctx.Right()}(x As Integer, y As Integer) As Integer
                        Get
                            Return 0
                        End Get
                    End Property
                End Class");
        }

        [Fact]
        public async Task SingleDimensionalIndexer_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public int this[int x] => 0;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public ReadOnly Default Property This(x As Integer) As Integer
                        Get
                            Return 0
                        End Get
                    End Property
                End Class");
        }

        [Fact]
        public async Task NonParameterfulProperty_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public int Int => 0;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public ReadOnly Property Int As Integer
                        Get
                            Return 0
                        End Get
                    End Property
                End Class");
        }
    }
}