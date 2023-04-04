// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotIgnoreReturnValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotIgnoreReturnValueCSharpTests
    {
        private readonly DiagnosticDescriptor doNotIgnoreRule = DoNotIgnoreReturnValueAnalyzer.DoNotIgnoreReturnValueRule;
        private readonly DiagnosticDescriptor doNotIgnoreRuleWithMessage = DoNotIgnoreReturnValueAnalyzer.DoNotIgnoreReturnValueRuleWithMessage;

        private const string attributeImplementationCSharp = $$"""
            namespace System.Diagnostics.CodeAnalysis
            {
                [System.AttributeUsage(System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
                internal class DoNotIgnoreAttribute : System.Attribute
                {
                    public DoNotIgnoreAttribute() { }
                    public string Message { get; set; }
                }
            }
            """;

        [Fact]
        public async Task AnnotatedMethod_WithoutReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    void AnnotatedVoid() { }

                    void M()
                    {
                        AnnotatedVoid();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        {|#1:AnnotatedMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_ProducesDiagnostic_WithMessage()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore(Message = "You need this 1")]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        {|#1:AnnotatedMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRuleWithMessage).WithLocation(1).WithArguments("C.AnnotatedMethod()", "You need this 1")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_WithCustomAttribute_NoMessageProperty()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                namespace System.Diagnostics.CodeAnalysis
                {
                    [System.AttributeUsage(System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
                    internal class DoNotIgnoreAttribute : System.Attribute
                    {
                        public DoNotIgnoreAttribute() { }
                    }
                }

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        {|#1:AnnotatedMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_WithCustomAttribute_NonStringMessageProperty()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                namespace System.Diagnostics.CodeAnalysis
                {
                    [System.AttributeUsage(System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
                    internal class DoNotIgnoreAttribute : System.Attribute
                    {
                        public DoNotIgnoreAttribute() { }
                        public int Message { get; set; }
                    }
                }

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        {|#1:AnnotatedMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedMethod()")
            );
        }

        [Fact]
        public async Task UnannotatedMethod_IgnoringReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    int UnannotatedMethod() => 1;

                    void M()
                    {
                        UnannotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_ConsumingReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    int UnannotatedMethod() => 1;

                    void M()
                    {
                        int r = UnannotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_DiscardingReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    int UnannotatedMethod() => 1;

                    void M()
                    {
                        _ = UnannotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_DiscardingReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        _ = AnnotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Assignment()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        int r = AnnotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Return()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    int M()
                    {
                        return AnnotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Invocation()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void Wrap(int wrappedParam) { }

                    void M()
                    {
                        Wrap(AnnotatedMethod());
                    }

                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_IfCondition()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        if (AnnotatedMethod() == 1) { }
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_TernaryCondition()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        bool one = AnnotatedMethod() == 1 ? true : false;
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_SwitchStatement()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        switch (AnnotatedMethod())
                        {
                            default: break;
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_SwitchExpression()
        {
            await new VerifyCS.Test
            {
                LanguageVersion = CSharp.LanguageVersion.CSharp8,
                TestState =
                {
                    Sources =
                    {
                        $$"""
                        {{attributeImplementationCSharp}}

                        class C
                        {
                            [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                            int AnnotatedMethod() => 1;

                            void M()
                            {
                                bool one = AnnotatedMethod() switch
                                {
                                    _ => false,
                                };
                            }
                        }
                        """
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Func()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        System.Func<int> GetValue = () => AnnotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_CastExplicit()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        long r = (long)AnnotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_CastImplicit()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    int AnnotatedMethod() => 1;

                    void M()
                    {
                        long r = AnnotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_AsOperator()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    string AnnotatedMethod() => null;

                    void M()
                    {
                        string r = AnnotatedMethod() as string;
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_IsOperator()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    string AnnotatedMethod() => null;

                    void M()
                    {
                        bool isString = AnnotatedMethod() is string;
                    }
                }
                """);
        }
    }
}
