// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task MultidimensionalIndexer_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} int {left}this{right}[int x, int y] => 0;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} ReadOnly Property {left}Int{right}(x As Integer, y As Integer) As Integer
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