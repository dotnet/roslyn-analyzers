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
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task ListProperty_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} System.Collections.Generic.List<int> {left}Ints{right} => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} Property {left}Actions{right} As System.Collections.Generic.List(Of Integer)
                End Class");
        }

        [Fact]
        public async Task NonNestedProperty_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    public System.Action Action => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Property Actions As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task ListField_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} System.Collections.Generic.List<int> {left}Ints{right};
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} {left}Actions{right} As System.Collections.Generic.List(Of Integer)
                End Class");
        }
            
        [Fact]
        public async Task NonListField_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    public System.Action Action;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Action As System.Action(Of System.Action(Of Integer))
                End Class");
        }

        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task ListReturn_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} System.Collections.Generic.List<int> {left}Ints{right}() => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} Function {left}Ints{right} As System.Collections.Generic.List(Of Integer)
                        Return Nothing
                    End Function
                End Class");
        }

        [Fact]
        public async Task NonListReturn_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    public System.Action Action() => null;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Function Action As System.Action(Of System.Action(Of Integer))
                        Return Nothing
                    End Function
                End Class");
        }

        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task ListParameter_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} void Ints(System.Collections.Generic.List<int> {left}ints{right}) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    {visibilityVB} Sub Ints({left}ints{right} As System.Collections.Generic.List(Of Integer))
                    End Sub
                End Class");
        }

        [Fact]
        public async Task NonListParameter_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    public void Action(System.Action a) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Sub Ints(a As System.Action)
                    End Sub
                End Class");
        }
    }
}