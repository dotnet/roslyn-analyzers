// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferLengthCountIsEmptyOverAnyFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(PreferLengthCountIsEmptyOverAnyAnalyzer.RuleId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            foreach (var diagnostic in context.Diagnostics)
            {
                var propertyName = diagnostic.Properties[PreferLengthCountIsEmptyOverAnyAnalyzer.DiagnosticPropertyKey];
                var (newRoot, codeFixTitle) = propertyName switch
                {
                    PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyText => (ReplaceAnyWithIsEmpty(root, node), MicrosoftNetCoreAnalyzersResources.PreferLengthCountIsEmptyOverAnyCodeFixTitle),
                    PreferLengthCountIsEmptyOverAnyAnalyzer.LengthText => (ReplaceAnyWithLength(root, node), MicrosoftNetCoreAnalyzersResources.PreferLengthCountIsEmptyOverAnyCodeFixTitle),
                    PreferLengthCountIsEmptyOverAnyAnalyzer.CountText => (ReplaceAnyWithCount(root, node), MicrosoftNetCoreAnalyzersResources.PreferLengthCountIsEmptyOverAnyCodeFixTitle),
                    _ => throw new NotSupportedException()
                };
                if (newRoot is null)
                {
                    continue;
                }

                var formattedCodeFixTitle = string.Format(CultureInfo.InvariantCulture, codeFixTitle, propertyName);
                var codeAction = CodeAction.Create(formattedCodeFixTitle, _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)), codeFixTitle);
                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        protected abstract SyntaxNode? ReplaceAnyWithIsEmpty(SyntaxNode root, SyntaxNode node);
        protected abstract SyntaxNode? ReplaceAnyWithLength(SyntaxNode root, SyntaxNode node);
        protected abstract SyntaxNode? ReplaceAnyWithCount(SyntaxNode root, SyntaxNode node);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}