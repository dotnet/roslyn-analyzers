// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UseStartsWithInsteadOfIndexOfComparisonWithZero,
    Microsoft.NetCore.Analyzers.Performance.UseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix>;

using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UseStartsWithInsteadOfIndexOfComparisonWithZero,
    Microsoft.NetCore.Analyzers.Performance.UseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public class UseStartsWithInsteadOfIndexOfComparisonWithZeroTests
    {
        [Fact]
        public async Task SimpleScenario_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|a.IndexOf("") == 0|];
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = a.StartsWith("");
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task SimpleScenario_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = [|a.IndexOf("abc") = 0|]
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = a.StartsWith("abc")
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task Negated_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|a.IndexOf("abc") != 0|];
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = !a.StartsWith("abc");
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task Negated_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = [|a.IndexOf("abc") <> 0|]
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = Not a.StartsWith("abc")
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task InArgument_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        System.Console.WriteLine([|a.IndexOf("abc") != 0|]);
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        System.Console.WriteLine(!a.StartsWith("abc"));
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task InArgument_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        System.Console.WriteLine([|a.IndexOf("abc") <> 0|])
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        System.Console.WriteLine(Not a.StartsWith("abc"))
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task FixAll_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|a.IndexOf("abc") != 0|];
                        _ = [|a.IndexOf("abcd") != 0|];
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = !a.StartsWith("abc");
                        _ = !a.StartsWith("abcd");
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task FixAll_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused1 = [|a.IndexOf("abc") <> 0|]
                        Dim unused2 = [|a.IndexOf("abcd") <> 0|]
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        Dim unused1 = Not a.StartsWith("abc")
                        Dim unused2 = Not a.StartsWith("abcd")
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task FixAllNested_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|a.IndexOf(([|"abc2".IndexOf("abc3") == 0|]).ToString()) == 0|];
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = a.StartsWith(("abc2".StartsWith("abc3")).ToString());
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task FixAllNested_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = [|a.IndexOf(([|"abc2".IndexOf("abc3") = 0|]).ToString()) = 0|]
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = a.StartsWith(("abc2".StartsWith("abc3")).ToString())
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
