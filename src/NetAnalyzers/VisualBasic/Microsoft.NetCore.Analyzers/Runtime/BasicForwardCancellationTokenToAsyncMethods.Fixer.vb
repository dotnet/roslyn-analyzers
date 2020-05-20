' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime

    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicForwardCancellationTokenToAsyncMethodsFixer

        Inherits ForwardCancellationTokenToAsyncMethodsFixer

        Private Shared Function IsCancellationTokenParameter(parameter As ParameterSyntax) As Boolean
            Dim type As SimpleNameSyntax = TryCast(parameter.AsClause.Type, SimpleNameSyntax)
            Return type IsNot Nothing AndAlso type.Identifier.ValueText.Equals(CancellationTokenName, StringComparison.Ordinal)
        End Function

        Private Shared Function GetCancellationTokenName(parameterList As SeparatedSyntaxList(Of ParameterSyntax)) As String
            Return parameterList.FirstOrDefault(Function(p) IsCancellationTokenParameter(p))?.Identifier.Identifier.ValueText
        End Function

        Protected Overrides Function TryGetAncestorDeclarationCancellationTokenParameterName(node As SyntaxNode, ByRef parameterName As String) As Boolean

            parameterName = Nothing

            Dim currentNode As SyntaxNode = node.Parent
            While currentNode IsNot Nothing

                Dim singleLineLambdaNode As SingleLineLambdaExpressionSyntax = TryCast(currentNode, SingleLineLambdaExpressionSyntax)
                Dim multiLineLambdaNode As MultiLineLambdaExpressionSyntax = TryCast(currentNode, MultiLineLambdaExpressionSyntax)
                Dim methodNode As MethodStatementSyntax = TryCast(currentNode, MethodStatementSyntax)
                Dim methodBlockNode As MethodBlockSyntax = TryCast(currentNode, MethodBlockSyntax)

                If singleLineLambdaNode IsNot Nothing Then
                    parameterName = GetCancellationTokenName(singleLineLambdaNode.SubOrFunctionHeader.ParameterList.Parameters)
                    Exit While
                ElseIf multiLineLambdaNode IsNot Nothing Then
                    parameterName = GetCancellationTokenName(multiLineLambdaNode.SubOrFunctionHeader.ParameterList.Parameters)
                    Exit While
                ElseIf methodNode IsNot Nothing Then
                    parameterName = GetCancellationTokenName(methodNode.ParameterList.Parameters)
                    Exit While
                ElseIf methodBlockNode IsNot Nothing Then
                    parameterName = GetCancellationTokenName(methodBlockNode.SubOrFunctionStatement.ParameterList.Parameters)
                    Exit While
                End If

                currentNode = currentNode.Parent

            End While

            Return Not String.IsNullOrEmpty(parameterName)
        End Function

    End Class

End Namespace




