
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Usage.HttpResponseHeaderTest,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Usage.UnitTests
{
    public class HttpResponseHeaderTestTests
    {
        const int val = 121212;

        [Fact]
        public async Task Test_Amir()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
                using System;
                using System.Net.Http;

                public class C {
                    static int Amir() => 1414;
                const int val = 121212 * 2;
                    public void M() {

                        HttpClientHandler handler3 = new HttpClientHandler()
                        {
                            MaxResponseHeadersLength = Amir() // Do you really mean 16 MB?
                        };

                        HttpClientHandler handler = new HttpClientHandler()
                        {
                            MaxResponseHeadersLength = val // Do you really mean 16 MB?
                        };

                        HttpClientHandler handler2 = new HttpClientHandler()
                        {
                            MaxResponseHeadersLength = 1414 // Do you really mean 16 MB?
                        };
                    }
                }
                        
                ",
            VerifyCS.Diagnostic(HttpResponseHeaderTest.RuleId));
        }

        private static VerifyCS.Test PopulateTestCs(string sourceCode, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = sourceCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp11
            };
            test.ExpectedDiagnostics.AddRange(expected);
            return test;
        }
    }
}