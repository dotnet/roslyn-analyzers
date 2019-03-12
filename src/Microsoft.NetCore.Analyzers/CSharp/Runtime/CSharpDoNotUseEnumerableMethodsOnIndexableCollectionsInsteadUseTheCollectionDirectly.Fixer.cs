// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NetCore.Analyzers.Runtime;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    /// <summary>
    /// RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic == null)
            {
                return Task.CompletedTask;
            }

            string methodPropertyKey = DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.MethodPropertyKey;
            // The fixer is only implemented for "Enumerable.First"
            if (!diagnostic.Properties.TryGetValue(methodPropertyKey, out var method) || method != "First")
            {
                return Task.CompletedTask;
            }

            string title = SystemRuntimeAnalyzersResources.UseIndexer;

            context.RegisterCodeFix(new MyCodeAction(title,
                                        async ct => await UseCollectionDirectly(context.Document, context.Span, ct).ConfigureAwait(false),
                                        equivalenceKey: title),
                                    diagnostic);

            return Task.CompletedTask;
        }

        private async Task<Document> UseCollectionDirectly(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode invocationNode = root.FindNode(span, getInnermostNodeForTie: true);
            if (!(invocationNode is InvocationExpressionSyntax))
            {
                return document;
            }

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var invocationOp = semanticModel.GetOperation(invocationNode) as IInvocationOperation;
            if (invocationOp == null)
            {
                return document;
            }

            var collectionSyntax = GetSyntaxOfType<ExpressionSyntax>(invocationOp.Arguments[0].Syntax);
            if (collectionSyntax == null)
            {
                return document;
            }

            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
            SyntaxTriviaList trailing = collectionSyntax.GetTrailingTrivia();
            collectionSyntax = collectionSyntax.WithTrailingTrivia(SyntaxTriviaList.Empty);
            SyntaxNode indexNode = generator.LiteralExpression(0);
            SyntaxNode elementAccessNode = generator.ElementAccessExpression(collectionSyntax, indexNode).WithTrailingTrivia(trailing);

            SyntaxNode newRoot = root.ReplaceNode(invocationNode, elementAccessNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private T GetSyntaxOfType<T>(SyntaxNode node)
        {
            if (!(node is T result))
            {
                result = node
                    .ChildNodes()
                    .OfType<T>()
                    .FirstOrDefault();
            }
            return result;
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}