// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.AvoidOutParametersAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidOutParametersTests
    {
        [Fact]
        public async Task PublicOut_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    public void GetObj(out object [|o|]) { o = null; }
                }");
        }

        [Fact]
        public async Task PrivateOut_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    private void GetObj(out object o) { o = null; }
                }");
        }

        [Fact]
        public async Task ProtectedOut_Warns()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void GetObj(out object [|o|]) { o = null; }
                }");
        }

        [Fact]
        public async Task PublicVal_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void GiveObj(object o) { o = null; }
                }");

        }

        [Fact]
        public async Task PublicRef_NoWarn()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                public class Class
                {
                    protected void SetAction(ref int o) { o = 0; }
                }");
        }
    }
}