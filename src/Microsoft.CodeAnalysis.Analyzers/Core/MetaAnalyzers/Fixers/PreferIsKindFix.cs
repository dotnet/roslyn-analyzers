﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers
{
    public abstract class PreferIsKindFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(PreferIsKindAnalyzer.Rule.Id);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CodeAnalysisDiagnosticsResources.PreferIsKindFix,
                        cancellationToken => ConvertKindToIsKindAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                        equivalenceKey: nameof(PreferIsKindFix)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> ConvertKindToIsKindAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            if (TryGetNodeToFix(editor.OriginalRoot, sourceSpan) is { } nodeToFix)
            {
                FixDiagnostic(editor, nodeToFix);
            }

            return editor.GetChangedDocument();
        }

        protected abstract SyntaxNode? TryGetNodeToFix(SyntaxNode root, TextSpan span);

        protected abstract void FixDiagnostic(DocumentEditor editor, SyntaxNode nodeToFix);

        private sealed class CustomFixAllProvider : Analyzer.Utilities.DocumentBasedFixAllProvider
        {
            private readonly PreferIsKindFix _fixer;

            public CustomFixAllProvider(PreferIsKindFix fixer)
            {
                _fixer = fixer;
            }

            protected override string CodeActionTitle => CodeAnalysisDiagnosticsResources.PreferIsKindFix;

            protected override async Task<SyntaxNode> FixAllInDocumentAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
            {
                var editor = await DocumentEditor.CreateAsync(document, fixAllContext.CancellationToken).ConfigureAwait(false);
                foreach (var diagnostic in diagnostics)
                {
                    var nodeToFix = _fixer.TryGetNodeToFix(editor.OriginalRoot, diagnostic.Location.SourceSpan);
                    if (nodeToFix is null)
                        continue;

                    _fixer.FixDiagnostic(editor, nodeToFix);
                }

                return editor.GetChangedRoot();
            }
        }
    }
}
