// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComRegistrationMethodsShouldBeUsedCorrectlyAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComRegistrationMethodsShouldBeUsedCorrectlyAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ComRegistrationMethodsShouldBeUsedCorrectlyTests
    {
        const string Matched = ComRegistrationMethodsShouldBeUsedCorrectlyAnalyzer.MatchedRuleId;
        const string Visible = ComRegistrationMethodsShouldBeUsedCorrectlyAnalyzer.VisibleRuleId;

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.Class)]
        public async Task RegisterMethodOnly_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                {ctx.AccessCS} class {ctx.Left(true, Matched)}Test{ctx.Right(true)}
                {{
                    [ComRegisterFunction]
                    internal static void Register() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                {ctx.AccessVB} Class {ctx.Left(true, Matched)}Test{ctx.Right(true)}
                    <ComRegisterFunction>
                    Friend Shared Sub Register
                    End Sub
                End Class");
        }

        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.InsideClass)]
        public async Task RegistrationMethods_WarnWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                public class Test
                {{
                    [ComRegisterFunction]
                    {ctx.AccessCS} static void {ctx.Left(true, Visible)}Register{ctx.Right(true)}() {{ }}

                    [ComUnregisterFunction]
                    {ctx.AccessCS} static void {ctx.Left(true, Visible)}Unregister{ctx.Right(true)}() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                Public Class Test
                    <ComRegisterFunction>
                    {ctx.AccessVB} Shared Sub {ctx.Left(true, Visible)}Register{ctx.Right(true)}
                    End Sub

                    <ComUnregisterFunction>
                    {ctx.AccessVB} Shared Sub {ctx.Left(true, Visible)}Unregister{ctx.Right(true)}
                    End Sub
                End Class");
        }


        [Theory]
        [AccessibilityTest(AccessibilityTestTarget.Class)]
        public async Task UnregisterMethodOnly_WarnsWhenExposed(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                {ctx.AccessCS} class {ctx.Left(true, Matched)}Test{ctx.Right(true)}
                {{
                    [ComUnregisterFunction]
                    internal static void Unregister() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                {ctx.AccessVB} Class {ctx.Left(true, Matched)}Test{ctx.Right(true)}
                    <ComUnregisterFunction>
                    Friend Shared Sub Unregister
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
                    internal static void Register() {{ }}
                    [ComUnregisterFunction]
                    internal static void Unregister() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                Class Test
                    <ComRegisterFunction>
                    Friend Shared Sub Register
                    End Sub
                    <ComUnregisterFunction>
                    Friend Shared Sub Unregister
                    End Sub
                End Class");
        }
    }
}