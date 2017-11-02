// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1804: Remove unused locals
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = AvoidUnusedPrivateFieldsAnalyzer.RuleId), Shared]
    public abstract class RemoveUnusedLocalsFixer : CodeFixProvider
    {
        internal const string RuleId = "CA1804";

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

            Diagnostic diagnostic = context.Diagnostics.Single();

            DocumentEditor editor = await DocumentEditor.CreateAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
            ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(node);
            var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, context.Document.Project.Solution, context.CancellationToken);
            
            var nodesToRemoveBuilder = ImmutableArray.CreateBuilder<SyntaxNode>();
            foreach (var referencedSymbol in referencedSymbols)
            {
                if (referencedSymbol?.Locations != null)
                {
                    foreach (var location in referencedSymbol.Locations)
                    {
                        if (location != null)
                        {
                            var referencedSymbolNode = root.FindNode(location.Location.SourceSpan);
                            if (referencedSymbolNode != null)
                            {
                                var assignmentNode = GetAssignmentStatement(referencedSymbolNode);
                                if (assignmentNode != null)
                                {
                                    nodesToRemoveBuilder.Add(assignmentNode);
                                }
                            }
                        }
                    }
                }
            }

            var declarationNode = editor.Generator.GetDeclaration(node);
            nodesToRemoveBuilder.Add(node);

            var nodesToRemove = nodesToRemoveBuilder.ToImmutable();

            context.RegisterCodeFix(
                new RemoveLocalAction(
                    MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage,
                    async ct => await RemoveNodes(context.Document, nodesToRemove, ct).ConfigureAwait(false)),
                diagnostic);

            return;
        }

        private async Task<Document> RemoveNodes(Document document, IEnumerable<SyntaxNode> nodes, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            foreach (var node in nodes.Where(n => n != null).OrderByDescending(n => n.SpanStart))
            {
                editor.RemoveNode(node);
            }

            return editor.GetChangedDocument();
        }

        protected abstract SyntaxNode GetAssignmentStatement(SyntaxNode node);

        private class RemoveLocalAction : DocumentChangeAction
        {
            public RemoveLocalAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => null;
        }
    }
}