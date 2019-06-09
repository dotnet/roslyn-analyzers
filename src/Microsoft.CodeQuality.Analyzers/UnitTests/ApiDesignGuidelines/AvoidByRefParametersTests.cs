// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidByRefParametersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidByRefParametersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidByRefParametersTests
    {
        #region Test Plumbing

        private class DiagnosticNumberCSTest : VerifyCS.Test
        {
            public DiagnosticNumberCSTest(int index)
            {
                Index = index;
            }

            public int Index { get; }

            protected override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers) => analyzers[0].SupportedDiagnostics[Index];
        }

        private class DiagnosticNumberVBTest : VerifyVB.Test
        {
            public DiagnosticNumberVBTest(int index)
            {
                Index = index;
            }

            public int Index { get; }

            protected override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers) => analyzers[0].SupportedDiagnostics[Index];
        }

        #endregion 

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task OutParameter_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await new DiagnosticNumberCSTest(0)
            {
                TestCode = $@"
                public class Test
                {{
                    {ctx.AccessCS} void GetObj(out object {ctx.Left}o{ctx.Right}) => o = null;
                }}"
            }.RunAsync();
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task RefParameter_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await new DiagnosticNumberCSTest(1)
            {
                TestCode = $@"
                public class Test
                {{
                    {ctx.AccessCS} void GetObj(ref object {ctx.Left}o{ctx.Right}) {{ }}
                }}"
            }.RunAsync();

            await new DiagnosticNumberVBTest(1)
            {
                TestCode = $@"
                Public Class Test
                    {ctx.AccessVB} Sub GetObj(ByRef {ctx.Left}o{ctx.Right} As Object)
                    End Sub
                End Class"
            }.RunAsync();
        }


        [Fact]
        public async Task PublicVal_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    protected void GiveObj(object o) { o = null; }
                }");
        }
    }
}