// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(SourceGeneratorAttributeAnalyzerFix))]
    [Shared]
    public sealed class SourceGeneratorAttributeAnalyzerFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            DiagnosticIds.MissingSourceGeneratorAttributeId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            if (node is null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var codeAction = CodeAction.Create(CodeAnalysisDiagnosticsResources.AddGeneratorAttribute,
                    (cancellationToken) => FixDocumentAsync(document, node, cancellationToken),
                    nameof(SourceGeneratorAttributeAnalyzerFix));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private static async Task<Document> FixDocumentAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;
            var generatorAttribute = generator.Attribute(WellKnownTypeNames.MicrosoftCodeAnalysisGeneratorAttribute);

            editor.ReplaceNode(node, generator.AddAttributes(node, generatorAttribute));

            return editor.GetChangedDocument();
        }
    }
}
