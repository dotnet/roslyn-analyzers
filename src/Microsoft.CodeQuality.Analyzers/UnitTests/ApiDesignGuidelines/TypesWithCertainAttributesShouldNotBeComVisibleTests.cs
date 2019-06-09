// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.TypesWithCertainAttributesShouldNotBeComVisibleAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.TypesWithCertainAttributesShouldNotBeComVisibleAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class TypesWithCertainAttributesShouldNotBeComVisibleAnalyzerTests
    {
        const string AutoLayout = TypesWithCertainAttributesShouldNotBeComVisibleAnalyzer.AutoLayoutRuleId;
        const string AutoDual = TypesWithCertainAttributesShouldNotBeComVisibleAnalyzer.AutoDualRuleId;

        [Theory]
        [MemberData(nameof(AutoLayoutTestData))]
        public async Task ComVisibleStruct_WarnsWhenIncorrect(AccessibilityContext ctx, string comVisibleAssembly, string comVisibleType, string extraAttribute)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;
                using System.ComponentModel;
                [assembly: {comVisibleAssembly}]
 
                [{comVisibleType}]
                [{extraAttribute}]
                {ctx.AccessCS} struct {ctx.Left(true, AutoLayout)}Test{ctx.Right(true)}
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices
                Imports System.ComponentModel

                <Assembly: {comVisibleAssembly}>

                <{comVisibleType}>
                <{extraAttribute}>
                {ctx.AccessVB} Structure {ctx.Left(true, AutoLayout)}Test{ctx.Right(true)}
                End Structure");
        }

        [Theory]
        [MemberData(nameof(AutoDualTestData))]
        public async Task AutoDualClass_WarnsWhenIncorrect(AccessibilityContext ctx, string comVisibleAssembly, string comVisibleType, string classInterface)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;
                using System.ComponentModel;
                [assembly: {comVisibleAssembly}]
 
                [{comVisibleType}]
                [{classInterface}]
                {ctx.AccessCS} class {ctx.Left(true, AutoDual)}Test{ctx.Right(true)}
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices
                Imports System.ComponentModel

                <Assembly: {comVisibleAssembly}>

                <{comVisibleType}>
                <{classInterface}>
                {ctx.AccessVB} Class {ctx.Left(true, AutoDual)}Test{ctx.Right(true)}
                End Class");
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

        #region Test Data

        public static IEnumerable<object[]> AutoLayoutTestData()
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

        public static IEnumerable<object[]> AutoDualTestData()
        {
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(true)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDual)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDual)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(false)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDual)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDual)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.None)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.None)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.None)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.None)" };


            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDual)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDual)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDual)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDual)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.AutoDispatch)" };

            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.None)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.None)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "ClassInterface(ClassInterfaceType.None)" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "ClassInterface(ClassInterfaceType.None)" };
        }

        #endregion
    }
}