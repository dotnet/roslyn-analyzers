// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidExcessiveParametersOnGenericTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidExcessiveParametersOnGenericTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidExcessiveParametersOnGenericTypesTests
    {
        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        public async Task ThreeArguments_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                {visibilityCS} class {left}Test{right}<T1, T2, T3>
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                {visibilityVB} Class {left}Test{right}(Of T1, T2, T3)
                End Class");
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("<T>", "(Of T)")]
        [InlineData("<T1, T2>", "(Of T1, T2)")]
        public async Task LessThanThreeArguments_NeverWarns(string arityCS, string arityVB)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test{arityCS}
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test{arityVB}
                End Class");
        }

        [Theory]
        [InlineData("<T1, T2, T3>", "(Of T1, T2, T3)")]
        [InlineData("<T1, T2, T3, T4>", "(Of T1, T2, T3, T4)")]
        [InlineData("<T1, T2, T3, T4, T5>", "(Of T1, T2, T3, T4, T5)")]
        public async Task MoreThanTwoArguments_WarnsWhenExposed(string arityCS, string arityVB)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class [|Test|]{arityCS}
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class [|Test|]{arityVB}
                End Class");
        }
    }
}