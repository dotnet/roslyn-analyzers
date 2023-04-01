// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotIgnoreReturnValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotIgnoreReturnValueAsyncVBTests
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

        [Fact]
        public async Task UnannotatedAsyncMethod_IgnoringReturnValue_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function UnannotatedAsyncMethod() As System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function

                    Async Sub M()
                        Await UnannotatedAsyncMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task UnannotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Async Function UnannotatedAsyncMethod() As System.Threading.Tasks.Task(Of Integer)
                        Return 1
                    End Function
                
                    Async Sub M()
                        Dim r As Integer = Await UnannotatedAsyncMethod()
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
        public async Task AnnotatedAsyncMethod_ConsumingReturnValue_NoDiagnostic()
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
    }
}
