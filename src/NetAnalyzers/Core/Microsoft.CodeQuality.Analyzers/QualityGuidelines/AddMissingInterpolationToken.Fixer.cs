// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public abstract class AbstractAddMissingInterpolationTokenFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AbstractAddMissingInterpolationTokenAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);
            context.RegisterCodeFix(
                new MyCodeAction(_ => AddToken(context.Document, root, node)),
                context.Diagnostics);
        }

        private Task<Document> AddToken(Document document, SyntaxNode root, SyntaxNode node)
        {
            var newNode = GetReplacement(node);
            root = root.ReplaceNode(node, newNode);
            return Task.FromResult(document.WithSyntaxRoot(root));
        }

        private protected abstract SyntaxNode GetReplacement(SyntaxNode node);

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenCodeFixTitle,
                      createChangedDocument,
                      nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenCodeFixTitle))
            {
            }
        }
    }
}
