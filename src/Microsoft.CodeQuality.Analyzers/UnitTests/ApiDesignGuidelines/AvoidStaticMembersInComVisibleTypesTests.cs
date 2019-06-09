// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidStaticMembersInComVisibleTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidStaticMembersInComVisibleTypesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidStaticMembersInComVisibleTypesTests
    {
        [Theory]
        [MemberData(nameof(WhenIncorrectTestData))]
        public async Task ComVisibleClass_WarnsWhenIncorrect(AccessibilityContext ctx, string comVisibleAssembly, string comVisibleType, string staticCS, string staticVB)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;
                using System.ComponentModel;
                [assembly: {comVisibleAssembly}]
 
                [{comVisibleType}]
                {ctx.AccessCS} class Test
                {{
                    public {staticCS} int {ctx.Left()}GetNum{ctx.Right()}() => 0;
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices
                Imports System.ComponentModel

                <Assembly: {comVisibleAssembly}>

                <{comVisibleType}>
                {ctx.AccessVB} Class Test
                    Public {staticVB} Function {ctx.Left()}GetNum{ctx.Right()} As Integer
                        Return 0
                    End Function
                End Class");
        }

        [Fact]
        public async Task ComVisibleClass_NonStatic_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System.Runtime.InteropServices;

                public class Test
                {
                    public void Hello() { }
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Imports System.Runtime.InteropServices

                Public Class Test
                    Public Sub Hello
                    End Sub
                End Class");
        }

        [Theory]
        [MemberData(nameof(NonPublicNeverWarnsTestData))]
        public async Task ComVisibleClass_NonPublicStatic_NeverWarns(AccessibilityContext ctx)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                public class Test
                {{
                    {ctx.AccessCS} static void Hello() {{ }}
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                Public Class Test
                    {ctx.AccessVB} Sub Hello
                    End Sub
                End Class");
        }

        #region Test Data

        public static IEnumerable<object[]> WhenIncorrectTestData()
        {
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(true)", "ComVisible(true)", "static", "Shared" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "static", "Shared" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, true), "ComVisible(false)", "ComVisible(true)", "static", "Shared" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "static", "Shared" };

            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(true)", "", "" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(false)", "", "" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(true)", "", "" };
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(false)", "ComVisible(false)", "", "" };


            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "static", "Shared" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "static", "Shared" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "static", "Shared" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "static", "Shared" };

            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(true)", "", "" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(true)", "ComVisible(false)", "", "" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(true)", "", "" };
            yield return new object[] { new AccessibilityContext(Accessibility.Internal, false), "ComVisible(false)", "ComVisible(false)", "", "" };
        }

        public static IEnumerable<object[]> NonPublicNeverWarnsTestData()
        {
            // Don't test public methods, since those warn
            return new AccessibilityTestAttribute(AccessibilityTestTarget.InsideClass).GetData(null).Skip(1);
        }

        #endregion
    }
}