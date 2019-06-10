// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComRegistrationMethodsShouldBeMatchedAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComRegistrationMethodsShouldBeMatchedAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ComRegistrationMethodsShouldBeMatchedTests
    {
        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.Class)]
        public async Task RegisterMethodOnly_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                {ctx.AccessCS} class {ctx.Left()}Test{ctx.Right()}
                {{
                    [ComRegisterFunction]
                    public static void Register() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                {ctx.AccessVB} Class {ctx.Left()}Test{ctx.Right()}
                    <ComRegisterFunction>
                    Public Shared Sub Register
                    End Sub
                End Class");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.Class)]
        public async Task UnregisterMethodOnly_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                {ctx.AccessCS} class {ctx.Left()}Test{ctx.Right()}
                {{
                    [ComUnregisterFunction]
                    public static void Unregister() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                {ctx.AccessVB} Class {ctx.Left()}Test{ctx.Right()}
                    <ComUnregisterFunction>
                    Public Shared Sub Unregister
                    End Sub
                End Class");
        }

        [Fact]
        public async Task BothFunctions_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                public class Test
                {{
                    [ComRegisterFunction]
                    public static void Register() {{ }}
                    [ComUnregisterFunction]
                    public static void Unregister() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                Class Test
                    <ComRegisterFunction>
                    Public Shared Sub Register
                    End Sub
                    <ComUnregisterFunction>
                    Public Shared Sub Unregister
                    End Sub
                End Class");
        }
    }
}