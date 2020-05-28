' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime

    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicForwardCancellationTokenToAsyncMethodsAnalyzer

        Inherits ForwardCancellationTokenToAsyncMethodsAnalyzer

        Protected Overrides Function GetMethodNameNode(invocationNode As SyntaxNode) As SyntaxNode

            Dim invocationExpression = TryCast(invocationNode, InvocationExpressionSyntax)
            If invocationExpression IsNot Nothing Then
                Return invocationExpression.Expression
            End If

            Return Nothing

        End Function

    End Class

End Namespace




