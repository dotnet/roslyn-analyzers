' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime

    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicForwardCancellationTokenToAsyncMethodsFixer

        Inherits ForwardCancellationTokenToAsyncMethodsFixer

        Protected Overrides Function TryGetAncestorDeclarationCancellationTokenParameterName(node As SyntaxNode, ByRef parameterName As String) As Boolean

            parameterName = Nothing

            Dim currentNode As SyntaxNode = node.Parent
            Dim parameters As IEnumerable(Of ParameterSyntax) = Nothing
            While currentNode IsNot Nothing

                Dim singleLineLambda As SingleLineLambdaExpressionSyntax = TryCast(currentNode, SingleLineLambdaExpressionSyntax)
                Dim multiLineLambda As MultiLineLambdaExpressionSyntax = TryCast(currentNode, MultiLineLambdaExpressionSyntax)
                Dim methodStatement As MethodStatementSyntax = TryCast(currentNode, MethodStatementSyntax)
                Dim methodBlock As MethodBlockSyntax = TryCast(currentNode, MethodBlockSyntax)

                If singleLineLambda IsNot Nothing Then
                    parameters = singleLineLambda.SubOrFunctionHeader.ParameterList.Parameters
                ElseIf multiLineLambda IsNot Nothing Then
                    parameters = multiLineLambda.SubOrFunctionHeader.ParameterList.Parameters
                ElseIf methodStatement IsNot Nothing Then
                    parameters = methodStatement.ParameterList.Parameters
                ElseIf methodBlock IsNot Nothing Then
                    parameters = methodBlock.SubOrFunctionStatement.ParameterList.Parameters
                End If

                If parameters IsNot Nothing Then
                    parameterName = GetCancellationTokenName(parameters)
                    Exit While
                End If

                currentNode = currentNode.Parent

            End While

            Return Not String.IsNullOrEmpty(parameterName)
        End Function

        Protected Overrides Function IsArgumentNamed(argumentOperation As IArgumentOperation) As Boolean
            Dim argument As SimpleArgumentSyntax = TryCast(argumentOperation.Syntax, SimpleArgumentSyntax)
            Return argument IsNot Nothing AndAlso argument.NameColonEquals IsNot Nothing
        End Function

        Private Shared Function GetCancellationTokenName(parameters As IEnumerable(Of ParameterSyntax)) As String
            Return parameters.Last()?.Identifier.Identifier.ValueText
        End Function

    End Class

End Namespace




