// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2007: Do not directly await a Task in libraries. Append ConfigureAwait(false) to the task.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class DoNotDirectlyAwaitATaskFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DoNotDirectlyAwaitATaskAnalyzer.RuleId); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode expression = root.FindNode(context.Span);

            if (expression != null)
            {
                context.RegisterCodeFix(
                    new MyCodeAction(MicrosoftApiDesignGuidelinesAnalyzersResources.AppendConfigureAwaitFalse,
                        async ct => await GetFix(context.Document, expression, ct).ConfigureAwait(false)),
                    context.Diagnostics);
            }
        }

        private async Task<Document> GetFix(Document document, SyntaxNode expression, CancellationToken cancellationToken)
        {
            // Rewrite the expression to include a .ConfigureAwait() after it. We reattach trailing trivia to the end.
            // This is especially important for VB, as the end-of-line may be in the trivia
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            SyntaxNode memberAccess = generator.MemberAccessExpression(expression.WithoutTrailingTrivia(), "ConfigureAwait");
            SyntaxNode falseLiteral = generator.FalseLiteralExpression();
            SyntaxNode invocation = generator.InvocationExpression(memberAccess, falseLiteral);
            invocation = invocation.WithLeadingTrivia(expression.GetLeadingTrivia()).WithTrailingTrivia(expression.GetTrailingTrivia());

            editor.ReplaceNode(expression, invocation);
            return editor.GetChangedDocument();
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument) :
                base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => Title;
        }
    }
}
