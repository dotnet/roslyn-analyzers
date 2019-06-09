// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AutoLayoutTypesShouldNotBeComVisibleAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AutoLayoutTypesShouldNotBeComVisibleAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AutoLayoutTypesShouldNotBeComVisibleTests
    {
        #region Test Data

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(true)", "ComVisible(true)", "StructLayout(LayoutKind.Auto)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "StructLayout(LayoutKind.Auto)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(false)", "ComVisible(true)", "StructLayout(LayoutKind.Auto)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "StructLayout(LayoutKind.Auto)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(true)", "StructLayout(LayoutKind.Sequential)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "StructLayout(LayoutKind.Sequential)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(true)", "StructLayout(LayoutKind.Sequential)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "StructLayout(LayoutKind.Sequential)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(true)", "Category(\"Test\")" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "Category(\"Test\")" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(true)", "Category(\"Test\")" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "Category(\"Test\")" };


            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "StructLayout(LayoutKind.Auto)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "StructLayout(LayoutKind.Auto)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "StructLayout(LayoutKind.Auto)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "StructLayout(LayoutKind.Auto)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "StructLayout(LayoutKind.Sequential)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "StructLayout(LayoutKind.Sequential)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "StructLayout(LayoutKind.Sequential)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "StructLayout(LayoutKind.Sequential)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "Category(\"Test\")" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "Category(\"Test\")" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "Category(\"Test\")" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "Category(\"Test\")" };
        }

        #endregion

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task ComVisibleStruct_WarnsWhenIncorrect(AccessibilityContext ctx, string comVisibleAssembly, string comVisibleType, string extraAttribute)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;
                using System.ComponentModel;
                [assembly: {comVisibleAssembly}]
 
                [{comVisibleType}]
                [{extraAttribute}]
                {ctx.AccessCS} struct {ctx.Left}Test{ctx.Right}
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices
                Imports System.ComponentModel

                <Assembly: {comVisibleAssembly}>

                <{comVisibleType}>
                <{extraAttribute}>
                {ctx.AccessVB} Structure {ctx.Left}Test{ctx.Right}
                End Structure");
        }

        [Fact]
        public async Task ComVisibleClass_AutoLayout_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System.Runtime.InteropServices;

                [StructLayout(LayoutKind.Auto)]
                public class Test
                {
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Imports System.Runtime.InteropServices

                <StructLayout(LayoutKind.Auto)>
                Public Class Test
                End Class");
        }
    }
}