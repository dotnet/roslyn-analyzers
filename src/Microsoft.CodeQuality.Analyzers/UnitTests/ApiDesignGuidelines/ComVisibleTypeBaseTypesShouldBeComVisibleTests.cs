// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComVisibleTypeBaseTypesShouldBeComVisibleAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.ComVisibleTypeBaseTypesShouldBeComVisibleAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class ComVisibleTypeBaseTypesShouldBeComVisibleTests
    {
        #region Test Data

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { new AccessibilityContext(Accessibility.Public, false), "ComVisible(true)", "ComVisible(true)" };
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
        public async Task ComVisibleClass_WarnsWhenIncorrect(AccessibilityContext ctx, string comVisibleType1, string comVisibleType2)
        {
            await VerifyCS.VerifyAnalyzerAsync($@"
                using System.Runtime.InteropServices;

                [{comVisibleType1}]
                {ctx.AccessCS} class Test1
                {{
                }}

                [{comVisibleType2}]
                {ctx.AccessCS} class {ctx.Left}Test2{ctx.Right} : Test1
                {{
                }}");

            await VerifyVB.VerifyAnalyzerAsync($@"
                Imports System.Runtime.InteropServices

                <{comVisibleType1}>
                {ctx.AccessVB} Class Test1
                End Class

                <{comVisibleType2}>
                {ctx.AccessVB} Class {ctx.Left}Test2{ctx.Right}
                    Inherits Test1
                End Class");
        }

        [Fact]
        public async Task ComVisibleInterface_NeverWarns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System.Runtime.InteropServices;

                [ComVisible(false)]
                public interface Test1
                {
                }

                [ComVisible(true)]
                public interface Test2 : Test1
                {
                }
");

            await VerifyVB.VerifyAnalyzerAsync(@"
                Imports System.Runtime.InteropServices

                <ComVisible(False)>
                Public Interface Test1
                End Interface

                <ComVisible(True)>
                Public Interface Test2
                    Inherits Test1
                End Interface
");
        }
    }
}