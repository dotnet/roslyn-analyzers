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
        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task OutParameter_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                public class Test
                {{
                    {visibilityCS} void GetObj(out object {left}o{right}) => o = null;
                }}");
        }

        #region Ref Tests Plumbing

        // Gets the second diagnostic (avoid ref parameters). If we don't do this,
        // the test for it will flip out because it doesn't know which diagnostic
        // to look for. Is there a better way to do this?
        static DiagnosticDescriptor DefaultDiagnostic(DiagnosticAnalyzer[] analyzers) => analyzers[0].SupportedDiagnostics[1]; 

        private class RefCSTest : VerifyCS.Test
        {
            protected override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers) => DefaultDiagnostic(analyzers);
        }

        private class RefVBTest : VerifyVB.Test
        {
            protected override DiagnosticDescriptor GetDefaultDiagnostic(DiagnosticAnalyzer[] analyzers) => DefaultDiagnostic(analyzers);
        }

        #endregion 

        [Theory]
        [AccessibilityData(Accessibility.Public, true)]
        [AccessibilityData(Accessibility.Protected, true)]
        [AccessibilityData(Accessibility.Internal, false)]
        [AccessibilityData(Accessibility.Private, false)]
        public async Task RefParameter_WarnsWhenExposed(string visibilityCS, string visibilityVB, string left, string right)
        {
            await new RefCSTest()
            {
                TestCode = $@"
                public class Test
                {{
                    {visibilityCS} void GetObj(ref object {left}o{right}) {{ }}
                }}"
            }.RunAsync();

            await new RefVBTest()
            {
                TestCode = $@"
                Public Class Test
                    {visibilityVB} Sub GetObj(ByRef {left}o{right} As Object)
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