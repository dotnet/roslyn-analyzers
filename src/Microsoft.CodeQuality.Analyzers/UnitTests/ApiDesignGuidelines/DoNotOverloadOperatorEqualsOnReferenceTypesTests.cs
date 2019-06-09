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
        [AccessibilityTest(AccessibilityTestTarget.Class)]
        public async Task ClassOverload_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                {ctx.AccessCS} class {ctx.Left}Test{ctx.Right}
                {{
                    public static bool operator ==(Test left, Test right) => true;
                    public static bool operator !=(Test left, Test right) => false;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                {ctx.AccessVB} Class {ctx.Left}Test{ctx.Right}
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
            await VerifyCS.VerifyAnalyzerAsync(@"
                public struct Test
                {
                    public static bool operator ==(Test left, Test right) => true;
                    public static bool operator !=(Test left, Test right) => false;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
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
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                End Class");
        }
    }
}