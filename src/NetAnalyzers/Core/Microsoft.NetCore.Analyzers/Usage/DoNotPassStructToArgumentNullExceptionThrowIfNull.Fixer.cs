// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Usage
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public abstract class DoNotPassStructToArgumentNullExceptionThrowIfNullFixer<TInvocationExpression> : CodeFixProvider
        where TInvocationExpression : SyntaxNode
    {
        protected const string HasValue = nameof(Nullable<int>.HasValue);
        protected const string ArgumentNullException = nameof(System.ArgumentNullException);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                if (root.FindNode(context.Span, getInnermostNodeForTie: true) is not TInvocationExpression invocation)
                {
                    continue;
                }

                SyntaxNode? newRoot = null;
                if (diagnostic.Id == DoNotPassStructToArgumentNullExceptionThrowIfNullAnalyzer.NonNullableStructRuleId && invocation.Parent is not null)
                {
                    newRoot = root.RemoveNode(invocation.Parent, SyntaxRemoveOptions.KeepNoTrivia);
                }
                else if (diagnostic.Id == DoNotPassStructToArgumentNullExceptionThrowIfNullAnalyzer.NullableStructRuleId)
                {
                    newRoot = await GetNewRootForNullableStructAsync(context.Document, invocation, context.CancellationToken).ConfigureAwait(false);
                }

                if (newRoot is not null)
                {
                    var codeAction = CodeAction.Create(MicrosoftNetCoreAnalyzersResources.DoNotPassNullableStructToArgumentNullExceptionThrowIfNullCodeFixTitle, _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)), diagnostic.Id);
                    context.RegisterCodeFix(codeAction, diagnostic);
                }
            }
        }

        protected abstract Task<SyntaxNode> GetNewRootForNullableStructAsync(Document document, TInvocationExpression invocation, CancellationToken cancellationToken);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            DoNotPassStructToArgumentNullExceptionThrowIfNullAnalyzer.NonNullableStructRuleId,
            DoNotPassStructToArgumentNullExceptionThrowIfNullAnalyzer.NullableStructRuleId
        );
    }
}