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
        public async Task ZeroOnLeft_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|0 == a.IndexOf("")|];
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
        public async Task ZeroOnLeft_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = [|0 = a.IndexOf("abc")|]
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

        [Fact]
        public async Task StringStringComparison_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|a.IndexOf("abc", System.StringComparison.Ordinal) == 0|];
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = a.StartsWith("abc", System.StringComparison.Ordinal);
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task StringStringComparison_VB_Diagnostic()
        {
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = [|a.IndexOf("abc", System.StringComparison.Ordinal) = 0|]
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = a.StartsWith("abc", System.StringComparison.Ordinal)
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task OutOfOrderNamedArguments_CSharp_Diagnostic()
        {
            var testCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = [|a.IndexOf(comparisonType: System.StringComparison.Ordinal, value: "abc") == 0|];
                    }
                }
                """;

            var fixedCode = """
                class C
                {
                    void M(string a)
                    {
                        _ = a.StartsWith(comparisonType: System.StringComparison.Ordinal, value: "abc");
                    }
                }
                """;

            await VerifyCS.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task OutOfOrderNamedArguments_VB_Diagnostic()
        {
            // IInvocationOperation.Arguments appears to behave differently in C# vs VB.
            // In C#, the order of arguments are preserved, as they appear in source.
            // In VB, the order of arguments is the same as parameters order.
            // If we wanted to make VB behavior similar to OutOfOrderNamedArguments_CSharp_Diagnostic, we will need
            // to go back to syntax. This scenario doesn't seem important/common, so might be good for now until
            // we hear any user feedback.
            var testCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = [|a.IndexOf(comparisonType:=System.StringComparison.Ordinal, value:="abc") = 0|]
                    End Sub
                End Class
                """;

            var fixedCode = """
                Class C
                    Sub M(a As String)
                        Dim unused = a.StartsWith(value:="abc", comparisonType:=System.StringComparison.Ordinal)
                    End Sub
                End Class
                """;

            await VerifyVB.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
