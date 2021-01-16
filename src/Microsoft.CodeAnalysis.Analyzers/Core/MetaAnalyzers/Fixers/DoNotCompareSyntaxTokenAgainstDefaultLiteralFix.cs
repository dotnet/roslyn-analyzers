// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Helpers;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers
{
    /// <summary>
    /// RS1034: Prefer token.IsKind(SyntaxKind.None) or token.RawKind == 0 over token == default.
    /// </summary>
    internal abstract class DoNotCompareSyntaxTokenAgainstDefaultLiteralFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.DoNotCompareSyntaxTokenAgainstDefaultLiteralRuleId);

        protected abstract ISyntaxFacts SyntaxFacts { get; }

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CodeAnalysisDiagnosticsResources.DoNotCompareSyntaxTokenAgainstDefaultLiteralCodeFix,
                        cancellationToken => FixAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                        equivalenceKey: nameof(DoNotCompareSyntaxTokenAgainstDefaultLiteralFix)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> FixAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var comparisonNode = root.FindNode(span, getInnermostNodeForTie: true);

            if (!SyntaxFacts.IsBinaryExpression(comparisonNode))
            {
                Debug.Fail("BinaryExpressionSyntax was expected in fixer.");
                return document;
            }

            SyntaxFacts.GetPartsOfBinaryExpression(comparisonNode, out var left, out _, out var right);

            SyntaxNode? tokenNode = null;
            if (SyntaxFacts.IsDefaultLiteralExpression(left))
            {
                tokenNode = right;
            }
            else if (SyntaxFacts.IsDefaultLiteralExpression(right))
            {
                tokenNode = left;
            }
            else
            {
                Debug.Fail("A 'default' literal expression was expected in fixer.");
                return document;
            }

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel.GetMemberGroup(tokenNode, cancellationToken).Any(s => s.Name == DiagnosticWellKnownNames.IsKindName))
            {
                // Offer token.IsKind(SyntaxKind.None)
                // TODO: Negate if the original comparison is !=.
                var generator = SyntaxGenerator.GetGenerator(document);
                var newNode = generator.InvocationExpression(
                    expression: generator.MemberAccessExpression(tokenNode, DiagnosticWellKnownNames.IsKindName),
                    arguments: generator.MemberAccessExpression(generator.IdentifierName(DiagnosticWellKnownNames.SyntaxKindName), DiagnosticWellKnownNames.SyntaxKindNoneName));
                return document.WithSyntaxRoot(root.ReplaceNode(comparisonNode, newNode));
            }
            else
            {
                // Offer token.RawKind == 0
                // TODO: Negate if the original comparison is !=.
                var generator = SyntaxGenerator.GetGenerator(document);
                var newNode = generator.ValueEqualsExpression(
                    left: generator.MemberAccessExpression(tokenNode, DiagnosticWellKnownNames.RawKindName),
                    right: generator.LiteralExpression(0));
                return document.WithSyntaxRoot(root.ReplaceNode(comparisonNode, newNode));
            }
        }
    }
}
