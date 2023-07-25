// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public class ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectlyTests
    {
        [Fact]
        public async Task CA2262_ProvideCorrectValueFor_HttpClientHandlerMaxResponseHeader_DiagnosticAsync()
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
                            MaxResponseHeadersLength = GetValue()
                        };

                        HttpClientHandler handler = new HttpClientHandler()
                        {
                            {|#0:MaxResponseHeadersLength = val|}
                        };

                        HttpClientHandler handler2 = new HttpClientHandler()
                        {
                            {|#1:MaxResponseHeadersLength = 1414|}
                        };
                    }
                }
                        
                ",
            VerifyCS.Diagnostic(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly.RuleId).WithLocation(0).WithArguments(242424),
            VerifyCS.Diagnostic(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly.RuleId).WithLocation(1).WithArguments(1414)
            );

            await VerifyVB.VerifyAnalyzerAsync(@"
                Imports System.Net.Http
                
                Public Class MainClass
                    Public Shared Sub Main()
                        Dim httpClientHandler As New HttpClientHandler()

                        {|#0:httpClientHandler.MaxResponseHeadersLength = 65536|}

                        Dim httpClient As New HttpClient(httpClientHandler)

                        httpClient.Dispose()
                        httpClientHandler.Dispose()
                    End Sub
                End Class
                ",
            VerifyVB.Diagnostic(ProvideHttpClientHandlerMaxResponseHeaderLengthValueCorrectly.RuleId).WithLocation(0).WithArguments(65536)
            );
        }
    }
}