// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;

namespace Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpProvideObsoleteAttributeMessageAnalyzer : ProvideObsoleteAttributeMessageAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.Attribute);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;

            if (!IsObsoleteAttributeName(StripOffNamespace(attribute.Name).Identifier.Text))
                return;

            //  We bail if the attribute has arguments, unless the first argument is null or empty.
            if (attribute.ArgumentList is not null && attribute.ArgumentList.Arguments.Count > 0)
            {
                if (attribute.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literalExpression)
                    return;
                if (!string.IsNullOrEmpty(literalExpression.Token.Value.ToString()))
                    return;
            }

            string identifierName = string.Empty;
            ISymbol? attributedSymbol = null;

            switch (attribute.Parent.Parent)
            {
                case TypeDeclarationSyntax typeDeclaration:
                    identifierName = typeDeclaration.Identifier.Text;
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken);
                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    identifierName = constructorDeclaration.Identifier.Text;
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(constructorDeclaration, context.CancellationToken);
                    break;
                case FieldDeclarationSyntax fieldDeclaration:
                    identifierName = fieldDeclaration.Declaration.Variables.ToString();
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0], context.CancellationToken);
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    identifierName = propertyDeclaration.Identifier.Text;
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    identifierName = methodDeclaration.Identifier.Text;
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
                    break;
                case EventFieldDeclarationSyntax eventFieldDeclaration:
                    identifierName = eventFieldDeclaration.Declaration.Variables.ToString();
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], context.CancellationToken);
                    break;
                case EventDeclarationSyntax eventDeclaration:
                    identifierName = eventDeclaration.Identifier.Text;
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(eventDeclaration, context.CancellationToken);
                    break;
                case DelegateDeclarationSyntax delegateDeclaration:
                    identifierName = delegateDeclaration.Identifier.Text;
                    attributedSymbol = context.SemanticModel.GetDeclaredSymbol(delegateDeclaration, context.CancellationToken);
                    break;
            }

            if (attributedSymbol is not null && attributedSymbol.DeclaredAccessibility is Accessibility.Public)
            {
                if (attributedSymbol.ContainingType is not null && attributedSymbol.ContainingType.DeclaredAccessibility is not Accessibility.Public)
                    return;

                context.ReportDiagnostic(attribute.CreateDiagnostic(Rule, identifierName));
            }

            return;

            //  Local functions

            static IdentifierNameSyntax StripOffNamespace(NameSyntax name)
            {
                while (name is QualifiedNameSyntax qualifiedName)
                    name = qualifiedName.Right;

                return (IdentifierNameSyntax)name;
            }
        }
    }
}
