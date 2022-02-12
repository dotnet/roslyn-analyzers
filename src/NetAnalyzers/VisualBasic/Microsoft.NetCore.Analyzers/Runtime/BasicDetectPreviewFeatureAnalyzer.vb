' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.NetCore.Analyzers.Runtime
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic
Imports System.Collections.Concurrent

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDetectPreviewFeatureAnalyzer
        Inherits DetectPreviewFeatureAnalyzer(Of InheritsOrImplementsStatementSyntax, TypeBlockSyntax, TypeConstraintSyntax, TypeArgumentListSyntax, ParameterSyntax)

        Private Function IsSyntaxToken(identifier As SyntaxToken, previewInterfaceSymbol As ISymbol) As Boolean
            Return identifier.ValueText.Equals(previewInterfaceSymbol.Name, StringComparison.OrdinalIgnoreCase)
        End Function

        Private Function IsIdentifierNameSyntax(identifier As TypeSyntax, previewInterfaceSymbol As ISymbol) As Boolean
            Dim identifierName = TryCast(identifier, IdentifierNameSyntax)
            If identifierName IsNot Nothing AndAlso IsSyntaxToken(identifierName.Identifier, previewInterfaceSymbol) Then
                Return True
            End If

            Dim nullable = TryCast(identifier, NullableTypeSyntax)
            If nullable IsNot Nothing AndAlso IsIdentifierNameSyntax(nullable.ElementType, previewInterfaceSymbol) Then
                Return True
            End If

            Return False
        End Function

        Protected Overrides Function GetConstraintSyntaxNodeForTypeConstrainedByPreviewTypes(typeOrMethodSymbol As ISymbol, previewInterfaceConstraintSymbol As ISymbol) As SyntaxNode
            Dim typeSymbolDeclaringReferences = typeOrMethodSymbol.DeclaringSyntaxReferences

            For Each syntaxReference In typeSymbolDeclaringReferences
                Dim classStatement = TryCast(syntaxReference.GetSyntax(), ClassStatementSyntax)
                If classStatement IsNot Nothing AndAlso classStatement.TypeParameterList IsNot Nothing Then
                    Return GetSyntaxNodeFromTypeConstraints(classStatement.TypeParameterList, previewInterfaceConstraintSymbol)
                End If

                Dim methodDeclaration = TryCast(syntaxReference.GetSyntax(), MethodStatementSyntax)
                If methodDeclaration IsNot Nothing AndAlso methodDeclaration.TypeParameterList IsNot Nothing Then
                    Return GetSyntaxNodeFromTypeConstraints(methodDeclaration.TypeParameterList, previewInterfaceConstraintSymbol)
                End If
            Next
            Return Nothing
        End Function

        Private Function GetSyntaxNodeFromTypeConstraints(typeParameters As TypeParameterListSyntax, previewSymbol As ISymbol) As SyntaxNode
            For Each typeParameter In typeParameters.Parameters
                Dim singleConstraint = TryCast(typeParameter.TypeParameterConstraintClause, TypeParameterSingleConstraintClauseSyntax)
                If singleConstraint IsNot Nothing Then
                    Return GetTypeConstraints(singleConstraint.Constraint, previewSymbol)
                End If

                Dim multipleConstraint = TryCast(typeParameter.TypeParameterConstraintClause, TypeParameterMultipleConstraintClauseSyntax)
                If multipleConstraint IsNot Nothing Then
                    For Each constraint In multipleConstraint.Constraints
                        Dim constraintSyntax = GetTypeConstraints(constraint, previewSymbol)
                        If constraintSyntax IsNot Nothing Then
                            Return constraintSyntax
                        End If
                    Next
                End If
            Next

            Return Nothing
        End Function

        Private Function GetTypeConstraints(constraint As ConstraintSyntax, previewSymbol As ISymbol) As SyntaxNode
            Dim typeConstraint = TryCast(constraint, TypeConstraintSyntax)
            If typeConstraint IsNot Nothing AndAlso IsIdentifierNameSyntax(typeConstraint.Type, previewSymbol) Then
                Return typeConstraint.Type
            End If

            Return Nothing
        End Function

        Private Function GetSyntaxNodeFromImplementsClause(implementsClause As ImplementsClauseSyntax, previewSymbol As ISymbol) As SyntaxNode
            For Each parameter In implementsClause.InterfaceMembers
                Dim interfacePart = TryCast(parameter.Left, IdentifierNameSyntax)
                If interfacePart IsNot Nothing Then
                    If IsSyntaxToken(interfacePart.Identifier, previewSymbol) Then
                        Return interfacePart
                    End If
                End If

                Dim methodPart = TryCast(parameter.Right, IdentifierNameSyntax)
                If methodPart IsNot Nothing Then
                    If IsSyntaxToken(methodPart.Identifier, previewSymbol) Then
                        Return methodPart
                    End If
                End If
            Next

            Return Nothing
        End Function

        Protected Overrides Function GetPreviewImplementsClauseSyntaxNodeForMethodOrProperty(methodOrPropertySymbol As ISymbol, previewSymbol As ISymbol) As SyntaxNode
            Dim methodSymbolDeclaringReferences = methodOrPropertySymbol.DeclaringSyntaxReferences

            For Each syntaxReference In methodSymbolDeclaringReferences
                Dim methodOrPropertyDefinition = syntaxReference.GetSyntax()
                Dim methodDeclaration = TryCast(methodOrPropertyDefinition, MethodStatementSyntax)
                If methodDeclaration IsNot Nothing Then
                    Dim node = GetSyntaxNodeFromImplementsClause(methodDeclaration.ImplementsClause, previewSymbol)
                    If node IsNot Nothing Then
                        Return node
                    End If
                End If

                Dim propertyDeclaration = TryCast(methodOrPropertyDefinition, PropertyStatementSyntax)
                If propertyDeclaration IsNot Nothing Then
                    Return GetSyntaxNodeFromImplementsClause(propertyDeclaration.ImplementsClause, previewSymbol)
                End If
            Next

            Return Nothing
        End Function

        Protected Overrides Sub AnalyzeTypeSyntax(context As CompilationStartAnalysisContext, requiresPreviewFeaturesSymbols As ConcurrentDictionary(Of ISymbol, (isPreview As Boolean, message As String, url As String)), symbolIsAnnotatedAsPreview As Func(Of ISymbol, Boolean))
            context.RegisterSyntaxNodeAction(
                Sub(syntaxNodeContext)
                    Dim node = DirectCast(syntaxNodeContext.Node, NameSyntax)
                    AnalyzeTypeSyntax(syntaxNodeContext, node, requiresPreviewFeaturesSymbols, symbolIsAnnotatedAsPreview)
                End Sub, SyntaxKind.CrefOperatorReference, SyntaxKind.GlobalName, SyntaxKind.QualifiedCrefOperatorReference, SyntaxKind.QualifiedName, SyntaxKind.GenericName, SyntaxKind.IdentifierName)
        End Sub

        Private Protected Overrides Function AdjustSyntaxNodeForGetSymbol(node As SyntaxNode) As SyntaxNode
            Dim declarator = TryCast(node, VariableDeclaratorSyntax)
            If declarator IsNot Nothing AndAlso declarator.Names.Count > 0 Then
                Return declarator.Names(0)
            End If
            Return node
        End Function

        Protected Overrides Function IsInReturnType(node As SyntaxNode) As Boolean
            Dim simpleAsClause = node.FirstAncestorOrSelf(Of SimpleAsClauseSyntax)()
            Return simpleAsClause IsNot Nothing AndAlso CanHaveReturnType(simpleAsClause.Parent)
        End Function

        Private Shared Function CanHaveReturnType(node As SyntaxNode) As Boolean
            Return node.IsKind(SyntaxKind.FunctionStatement) OrElse node.IsKind(SyntaxKind.DeclareFunctionStatement) OrElse node.IsKind(SyntaxKind.DelegateFunctionStatement) _
                OrElse node.IsKind(SyntaxKind.EventStatement) OrElse node.IsKind(SyntaxKind.OperatorStatement) OrElse node.IsKind(SyntaxKind.PropertyStatement)
        End Function

        Protected Overrides Function IsParameter(node As SyntaxNode) As Boolean
            Return node.Parent.IsKind(SyntaxKind.SimpleAsClause) AndAlso node.Parent.Parent.IsKind(SyntaxKind.Parameter)
        End Function
    End Class

End Namespace