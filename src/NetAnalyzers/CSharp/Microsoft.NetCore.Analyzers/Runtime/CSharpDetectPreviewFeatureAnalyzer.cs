// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Runtime;

// TODO: TypeArgument MyMethodCall<PreviewType>();
// TODO: Preview attribute on method

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDetectPreviewFeatureAnalyzer : DetectPreviewFeatureAnalyzer<BaseTypeSyntax, BaseTypeDeclarationSyntax, TypeConstraintSyntax, TypeArgumentListSyntax, ParameterSyntax>
    {
        private static TypeSyntax GetElementTypeForNullableAndArrayTypeNodes(TypeSyntax parameterType)
        {
            while (parameterType is NullableTypeSyntax nullable)
            {
                parameterType = nullable.ElementType;
            }

            while (parameterType is ArrayTypeSyntax arrayType)
            {
                parameterType = arrayType.ElementType;
            }

            return parameterType;
        }

        private static bool TryMatchGenericSyntaxNodeWithGivenSymbol(GenericNameSyntax genericName, ISymbol previewReturnTypeSymbol, [NotNullWhen(true)] out SyntaxNode? syntaxNode)
        {
            if (IsSyntaxToken(genericName.Identifier, previewReturnTypeSymbol))
            {
                syntaxNode = genericName;
                return true;
            }

            TypeArgumentListSyntax typeArgumentList = genericName.TypeArgumentList;
            foreach (TypeSyntax typeArgument in typeArgumentList.Arguments)
            {
                TypeSyntax typeArgumentElementType = GetElementTypeForNullableAndArrayTypeNodes(typeArgument);
                if (typeArgumentElementType is GenericNameSyntax innerGenericName)
                {
                    if (TryMatchGenericSyntaxNodeWithGivenSymbol(innerGenericName, previewReturnTypeSymbol, out syntaxNode))
                    {
                        return true;
                    }
                }

                if (IsIdentifierNameSyntax(typeArgumentElementType, previewReturnTypeSymbol))
                {
                    syntaxNode = typeArgumentElementType;
                    return true;
                }
            }

            syntaxNode = null;
            return false;
        }

        protected override SyntaxNode? GetConstraintSyntaxNodeForTypeConstrainedByPreviewTypes(ISymbol typeOrMethodSymbol, ISymbol previewInterfaceConstraintSymbol)
        {
            ImmutableArray<SyntaxReference> typeSymbolDeclaringReferences = typeOrMethodSymbol.DeclaringSyntaxReferences;

            foreach (SyntaxReference? syntaxReference in typeSymbolDeclaringReferences)
            {
                SyntaxNode typeOrMethodDefinition = syntaxReference.GetSyntax();
                if (typeOrMethodDefinition is TypeDeclarationSyntax typeDeclaration)
                {
                    // For ex: class A<T> where T : IFoo, new() // where IFoo is preview
                    SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = typeDeclaration.ConstraintClauses;
                    if (TryGetConstraintClauseNode(constraintClauses, previewInterfaceConstraintSymbol, out SyntaxNode? ret))
                    {
                        return ret;
                    }
                }
                else if (typeOrMethodDefinition is MethodDeclarationSyntax methodDeclaration)
                {
                    SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = methodDeclaration.ConstraintClauses;
                    if (TryGetConstraintClauseNode(constraintClauses, previewInterfaceConstraintSymbol, out SyntaxNode? ret))
                    {
                        return ret;
                    }
                }
            }

            return null;
        }

        private static bool TryGetConstraintClauseNode(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, ISymbol previewInterfaceConstraintSymbol, [NotNullWhen(true)] out SyntaxNode? syntaxNode)
        {
            foreach (TypeParameterConstraintClauseSyntax constraintClause in constraintClauses)
            {
                SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = constraintClause.Constraints;
                foreach (TypeParameterConstraintSyntax? constraint in constraints)
                {
                    if (constraint is TypeConstraintSyntax typeConstraintSyntax)
                    {
                        TypeSyntax typeConstraintSyntaxType = typeConstraintSyntax.Type;
                        typeConstraintSyntaxType = GetElementTypeForNullableAndArrayTypeNodes(typeConstraintSyntaxType);
                        if (typeConstraintSyntaxType is GenericNameSyntax generic)
                        {
                            if (TryMatchGenericSyntaxNodeWithGivenSymbol(generic, previewInterfaceConstraintSymbol, out SyntaxNode? previewConstraint))
                            {
                                syntaxNode = previewConstraint;
                                return true;
                            }
                        }

                        if (IsIdentifierNameSyntax(typeConstraintSyntaxType, previewInterfaceConstraintSymbol))
                        {
                            syntaxNode = constraint;
                            return true;
                        }
                    }
                }
            }

            syntaxNode = null;
            return false;
        }

        private static bool IsSyntaxToken(SyntaxToken identifier, ISymbol previewInterfaceSymbol) => identifier.ValueText == previewInterfaceSymbol.Name;

        private static bool IsIdentifierNameSyntax(TypeSyntax identifier, ISymbol previewInterfaceSymbol) => identifier is IdentifierNameSyntax identifierName && IsSyntaxToken(identifierName.Identifier, previewInterfaceSymbol) ||
          identifier is NullableTypeSyntax nullable && IsIdentifierNameSyntax(nullable.ElementType, previewInterfaceSymbol);

        protected override SyntaxNode? GetPreviewImplementsClauseSyntaxNodeForMethodOrProperty(ISymbol methodSymbol, ISymbol previewSymbol)
        {
            throw new System.NotImplementedException();
        }

        protected override void AnalyzeTypeSyntax(
            CompilationStartAnalysisContext context,
            ConcurrentDictionary<ISymbol, (bool isPreview, string? message, string? url)> requiresPreviewFeaturesSymbols,
            Func<ISymbol, bool> symbolIsAnnotatedAsPreview)
        {
            context.RegisterSyntaxNodeAction(context =>
            {
                var node = (NameSyntax)context.Node;
                AnalyzeTypeSyntax(context, node, requiresPreviewFeaturesSymbols, symbolIsAnnotatedAsPreview);
            }, SyntaxKind.GenericName, SyntaxKind.IdentifierName);
        }

        private protected override SyntaxNode AdjustSyntaxNodeForGetSymbol(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.VariableDeclaration))
            {
                var variableDeclarationSyntax = (VariableDeclarationSyntax)node;
                if (variableDeclarationSyntax.Variables.Count > 0)
                {
                    return variableDeclarationSyntax.Variables[0];
                }
            }

            return node;
        }

        protected override bool IsInReturnType(SyntaxNode node)
        {
            var parent = node.Parent;
            while (parent is not null)
            {
                if (parent is LocalFunctionStatementSyntax localFunction)
                {
                    return localFunction.ReturnType.Span.Contains(node.Span);
                }
                else if (parent is MethodDeclarationSyntax method)
                {
                    return method.ReturnType.Span.Contains(node.Span);
                }
                else if (parent is OperatorDeclarationSyntax @operator)
                {
                    return @operator.ReturnType.Span.Contains(node.Span);
                }
                else if (parent is BasePropertyDeclarationSyntax property)
                {
                    return property.Type.Span.Contains(node.Span);
                }
                else if (parent is MemberDeclarationSyntax)
                {
                    return false;
                }

                parent = parent.Parent;
            }

            return false;
        }

        protected override bool IsParameter(SyntaxNode node)
        {
            return node.Parent.IsKind(SyntaxKind.Parameter);
        }
    }
}
