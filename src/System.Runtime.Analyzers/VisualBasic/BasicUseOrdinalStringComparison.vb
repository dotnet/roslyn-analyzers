' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicUseOrdinalStringComparisonAnalyzer
        Inherits UseOrdinalStringComparisonAnalyzer

        Protected Overrides Function GetMethodNameLocation(invocationNode As SyntaxNode) As Location
            Debug.Assert(invocationNode.IsKind(SyntaxKind.InvocationExpression))

            Dim invocation = CType(invocationNode, InvocationExpressionSyntax)
            If invocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) Then
                Return DirectCast(invocation.Expression, MemberAccessExpressionSyntax).Name.GetLocation()
            ElseIf invocation.Expression.IsKind(SyntaxKind.ConditionalAccessExpression) Then
                Return DirectCast(invocation.Expression, ConditionalAccessExpressionSyntax).WhenNotNull.GetLocation()
            End If

            Return invocation.GetLocation()
        End Function

        Protected Overrides Function GetOperatorTokenLocation(binaryOperationNode As SyntaxNode) As Location
            Debug.Assert(TypeOf binaryOperationNode Is BinaryExpressionSyntax)

            Return DirectCast(binaryOperationNode, BinaryExpressionSyntax).OperatorToken.GetLocation()
        End Function
    End Class
End Namespace
