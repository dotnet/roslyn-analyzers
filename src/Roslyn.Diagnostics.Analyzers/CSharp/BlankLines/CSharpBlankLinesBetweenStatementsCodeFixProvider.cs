// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable disable warnings

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Roslyn.Diagnostics.Analyzers;

namespace Roslyn.Diagnostics.CSharp.Analyzers.BlankLines
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpBlankLinesBetweenStatementsCodeFixProvider : CodeFixProvider
    {
        private static readonly SyntaxTrivia s_endOfLine = SyntaxFactory.EndOfLine(Environment.NewLine);

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(RoslynDiagnosticIds.BlankLinesBetweenStatementsRuleId);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var diagnostic = context.Diagnostics.First();
            context.RegisterCodeFix(
                CodeAction.Create(
                    RoslynDiagnosticsAnalyzersResources.Add_blank_line_after_block,
                    c => UpdateDocumentAsync(document, diagnostic, c),
                    RoslynDiagnosticsAnalyzersResources.Add_blank_line_after_block),
                context.Diagnostics);
            return Task.CompletedTask;
        }

        private static async Task<Document> UpdateDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            => document.WithSyntaxRoot(await FixAllAsync(document, ImmutableArray.Create(diagnostic), cancellationToken).ConfigureAwait(false));

        public static async Task<SyntaxNode> FixAllAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var nextTokens = diagnostics.Select(d => root.FindToken(d.Location.SourceSpan.Start).GetNextToken());
            var newRoot = root.ReplaceTokens(
                nextTokens,
                (original, current) => current.WithLeadingTrivia(current.LeadingTrivia.Insert(0, s_endOfLine)));

            return newRoot;
        }
    }
}
