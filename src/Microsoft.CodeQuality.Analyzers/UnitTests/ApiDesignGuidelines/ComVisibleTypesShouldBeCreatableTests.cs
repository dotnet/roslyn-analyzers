// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComVisibleTypesShouldBeCreatableAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComVisibleTypesShouldBeCreatableAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ComVisibleTypesShouldBeCreatableTests
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task ComVisibleClass_NonCreatable_WarnsWhenExposed(AccessibilityContext ctx, string comVisibleType1, string comVisibleType2)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                [assembly: {comVisibleType1}]

                [{comVisibleType2}]
                {ctx.AccessCS} class {ctx.Left()}Test{ctx.Right()}
                {{
                    public Test(int x) {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                <Assembly: {comVisibleType1}>

                <{comVisibleType2}>
                {ctx.AccessVB} Class {ctx.Left()}Test{ctx.Right()}
                    Public Sub New(x As Integer)
                    End Sub
                End Class");
        }

        [Fact]
        public async Task ComVisibleClass_Creatable_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System.Runtime.InteropServices;

                [ComVisible(true)]
                public class Test
                {
                    public Test() { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Imports System.Runtime.InteropServices

                <ComVisible(True)>
                Public Class Test
                    Public Sub New
                    End Sub
                End Class");
        }

        #region Test Data

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(true)", "ComVisible(true)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(false)", "ComVisible(true)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)" };
        }

        #endregion
    }
}