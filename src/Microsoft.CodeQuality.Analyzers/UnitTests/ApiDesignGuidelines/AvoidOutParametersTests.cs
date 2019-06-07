// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidOutParametersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidOutParametersTests
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

        [Fact]
        public async Task PublicVal_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    protected void GiveObj(object o) { o = null; }
                }");

        }

        [Fact]
        public async Task PublicRef_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Test
                {
                    protected void SetAction(ref int o) { o = 0; }
                }");
        }
    }
}