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

        private const string attributeImplementationVB = $"""
            Namespace System.Diagnostics.CodeAnalysis
                <System.AttributeUsage(System.AttributeTargets.ReturnValue Or System.AttributeTargets.Parameter)>
                Public Class DoNotIgnoreAttribute
                    Inherits System.Attribute
                    Public Property Message As String
                End Class
            End Namespace
            """;


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
        public async Task AnnotatedMethod_ConsumingReturnValue_NoDiagnostic()
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
    }
}
