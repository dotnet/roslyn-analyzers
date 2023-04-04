// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.NetCore.Analyzers.Runtime;
using Xunit;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.DoNotIgnoreReturnValueAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class DoNotIgnoreReturnValueVBTests
    {
        private readonly DiagnosticDescriptor doNotIgnoreRule = DoNotIgnoreReturnValueAnalyzer.DoNotIgnoreReturnValueRule;
        private readonly DiagnosticDescriptor doNotIgnoreRuleWithMessage = DoNotIgnoreReturnValueAnalyzer.DoNotIgnoreReturnValueRuleWithMessage;

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
        public async Task AnnotatedMethod_IgnoringReturnValue_ProducesDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        {|#1:AnnotatedMethod()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_ProducesDiagnostic_WithMessage()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore(Message := "You need this 1")> Integer
                        Return 1
                    End Function

                    Sub M()
                        {|#1:AnnotatedMethod()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRuleWithMessage).WithLocation(1).WithArguments("C.AnnotatedMethod()", "You need this 1")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_WithCustomAttribute_NoMessageProperty()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                Namespace System.Diagnostics.CodeAnalysis
                    <System.AttributeUsage(System.AttributeTargets.ReturnValue)>
                    Friend Class DoNotIgnoreAttribute
                        Inherits System.Attribute
                    End Class
                End Namespace

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        {|#1:AnnotatedMethod()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedMethod()")
            );
        }

        [Fact]
        public async Task AnnotatedMethod_IgnoringReturnValue_WithCustomAttribute_NonStringMessageProperty()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                Namespace System.Diagnostics.CodeAnalysis
                    <System.AttributeUsage(System.AttributeTargets.ReturnValue)>
                    Friend Class DoNotIgnoreAttribute
                        Inherits System.Attribute
                        Public Property Message As Integer
                    End Class
                End Namespace

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        {|#1:AnnotatedMethod()|}
                    End Sub
                End Class
                """,
                VerifyVB.Diagnostic(doNotIgnoreRule).WithLocation(1).WithArguments("C.AnnotatedMethod()")
            );
        }

        [Fact]
        public async Task UnannotatedMethod_IgnoringReturnValue_NoDiagnostic()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function UnannotatedMethod() As Integer
                        Return 1
                    End Function

                    Sub M()
                        UnannotatedMethod()
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
                    Function UnannotatedMethod() As Integer
                        Return 1
                    End Function

                    Sub M()
                        Dim r As Integer = UnannotatedMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Assignment()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        Dim r As Integer = AnnotatedMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Return()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Function M() As Integer
                        Return AnnotatedMethod()
                    End Function
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Invocation()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub Wrap(wrappedParam As Integer)
                    End Sub

                    Sub M()
                        Wrap(AnnotatedMethod())
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_IfCondition()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        If AnnotatedMethod() = 1 Then
                        End If
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_TernaryCondition()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        Dim one as Boolean = If(AnnotatedMethod() = 1, True, False)
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_SwitchStatement()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        Select Case AnnotatedMethod()
                        End Select
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_Func()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        Dim GetValue As System.Func(Of Integer) = Function() AnnotatedMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_CastExplicit()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        Dim r As Long = CLng(AnnotatedMethod())
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_CastImplicit()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> Integer
                        Return 1
                    End Function

                    Sub M()
                        Dim r As Long = AnnotatedMethod()
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_TryCast()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> String
                        Return Nothing
                    End Function

                    Sub M()
                        Dim r As String = TryCast(AnnotatedMethod(), String)
                    End Sub
                End Class
                """);
        }

        [Fact]
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic_TypeOfOperator()
        {
            await VerifyVB.VerifyAnalyzerAsync($$"""
                {{attributeImplementationVB}}

                Public Class C
                    Function AnnotatedMethod() As <System.Diagnostics.CodeAnalysis.DoNotIgnore> String
                        Return Nothing
                    End Function

                    Sub M()
                        Dim isString As Boolean = TypeOf AnnotatedMethod() Is String
                    End Sub
                End Class
                """);
        }
    }
}
