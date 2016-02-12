// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Analyzer.Utilities;

namespace Microsoft.Maintainability.Analyzers
{
    /// <summary>
    /// CA1823: Avoid unused private fields
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = AvoidUnusedPrivateFieldsAnalyzer.RuleId), Shared]
    public sealed class AvoidUnusedPrivateFieldsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AvoidUnusedPrivateFieldsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);

            if (node == null)
            {
                return;
            }

            // We cannot have multiple overlapping diagnostics of this id. 
            Diagnostic diagnostic = context.Diagnostics.Single();
            context.RegisterCodeFix(
                new RemoveFieldAction(
                    MicrosoftMaintainabilityAnalyzersResources.AvoidUnusedPrivateFieldsMessage,
                    async ct => await RemoveField(context.Document, node, ct).ConfigureAwait(false)),
                diagnostic);

            return;
        }

        private async Task<Document> RemoveField(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            node = editor.Generator.GetDeclaration(node);
            editor.RemoveNode(node);
            return editor.GetChangedDocument();
        }

        private class RemoveFieldAction : DocumentChangeAction
        {
            public RemoveFieldAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => null;
        }
    }
}