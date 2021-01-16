// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers
{
    /// <summary>
    /// RS1034: Prefer token.IsKind(SyntaxKind.None) or token.RawKind == 0 over token == default.
    /// </summary>
    internal abstract class DoNotCompareSyntaxTokenAgainstDefaultLiteralFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.DoNotCompareSyntaxTokenAgainstDefaultLiteralRuleId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CodeAnalysisDiagnosticsResources.DoNotCompareSyntaxTokenAgainstDefaultLiteralCodeFix,
                        cancellationToken => FixAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                        equivalenceKey: nameof(DoNotCompareSyntaxTokenAgainstDefaultLiteralFix)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private Task<Document> FixAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
