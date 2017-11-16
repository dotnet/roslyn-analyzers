' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.Maintainability

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public NotInheritable Class BasicUseNameofInPlaceOfString
    Inherits UseNameofInPlaceOfStringAnalyzer(Of SyntaxKind)

    Protected Overrides ReadOnly Property ArgumentSyntaxKind As SyntaxKind
        Get
            Return SyntaxKind.SimpleArgument
        End Get
    End Property

    Friend Overrides Function GetIndexOfArgument(argumentList As SyntaxNode, argumentSyntaxNode As SyntaxNode) As Integer
        Dim argumentListSyntax = TryCast(argumentList, ArgumentListSyntax)
        Dim argumentSyntax = TryCast(argumentSyntaxNode, ArgumentSyntax)
        Return argumentListSyntax.Arguments.IndexOf(argumentSyntax)
    End Function

    Friend Overrides Function GetArgumentExpression(argumentList As SyntaxNode) As SyntaxNode
        Dim expression = TryCast(argumentList, ArgumentListSyntax)
        Return expression.Parent
    End Function

    Friend Overrides Function GetArgumentListSyntax(argumentSyntaxNode As SyntaxNode) As SyntaxNode
        Dim argumentSyntax = TryCast(argumentSyntaxNode, ArgumentSyntax)
        Dim argumentListSyntax = TryCast(argumentSyntax.Parent, ArgumentListSyntax)
        Return argumentListSyntax
    End Function

    Friend Overrides Function IsValidIdentifier(stringLiteral As String) As Boolean
        Return SyntaxFacts.IsValidIdentifier(stringLiteral)
    End Function

    Friend Overrides Function TryGetStringLiteralOfExpression(argument As SyntaxNode, ByRef stringLiteral As SyntaxNode, ByRef stringText As String) As Boolean
        Dim argumentSyntax = TryCast(argument, ArgumentSyntax)
        Dim expression = argumentSyntax.GetExpression()
        If (expression Is Nothing) Or (Not expression.IsKind(SyntaxKind.StringLiteralExpression)) Then
            stringLiteral = Nothing
            stringText = ""
            Return False
        End If

        stringLiteral = expression
        Dim literalExpressionSyntax = TryCast(expression, LiteralExpressionSyntax)
        stringText = literalExpressionSyntax.Token.ValueText
        Return True
    End Function

    Friend Overrides Function TryGetNamedArgument(argumentSyntaxNode As SyntaxNode, ByRef argumentName As String) As Boolean
        Dim simpleArgumentSyntax = TryCast(argumentSyntaxNode, SimpleArgumentSyntax)
        If simpleArgumentSyntax.NameColonEquals Is Nothing Then
            argumentName = Nothing
            Return False
        End If

        argumentName = simpleArgumentSyntax.NameColonEquals.Name.Identifier.ValueText
        Return False
    End Function

    Friend Overrides Iterator Function GetParametersInScope(node As SyntaxNode) As IEnumerable(Of String)
        For Each ancestor In node.AncestorsAndSelf()
            Select Case ancestor.Kind
                Case SyntaxKind.MultiLineFunctionLambdaExpression,
                     SyntaxKind.SingleLineFunctionLambdaExpression,
                     SyntaxKind.MultiLineSubLambdaExpression,
                     SyntaxKind.SingleLineSubLambdaExpression
                    Dim parameters = DirectCast(ancestor, LambdaExpressionSyntax).SubOrFunctionHeader.ParameterList.Parameters
                    For Each parameter In parameters
                        Yield DirectCast(parameter.Identifier, ModifiedIdentifierSyntax).Identifier.ValueText
                    Next
                Case SyntaxKind.SubBlock,
                     SyntaxKind.FunctionBlock
                    Dim parameters = DirectCast(ancestor, MethodBlockSyntax).BlockStatement.ParameterList.Parameters
                    For Each parameter In parameters
                        Yield DirectCast(parameter.Identifier, ModifiedIdentifierSyntax).Identifier.ValueText
                    Next
            End Select
        Next

    End Function

    Friend Overrides Iterator Function GetPropertiesInScope(argument As SyntaxNode) As IEnumerable(Of String)
        Dim argumentSyntax = DirectCast(argument, SimpleArgumentSyntax)

        Dim containingTypeBlock = argumentSyntax.FirstAncestorOrSelf(Of SyntaxNode)(Function(ancestor)
                                                                                        Return ancestor.IsKind(SyntaxKind.ClassBlock) OrElse
                                                                                            ancestor.IsKind(SyntaxKind.StructureBlock) OrElse
                                                                                            ancestor.IsKind(SyntaxKind.ModuleBlock)
                                                                                    End Function)

        If containingTypeBlock Is Nothing Then
            Yield String.Empty
        End If

        Dim propertyNodes = containingTypeBlock.ChildNodes.Where(Function(t) t.IsKind(SyntaxKind.PropertyBlock))
        Dim propertyNames As List(Of String) = Nothing
        For Each propertyNode In propertyNodes
            Dim propertyStatementSyntax = DirectCast(propertyNode, PropertyBlockSyntax)
            Yield propertyStatementSyntax.PropertyStatement.Identifier.ValueText
        Next

    End Function
End Class
