// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotOverloadOperatorEqualsOnReferenceTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotOverloadOperatorEqualsOnReferenceTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class DoNotOverloadOperatorEqualsOnReferenceTypesTests
    {
        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        public async Task ClassOverload_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                {visibilityCS} class {left}Test{right}
                {{
                    public static bool operator ==(Test left, Test right) => true;
                    public static bool operator !=(Test left, Test right) => false;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                {visibilityVB} Class {left}Test{right}
                    Public Shared Operator =(left As Test, right As Test) As Boolean
                        Return True
                    End Operator
                    Public Shared Operator <>(left As Test, right As Test) As Boolean
                        Return False
                    End Operator
                End Class");
        }

        [Fact]
        public async Task StructOverload_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public struct Test
                {{
                    public static bool operator ==(Test left, Test right) => true;
                    public static bool operator !=(Test left, Test right) => false;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Structure Test
                    Public Shared Operator =(left As Test, right As Test) As Boolean
                        Return True
                    End Operator
                    Public Shared Operator <>(left As Test, right As Test) As Boolean
                        Return False
                    End Operator
                End Structure");
        }

        [Fact]
        public async Task ClassNoOverload_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                End Class");
        }
    }
}