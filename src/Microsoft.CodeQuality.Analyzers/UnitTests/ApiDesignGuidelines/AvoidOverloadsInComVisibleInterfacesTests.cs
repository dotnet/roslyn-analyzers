// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidOverloadsInComVisibleInterfacesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidOverloadsInComVisibleInterfacesAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidOverloadsInComVisibleInterfacesTests
    {
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

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task ComVisibleOverload_WarnsWhenExposed(AccessibilityContext ctx, string comVisibleAssembly, string comVisibleType)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;
                [assembly: {comVisibleAssembly}]
 
                [{comVisibleType}]
                {ctx.AccessCS} interface {ctx.Left}Test{ctx.Right}
                {{
                    void Method();
                    void Method(int x);
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices
                <Assembly: {comVisibleAssembly}>

                <{comVisibleType}>
                {ctx.AccessVB} Interface {ctx.Left}Test{ctx.Right}
                    Sub Method
                    Sub Method(x As Integer)
                End Interface");
        }

        [Fact]
        public async Task Overload_NotComVisible_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public interface Test
                {
                    void Method();

                    [System.Runtime.InteropServices.ComVisible(false)]
                    void Method(int x);
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Interface Test
                    Sub Method

                    <System.Runtime.InteropServices.ComVisible(False)>
                    Sub Method(x As Integer)
                End Interface");
        }

        [Fact]
        public async Task ComVisible_NoOverload_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public interface Test
                {
                    void Method();
                    void Method2();
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Interface Test
                    Sub Method
                    Sub Method2
                End Interface");
        }

        [Fact]
        public async Task ComVisibleGeneric_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public interface Test<T>
                {
                    void Method();
                    void Method(int x);
                }");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Public Interface Test(Of T)
                    Sub Method
                    Sub Method(x As Integer)
                End Interface");
        }


        [Fact]
        public async Task ComVisible_Class_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    public void Method() { }
                    public void Method(int x) { }
                }");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Public Class Test
                    Public Sub Method
                    End Sub
                    Public Sub Method(x As Integer)
                    End Sub
                End Class");
        }
    }
}