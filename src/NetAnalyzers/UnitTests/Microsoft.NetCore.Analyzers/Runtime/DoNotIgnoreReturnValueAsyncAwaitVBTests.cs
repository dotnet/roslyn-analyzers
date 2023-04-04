// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Test.Utilities;
using Xunit;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotIgnoreReturnValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotIgnoreReturnValueAsyncAwaitVBTests
    {
        private readonly DiagnosticDescriptor doNotIgnoreRule = DoNotIgnoreReturnValueAnalyzer.DoNotIgnoreReturnValueRule;

        private const string attributeImplementationVB = $"""
            Namespace System.Diagnostics.CodeAnalysis
                <System.AttributeUsage(System.AttributeTargets.ReturnValue)>
                Friend Class DoNotIgnoreAttribute
                    Inherits System.Attribute
                    Public Property Message As String
                End Class
            End Namespace
            """;

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/67616")]
        public async Task AnnotatedAsyncMethod_WithoutReturnValue_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncVoid() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task
                    End Function

                    Async Sub M()
                        Await AnnotatedAsyncVoid() ' Until https://github.com/dotnet/roslyn/issues/67616 is fixed,
                                                   ' this will produce a false-positive diagnostic.
                                                   ' It's an authoring mistake to mark AnnotatedAsyncVoid as [DoNotIgnore]
                                                   ' though, so the false-positive is acceptable until that issue is fixed.
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_IgnoringReturnValue_WithoutAwait_ProducesDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        {|#1:AnnotatedAsyncMethod()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedAsyncMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_IgnoringReturnValue_WithAwait_ProducesDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        {|#1:Await AnnotatedAsyncMethod()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedAsyncMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedNonAsyncTask_IgnoringReturnValueWithAwait_ProducesDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedNonAsyncTask() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return System.Threading.Tasks.Task.FromResult(1)
                    End Function

                    Async Sub M()
                        {|#1:Await AnnotatedNonAsyncTask()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedNonAsyncTask()")
            );
        }

        [Fact]
        public async Task AnnotatedNonAsyncValueTask_IgnoringReturnValueWithAwait_ProducesDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedNonAsyncValueTask() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.ValueTask(Of Integer)
                        Return New System.Threading.Tasks.ValueTask(Of Integer)(1)
                    End Function

                    Async Sub M()
                        {|#1:Await AnnotatedNonAsyncValueTask()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedNonAsyncValueTask()")
            );
        }

        [Fact]
        public async Task UnannotatedMethod_IgnoringReturnValue_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function UnannotatedMethod() As System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Await UnannotatedMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task UnannotatedMethod_ConsumingReturnValue_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function UnannotatedMethod() As System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim r As Integer = Await UnannotatedMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingTask_NoDiagnostic_Assignment()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim t As System.Threading.Tasks.Task = AnnotatedAsyncMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Assignment()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim r As Integer = Await AnnotatedAsyncMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Return()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Function M() As System.Threading.Tasks.Task(Of Integer)
                        Return Await AnnotatedAsyncMethod()
                    End Function
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Invocation()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Sub Wrap(wrappedParam As Integer)
                    End Sub

                    Async Sub M()
                        Wrap(Await AnnotatedAsyncMethod())
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_IfCondition()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        If Await AnnotatedAsyncMethod() = 1 Then
                        End If
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_TernaryCondition()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim one As Boolean = If(Await AnnotatedAsyncMethod() = 1, True, False)
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_SwitchStatement()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Select Case Await AnnotatedAsyncMethod()
                        End Select
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_Func()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim GetValue As System.Func(Of System.Threading.Tasks.Task(Of Integer)) = Async Function () Await AnnotatedAsyncMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_CastExplicit()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim r As Long = CLng(Await AnnotatedAsyncMethod())
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_CastImplicit()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Dim r As Long = Await AnnotatedAsyncMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_TryCast()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of String)
                        Return Nothing
                    End Function

                    Async Sub M()
                        Dim r As String = TryCast(Await AnnotatedAsyncMethod(), String)
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic_TypeOfOperator()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function AnnotatedAsyncMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> System.Threading.Tasks.Task(Of String)
                        Return Nothing
                    End Function

                    Async Sub M()
                        Dim isString As Boolean = TypeOf Await AnnotatedAsyncMethod() Is String
                    End Sub
                End Class
                """);
        }
    }
}
