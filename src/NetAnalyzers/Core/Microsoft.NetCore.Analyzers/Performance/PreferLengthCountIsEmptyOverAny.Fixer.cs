// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferLengthCountIsEmptyOverAnyFixer : CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root?.FindNode(context.Span);
            if (node is null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var (newRoot, codeFixTitle) = diagnostic.Id switch
                {
                    PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyId => (ReplaceAnyWithIsEmpty(root!, node), MicrosoftNetCoreAnalyzersResources.PreferIsEmptyOverAnyCodeFixTitle),
                    PreferLengthCountIsEmptyOverAnyAnalyzer.LengthId => (ReplaceAnyWithLength(root!, node), MicrosoftNetCoreAnalyzersResources.PreferLengthOverAnyCodeFixTitle),
                    PreferLengthCountIsEmptyOverAnyAnalyzer.CountId => (ReplaceAnyWithCount(root!, node), MicrosoftNetCoreAnalyzersResources.PreferCountOverAnyCodeFixTitle),
                    _ => throw new ArgumentOutOfRangeException(nameof(context))
                };
                if (newRoot is null)
                {
                    continue;
                }

                var codeAction = CodeAction.Create(codeFixTitle, _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)), codeFixTitle);
                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        protected abstract SyntaxNode? ReplaceAnyWithIsEmpty(SyntaxNode root, SyntaxNode node);
        protected abstract SyntaxNode? ReplaceAnyWithLength(SyntaxNode root, SyntaxNode node);
        protected abstract SyntaxNode? ReplaceAnyWithCount(SyntaxNode root, SyntaxNode node);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            PreferLengthCountIsEmptyOverAnyAnalyzer.LengthId,
            PreferLengthCountIsEmptyOverAnyAnalyzer.CountId,
            PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyId
        );
    }
}