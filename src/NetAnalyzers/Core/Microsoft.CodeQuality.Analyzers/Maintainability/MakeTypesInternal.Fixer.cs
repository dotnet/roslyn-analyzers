// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    public abstract class MakeTypesInternalFixer : CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var newNode = MakeInternal(node);
            if (newNode is null)
            {
                return;
            }

            root = root.ReplaceNode(node, newNode.WithTriviaFrom(node));

            var codeAction = CodeAction.Create(
                MicrosoftCodeQualityAnalyzersResources.MakeTypesInternalCodeFixTitle,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(root)),
                MicrosoftCodeQualityAnalyzersResources.MakeTypesInternalCodeFixTitle);
            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        protected abstract SyntaxNode? MakeInternal(SyntaxNode node);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(MakeTypesInternal<SymbolKind>.RuleId);
    }
}