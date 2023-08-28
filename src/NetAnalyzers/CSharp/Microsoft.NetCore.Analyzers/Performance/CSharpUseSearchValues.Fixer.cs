// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <inheritdoc/>
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpUseSearchValuesFixer : UseSearchValuesFixer
    {
        protected override async ValueTask<(SyntaxNode TypeDeclaration, INamedTypeSymbol? TypeSymbol)> GetTypeSymbolAsync(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            SyntaxNode? typeDeclarationOrCompilationUnit = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();

            typeDeclarationOrCompilationUnit ??= await node.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var typeSymbol = typeDeclarationOrCompilationUnit is TypeDeclarationSyntax typeDeclaration ?
                semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) :
                semanticModel.GetDeclaredSymbol((CompilationUnitSyntax)typeDeclarationOrCompilationUnit, cancellationToken)?.ContainingType;

            return (typeDeclarationOrCompilationUnit, typeSymbol);
        }

        protected override string ReplaceSearchValuesFieldName(string name)
        {
            return SyntaxFactory.Identifier(name).WithAdditionalAnnotations(RenameAnnotation.Create()).ValueText;
        }

        protected override SyntaxNode GetDeclaratorInitializer(SyntaxNode syntax)
        {
            if (syntax is VariableDeclaratorSyntax variableDeclarator)
            {
                return variableDeclarator.Initializer!.Value;
            }

            if (syntax is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration.ExpressionBody!.Expression;
            }

            throw new InvalidOperationException($"Expected 'VariableDeclaratorSyntax' or 'PropertyDeclarationSyntax', got {syntax.GetType().Name}");
        }
    }
}