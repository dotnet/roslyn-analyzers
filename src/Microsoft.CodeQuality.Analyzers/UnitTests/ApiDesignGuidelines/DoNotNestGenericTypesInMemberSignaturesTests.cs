// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotNestGenericTypesInMemberSignaturesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotNestGenericTypesInMemberSignaturesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotNestGenericTypesInMemberSignaturesTests
    {
        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task NestedProperty_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} System.Action<System.Action<int>> {left}Actions{right} => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} Property {left}Actions{right} As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task NonNestedProperty_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    public System.Action<int> Actions => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Property Actions As System.Action(Of Integer)
                End Class");
        }

        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task NestedField_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} System.Action<System.Action<int>> {left}Actions{right};
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} {left}Actions{right} As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Fact]
        public async Task NonNestedField_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public System.Action<int> Action;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Action As System.Action(Of Integer)
                End Class");
        }


        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task NestedReturn_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} System.Action<System.Action<int>> {left}Actions{right}() => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} Function {left}Actions{right} As System.Action(Of System.Action(Of Integer))
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task NonNestedReturn_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    public System.Action<int> Actions() => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Function Actions As System.Action(Of Integer)
                            Return Nothing
                    End Function
                End Class");
        }

        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task NestedParameter_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} void Actions(System.Action<System.Action<int>> {left}action{right}) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} Sub Actions({left}action{right} As System.Action(Of System.Action(Of Integer)))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task NonNestedParameter_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public void DoActions(System.Action<int> action) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Sub DoActions(action As System.Action(Of Integer))
                    End Sub
                End Class");
        }
    }
}