' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicForwardCancellationTokenToInvocationsAnalyzer

        Inherits ForwardCancellationTokenToInvocationsAnalyzer

        Protected Overrides Function GetMethodNameNode(invocationNode As SyntaxNode) As SyntaxNode

            Dim invocationExpression = TryCast(invocationNode, InvocationExpressionSyntax)

            If invocationExpression IsNot Nothing Then

                Dim memberBindingExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If memberBindingExpression IsNot Nothing Then

                    Return memberBindingExpression.Name

                End If

                Return invocationExpression.Expression

            End If

            Return Nothing

        End Function

    End Class

End Namespace




