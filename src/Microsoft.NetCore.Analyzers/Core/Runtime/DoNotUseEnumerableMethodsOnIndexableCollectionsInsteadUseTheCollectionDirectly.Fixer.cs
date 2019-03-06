// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    /// </summary>
    public abstract class DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var codeAction = CodeAction.Create("Use indexer", c => UseCollectionDirectly(context.Document, context.Span, c));
            context.RegisterCodeFix(codeAction, diagnostic);

            // Fixer not yet implemented.
            return Task.CompletedTask;

        }

        private async Task<Document> UseCollectionDirectly(Document document, TextSpan span, CancellationToken c)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(c);
            var semanticModel = await document.GetSemanticModelAsync(c);

            var invocationSyntax = GetSyntaxOfType<InvocationExpressionSyntax>(syntaxRoot.FindNode(span));
            var invocationOp = (IInvocationOperation)semanticModel.GetOperation(invocationSyntax);

            //implemented for "Enumerable.First" only
            if (invocationOp.TargetMethod.Name != "First")
            {
                return document;
            }

            var collectionSyntax = GetSyntaxOfType<ExpressionSyntax>(invocationOp.Arguments[0].Syntax);

            var generator = SyntaxGenerator.GetGenerator(document);
            var trailing = collectionSyntax.GetTrailingTrivia();
            collectionSyntax = collectionSyntax.WithTrailingTrivia(SyntaxTriviaList.Empty);
            var indexSyntax = generator.LiteralExpression(0);
            var elementAccessSyntax = generator.ElementAccessExpression(collectionSyntax, indexSyntax).WithTrailingTrivia(trailing);

            var newSyntaxRoot = syntaxRoot.ReplaceNode(invocationSyntax, elementAccessSyntax);
            return document.WithSyntaxRoot(newSyntaxRoot);
        }

        private T GetSyntaxOfType<T>(SyntaxNode node)
        {
            if (!(node is T result))
            {
                result = node
                    .ChildNodes()
                    .OfType<T>()
                    .First();
            }
            return result;
        }
    }
}