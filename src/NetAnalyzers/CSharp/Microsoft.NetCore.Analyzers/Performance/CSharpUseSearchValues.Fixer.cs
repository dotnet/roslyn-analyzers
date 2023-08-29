// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <inheritdoc/>
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpUseSearchValuesFixer : UseSearchValuesFixer
    {
        protected override async ValueTask<(SyntaxNode TypeDeclaration, INamedTypeSymbol? TypeSymbol)> GetTypeSymbolAsync(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            SyntaxNode? typeDeclarationOrCompilationUnit = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();

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
                return CSharpUseSearchValuesAnalyzer.TryGetPropertyGetterExpression(propertyDeclaration)!;
            }

            throw new InvalidOperationException($"Expected 'VariableDeclaratorSyntax' or 'PropertyDeclarationSyntax', got {syntax.GetType().Name}");
        }

        // new[] { 'a', 'b', 'c' } => "abc"
        // new[] { (byte)'a', (byte)'b', (byte)'c' } => "abc"u8
        protected override SyntaxNode? TryReplaceArrayCreationWithInlineLiteralExpression(IOperation operation)
        {
            if (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
            }

            if (operation is IArrayCreationOperation arrayCreation &&
                arrayCreation.GetElementType() is { } elementType)
            {
                bool isByte = elementType.SpecialType == SpecialType.System_Byte;

                if (isByte &&
                    operation.SemanticModel?.Compilation is CSharpCompilation { LanguageVersion: < LanguageVersion.CSharp11 })
                {
                    // Can't use Utf8StringLiterals
                    return null;
                }

                List<char> values = new();

                if (arrayCreation.Syntax is ExpressionSyntax creationSyntax &&
                    CSharpUseSearchValuesAnalyzer.IsConstantByteOrCharArrayCreationExpression(creationSyntax, values, out _) &&
                    values.Count <= 128 &&                  // Arbitrary limit to avoid emitting huge literals
                    !ContainsAnyComments(creationSyntax))   // Avoid removing potentially valuable comments
                {
                    string valuesString = string.Concat(values);
                    string stringLiteral = SymbolDisplay.FormatLiteral(valuesString, quote: true);

                    return SyntaxFactory.LiteralExpression(
                        isByte ? SyntaxKind.Utf8StringLiteralExpression : SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Token(
                            leading: default,
                            kind: isByte ? SyntaxKind.Utf8StringLiteralToken : SyntaxKind.StringLiteralToken,
                            text: isByte ? $"{stringLiteral}u8" : stringLiteral,
                            valueText: valuesString,
                            trailing: default));
                }
            }

            return null;
        }

        private static bool ContainsAnyComments(SyntaxNode node)
        {
            foreach (SyntaxTrivia trivia in node.DescendantTrivia(node.Span))
            {
                if (trivia.Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia)
                {
                    return true;
                }
            }

            return false;
        }
    }
}