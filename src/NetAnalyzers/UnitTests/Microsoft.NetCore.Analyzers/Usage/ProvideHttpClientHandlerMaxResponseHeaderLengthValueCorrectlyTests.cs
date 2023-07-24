// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public class HttpResponseHeaderTestTests
    {
        [Fact]
        public async Task Test_Amir()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System;
                using System.Net.Http;

                public class TestClass {
                    
                    static int GetValue() => 1414;
                    const int val = 121212 * 2;

                    public void TestMethod() {

                        HttpClientHandler handler3 = new HttpClientHandler()
                        {
                            MaxResponseHeadersLength = GetValue() // Do you really mean 16 MB?
                        };

                        // HttpClientHandler handler = new HttpClientHandler()
                        // {
                        //     MaxResponseHeadersLength = val // Do you really mean 16 MB?
                        // };

                        HttpClientHandler handler2 = new HttpClientHandler()
                        {
                            MaxResponseHeadersLength = 1414 // Do you really mean 16 MB?
                        };
                    }
                }
                        
                ",
            VerifyCS.Diagnostic(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly.RuleId));
        }
    }
}