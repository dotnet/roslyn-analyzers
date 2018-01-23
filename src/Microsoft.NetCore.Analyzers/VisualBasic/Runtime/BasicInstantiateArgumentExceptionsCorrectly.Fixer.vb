' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.NetCore.Analyzers.Runtime
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    ''' <summary>
    ''' CA2208: Instantiate argument exceptions correctly
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicInstantiateArgumentExceptionsCorrectlyFixer
        Inherits InstantiateArgumentExceptionsCorrectlyFixer
        Protected Overrides Function GetParameterUsageAnalysisScope(creation As SyntaxNode) As SyntaxNode
            Dim singleLineIfStatement = creation.FirstAncestorOrSelf(Of SingleLineIfStatementSyntax)()
            If singleLineIfStatement IsNot Nothing Then
                Return singleLineIfStatement.Condition
            End If

            Dim multiLineIfBlock = creation.FirstAncestorOrSelf(Of MultiLineIfBlockSyntax)()
            If multiLineIfBlock IsNot Nothing Then
                Return multiLineIfBlock.IfStatement?.Condition
            End If

            Return Nothing
        End Function

        Protected Overrides Function MoveArgumentToNextParameter(creation As SyntaxNode, argument As SyntaxNode, newArgument As String) As SyntaxNode
            Dim typedCreation = CType(creation, ObjectCreationExpressionSyntax)

            Dim newArgumentNode = SyntaxFactory.SimpleArgument(
                SyntaxFactory.StringLiteralExpression(
                    SyntaxFactory.Literal(newArgument)))

            Dim newArgumentList = typedCreation.ArgumentList.InsertNodesBefore(argument, {newArgumentNode})
            Return typedCreation.WithArgumentList(newArgumentList)
        End Function
    End Class
End Namespace
