// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.OverloadOperatorEqualsOnOverloadingAddAndSubtractAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.OverloadOperatorEqualsOnOverloadingAddAndSubtractAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class OverloadOperatorEqualsOnOverloadingAddAndSubtractTests
    {
        [Theory]
        [MemberData(nameof(OperatorTestData))]
        public async Task SingleOperatorOnly_WarnsWhenExposed(AccessibilityContext ctx, string op)
        {
            System.Console.Write("hello");
            await VerifyCS.VerifyAnalyzerAsync($@"
                {ctx.AccessCS} class {ctx.Left()}Test{ctx.Right()}
                {{
                    public static Test operator {op}(Test left, Test right) => left;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                {ctx.AccessVB} Class {ctx.Left()}Test{ctx.Right()}
                    Public Shared Operator {op}(left As Test, right As Test) As Test
                        Return left
                    End Operator
                End Class");
        }


        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.Class)]
        public async Task MultiOperator_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                {ctx.AccessCS} class {ctx.Left()}Test{ctx.Right()}
                {{
                    public static Test operator +(Test left, Test right) => left;
                    public static Test operator -(Test left, Test right) => right;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                {ctx.AccessVB} Class {ctx.Left()}Test{ctx.Right()}
                    Public Shared Operator +(left As Test, right As Test) As Test
                        Return left
                    End Operator
                    Public Shared Operator -(left As Test, right As Test) As Test
                        Return left
                    End Operator
                End Class");
        }

        [Fact]
        public async Task AllOperators_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public static Test operator +(Test left, Test right) => left;
                    public static Test operator -(Test left, Test right) => right;
                    public static bool operator ==(Test left, Test right) => true;
                    public static bool operator !=(Test left, Test right) => false;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                    Public Shared Operator +(left As Test, right As Test) As Test
                        Return left
                    End Operator
                    Public Shared Operator -(left As Test, right As Test) As Test
                        Return left
                    End Operator
                    Public Shared Operator =(left As Test, right As Test) As Boolean
                        Return True
                    End Operator
                    Public Shared Operator <>(left As Test, right As Test) As Boolean
                        Return False
                    End Operator
                End Class");
        }

        [Fact]
        public async Task NoOperators_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Class Test
                End Class");
        }

        #region Test Data

        public static IEnumerable<object[]> OperatorTestData()
        {
            foreach (var arr in new AccessibilityTestAttribute(AccessibilityTestTarget.Class).GetData(null))
            {
                yield return arr.Concat(new[] { "+" }).ToArray();
                yield return arr.Concat(new[] { "-" }).ToArray();
            }
        }

        #endregion
    }
}