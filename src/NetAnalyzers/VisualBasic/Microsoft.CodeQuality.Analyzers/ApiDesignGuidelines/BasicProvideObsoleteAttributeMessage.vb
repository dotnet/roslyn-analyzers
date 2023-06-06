' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Analyzer.Utilities.Extensions

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicProvideObsoleteAttributeMessageAnalyzer
        Inherits ProvideObsoleteAttributeMessageAnalyzer

        Public Overrides Sub Initialize(context As AnalysisContext)
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)
            context.EnableConcurrentExecution()
            context.RegisterSyntaxNodeAction(AddressOf AnalyzeSyntaxNode, SyntaxKind.Attribute)
        End Sub

        Private Shared Sub AnalyzeSyntaxNode(context As SyntaxNodeAnalysisContext)
            Dim attribute = DirectCast(context.Node, AttributeSyntax)

            If Not IsObsoleteAttributeName(StripOffNamespace(attribute.Name).Identifier.Text) Then
                Return
            End If

            'We bail if the attribute has arguments, unless the first argument is null or empty.
            If attribute.ArgumentList IsNot Nothing AndAlso attribute.ArgumentList.Arguments.Count > 0 Then
                Dim literalExpression = TryCast(attribute.ArgumentList.Arguments(0).GetExpression(), LiteralExpressionSyntax)

                If literalExpression Is Nothing Then
                    Return
                End If

                If Not String.IsNullOrEmpty(literalExpression.Token.Value.ToString()) Then
                    Return
                End If
            End If

            Dim identifierName = String.Empty
            Dim attributedSymbol As ISymbol = Nothing
            Dim declaration = attribute.Parent.Parent

            If TypeOf declaration Is TypeStatementSyntax Then
                Dim typeDeclaration = DirectCast(declaration, TypeStatementSyntax)
                identifierName = typeDeclaration.Identifier.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken)
            ElseIf TypeOf declaration Is SubNewStatementSyntax Then
                Dim subNewStatement = DirectCast(declaration, SubNewStatementSyntax)
                identifierName = subNewStatement.NewKeyword.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(subNewStatement, context.CancellationToken)
            ElseIf TypeOf declaration Is FieldDeclarationSyntax Then
                Dim fieldDeclaration = DirectCast(declaration, FieldDeclarationSyntax)
                identifierName = fieldDeclaration.Declarators(0).Names(0).Identifier.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Declarators(0).Names(0), context.CancellationToken)
            ElseIf TypeOf declaration Is PropertyStatementSyntax Then
                Dim propertyDeclaration = DirectCast(declaration, PropertyStatementSyntax)
                identifierName = propertyDeclaration.Identifier.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken)
            ElseIf TypeOf declaration Is MethodStatementSyntax Then
                Dim methodDeclaration = DirectCast(declaration, MethodStatementSyntax)
                identifierName = methodDeclaration.Identifier.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken)
            ElseIf TypeOf declaration Is EventStatementSyntax Then
                Dim eventDeclaration = DirectCast(declaration, EventStatementSyntax)
                identifierName = eventDeclaration.Identifier.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(eventDeclaration, context.CancellationToken)
            ElseIf TypeOf declaration Is DelegateStatementSyntax Then
                Dim delegateDeclaration = DirectCast(declaration, DelegateStatementSyntax)
                identifierName = delegateDeclaration.Identifier.Text
                attributedSymbol = context.SemanticModel.GetDeclaredSymbol(delegateDeclaration, context.CancellationToken)
            End If

            If attributedSymbol IsNot Nothing AndAlso attributedSymbol.DeclaredAccessibility = Accessibility.Public Then
                If attributedSymbol.ContainingType IsNot Nothing AndAlso attributedSymbol.ContainingType.DeclaredAccessibility <> Accessibility.Public Then
                    Return
                End If

                context.ReportDiagnostic(attribute.CreateDiagnostic(Rule, identifierName))
            End If
        End Sub

        Private Shared Function StripOffNamespace(name As TypeSyntax) As IdentifierNameSyntax
            While TypeOf name Is QualifiedNameSyntax
                name = DirectCast(name, QualifiedNameSyntax).Right
            End While

            Return DirectCast(name, IdentifierNameSyntax)
        End Function
    End Class
End Namespace
