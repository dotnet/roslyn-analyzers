// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidNonpublicFieldsInComVisibleValueTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidNonpublicFieldsInComVisibleValueTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidNonpublicFieldsInComVisibleValueTypesTests
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task ComVisibleStruct_NonpublicFields_WarnsWhenExposed(AccessibilityContext ctx, string comVisibleType1, string comVisibleType2)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                [assembly: {comVisibleType1}]

                [{comVisibleType2}]
                {ctx.AccessCS} struct Test
                {{
                    private int {ctx.Left()}A{ctx.Right()};
                    internal int {ctx.Left()}E{ctx.Right()};
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                <Assembly: {comVisibleType1}>

                <{comVisibleType2}>
                {ctx.AccessVB} Structure Test
                    Private {ctx.Left()}A{ctx.Right()} As Integer
                    Friend {ctx.Left()}E{ctx.Right()} As Integer
                End Structure");
        }

        [Fact]
        public async Task ComVisibleClass_NonpublicFields_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System.Runtime.InteropServices;

                [ComVisible(true)]
                public class Test
                {
                    private int A;
                    internal int E;
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Imports System.Runtime.InteropServices

                <ComVisible(True)>
                Public Class Test
                    Private A As Integer
                    Friend E As Integer
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