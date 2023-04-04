// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotIgnoreReturnValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotIgnoreReturnValueAsyncCSharpTests
    {
        private readonly DiagnosticDescriptor doNotIgnoreRule = DoNotIgnoreReturnValueAnalyzer.DoNotIgnoreReturnValueRule;

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
        public async Task AnnotatedAsyncMethod_WithoutReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task AnnotatedAsyncVoid() { }

                    async void M()
                    {
                        await AnnotatedAsyncVoid();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_IgnoringReturnValue_WithoutAwait_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        {|#1:AnnotatedAsyncMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedAsyncMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_IgnoringReturnValue_WithAwait_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        {|#1:await AnnotatedAsyncMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedAsyncMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ValueTask_IgnoringReturnValue_WithAwait_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.ValueTask<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        {|#1:await AnnotatedAsyncMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedAsyncMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedNonAsyncTask_IgnoringReturnValueWithAwait_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    System.Threading.Tasks.Task<int> AnnotatedNonAsyncTask() => System.Threading.Tasks.Task.FromResult(1);

                    async void M()
                    {
                        {|#1:await AnnotatedNonAsyncTask()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedNonAsyncTask()")
            );
        }

        [Fact]
        public async Task AnnotatedNonAsyncValueTask_IgnoringReturnValueWithAwait_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    System.Threading.Tasks.ValueTask<int> AnnotatedNonAsyncValueTask() => new System.Threading.Tasks.ValueTask<int>(1);

                    async void M()
                    {
                        {|#1:await AnnotatedNonAsyncValueTask()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedNonAsyncValueTask()")
            );
        }

#if NETCOREAPP
        [Fact]
        public async Task AnnotatedAsyncMethod_TaskLikeType_IgnoringReturnValue_WithAwait_ProducesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(MyTaskMethodBuilder<>))]
                class MyTask<T>
                {
                    public Awaiter<T> GetAwaiter() => throw new System.NotImplementedException();
                }

                class Awaiter<T> : System.Runtime.CompilerServices.INotifyCompletion
                {
                    public bool IsCompleted { get; }
                    public T GetResult() => throw new System.NotImplementedException();
                    public void OnCompleted(System.Action completion) { }
                }

                class MyTaskMethodBuilder<T>
                {
                    public static MyTaskMethodBuilder<T> Create() => throw new System.NotImplementedException();

                    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : System.Runtime.CompilerServices.IAsyncStateMachine { }
                    public void SetStateMachine(System.Runtime.CompilerServices.IAsyncStateMachine stateMachine) { }
                    public void SetException(System.Exception exception) { }
                    public void SetResult(T result) { }

                    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
                        ref TAwaiter awaiter, ref TStateMachine stateMachine)
                        where TAwaiter : System.Runtime.CompilerServices.INotifyCompletion
                        where TStateMachine : System.Runtime.CompilerServices.IAsyncStateMachine
                    {
                    }

                    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
                        ref TAwaiter awaiter, ref TStateMachine stateMachine)
                        where TAwaiter : System.Runtime.CompilerServices.ICriticalNotifyCompletion
                        where TStateMachine : System.Runtime.CompilerServices.IAsyncStateMachine
                    {
                    }

                    public MyTask<T> Task { get; }
                }

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async MyTask<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        {|#1:await AnnotatedAsyncMethod()|};
                    }
                }
                """,
                VerifyCS.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedAsyncMethod()")
            );
        }
#endif

        [Fact]
        public async Task UnannotatedMethod_IgnoringReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    async System.Threading.Tasks.Task<int> UnannotatedMethod() => 1;

                    async void M()
                    {
                        await UnannotatedMethod();
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
                    async System.Threading.Tasks.Task<int> UnannotatedMethod() => 1;

                    async void M()
                    {
                        int r = await UnannotatedMethod();
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
                    async System.Threading.Tasks.Task<int> UnannotatedMethod() => 1;

                    async void M()
                    {
                        _ = await UnannotatedMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_DiscardingReturnValue_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        _ = await AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingTask_NoDiagnostic_Assignment()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        System.Threading.Tasks.Task t = AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Assignment()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        int r = await AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Return()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async System.Threading.Tasks.Task<int> M()
                    {
                        return await AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Invocation()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    void Wrap(int wrappedParam) { }

                    async void M()
                    {
                        Wrap(await AnnotatedAsyncMethod());
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_IfCondition()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        if (await AnnotatedAsyncMethod() == 1) { }
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_TernaryCondition()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        bool one = await AnnotatedAsyncMethod() == 1 ? true : false;
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_SwitchStatement()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        switch (await AnnotatedAsyncMethod())
                        {
                            default: break;
                        }
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_SwitchExpression()
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
                            async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                            async void M()
                            {
                                bool one = await AnnotatedAsyncMethod() switch
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
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Func()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        System.Func<System.Threading.Tasks.Task<int>> GetValue = async () => await AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_CastExplicit()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        long r = (long)await AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_CastImplicit()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<int> AnnotatedAsyncMethod() => 1;

                    async void M()
                    {
                        long r = await AnnotatedAsyncMethod();
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_AsOperator()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<string> AnnotatedAsyncMethod() => null;

                    async void M()
                    {
                        string r = await AnnotatedAsyncMethod() as string;
                    }
                }
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_IsOperator()
        {
            await VerifyCS.VerifyAnalyzerAsync($$"""
                {{attributeImplementationCSharp}}

                class C
                {
                    [return: System.Diagnostics.CodeAnalysis.DoNotIgnore]
                    async System.Threading.Tasks.Task<string> AnnotatedAsyncMethod() => null;

                    async void M()
                    {
                        bool isString = await AnnotatedAsyncMethod() is string;
                    }
                }
                """);
        }
    }
}
