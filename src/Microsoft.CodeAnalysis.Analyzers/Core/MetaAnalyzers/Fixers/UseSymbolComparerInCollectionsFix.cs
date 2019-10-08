// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(UseSymbolComparerInCollectionsFix))]
    [Shared]
    public class UseSymbolComparerInCollectionsFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.UseComparerInSymbolCollectionsRuleId);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        CodeAnalysisDiagnosticsResources.CompareSymbolsCorrectlyCodeFix,
                        cancellationToken => AddDefaultComparerAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                        equivalenceKey: nameof(UseSymbolComparerInCollectionsFix)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> AddDefaultComparerAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var expression = root.FindNode(sourceSpan, getInnermostNodeForTie: true);
            var rawOperation = semanticModel.GetOperation(expression, cancellationToken);

            if (!(rawOperation is IInvocationOperation invocationOperation))
            {
                return document;
            }

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;

            var invocationSyntax = (InvocationExpressionSyntax)invocationOperation.Syntax;

            var newInvocation = invocationSyntax.WithArgumentList(
                invocationSyntax.ArgumentList.AddArguments((ArgumentSyntax)generator.Argument(GetEqualityComparerDefault(generator))));

            editor.ReplaceNode(invocationSyntax, newInvocation);

            return editor.GetChangedDocument();
        }

        private static SyntaxNode GetEqualityComparerDefault(SyntaxGenerator generator)
            => generator.MemberAccessExpression(generator.DottedName(CompareSymbolsCorrectlyAnalyzer.SymbolEqualityComparerName), "Default");
    }
}
