// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1804: Remove unused locals
    /// </summary>
    public abstract class RemoveUnusedLocalsFixer : CodeFixProvider
    {
        internal const string RuleId = "CA1804";
        private readonly NodesProvider _nodesProvider;

        protected RemoveUnusedLocalsFixer(NodesProvider nodesProvider) => _nodesProvider = nodesProvider;

        public sealed override FixAllProvider GetFixAllProvider() => new RemoveLocalFixAllProvider(_nodesProvider);

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(
                new RemoveLocalAction(
                    MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage,
                    async ct => await _nodesProvider.RemoveNodes(context.Document, diagnostic, ct).ConfigureAwait(false)),
                diagnostic);

            return Task.CompletedTask;
        }

        protected abstract class NodesProvider
        {
            public abstract SyntaxNode GetNodeToRemoveOrReplace(SyntaxNode node);

            public abstract void RemoveAllUnusedLocalDeclarations(HashSet<SyntaxNode> nodesToRemove);

            public async Task<ImmutableArray<SyntaxNode>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);

                DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(node);
                var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

                var nodesToRemoveBuilder = ImmutableArray.CreateBuilder<SyntaxNode>();
                foreach (var referencedSymbol in referencedSymbols)
                {
                    if (referencedSymbol?.Locations != null)
                    {
                        foreach (var location in referencedSymbol.Locations)
                        {
                            var referencedSymbolNode = root.FindNode(location.Location.SourceSpan);
                            if (referencedSymbolNode != null)
                            {
                                var nodeToRemoveOrReplace = GetNodeToRemoveOrReplace(referencedSymbolNode);
                                if (nodeToRemoveOrReplace != null)
                                {
                                    nodesToRemoveBuilder.Add(nodeToRemoveOrReplace);
                                }
                            }
                        }
                    }
                }

                nodesToRemoveBuilder.Add(node);

                var nodesToRemove = nodesToRemoveBuilder.ToImmutable();
                return nodesToRemove;
            }

            public async Task<Document> RemoveNodes(Document document, IEnumerable<SyntaxNode> nodes, CancellationToken cancellationToken)
            {
                DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

                // Start removing from bottom to top to keep spans of nodes that are removed later.
                foreach (var node in nodes.Where(n => n != null).OrderByDescending(n => n.SpanStart))
                {
                    RemoveNode(editor, node);
                }

                return editor.GetChangedDocument();
            }

            public async Task<Document> RemoveNodes(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                var nodesToRemove = await GetNodesToRemoveAsync(document, diagnostic, cancellationToken).ConfigureAwait(false);
                return await RemoveNodes(document, nodesToRemove, cancellationToken).ConfigureAwait(false);
            }

            public abstract void RemoveNode(DocumentEditor editor, SyntaxNode node);
        }

        private sealed class RemoveLocalAction : DocumentChangeAction
        {
            public RemoveLocalAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument) { }

            public override string EquivalenceKey => null;
        }

        private sealed class RemoveLocalFixAllAction : CodeAction
        {
            private readonly NodesProvider _nodesProvider;
            private readonly FixAllContext _fixAllContext;

            public RemoveLocalFixAllAction(FixAllContext fixAllContext, NodesProvider nodesProvider)
            {
                _fixAllContext = fixAllContext;
                _nodesProvider = nodesProvider;
            }

            public override string Title { get => MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage; }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var nodesToRemoveMap = await BuildNodesToRemoveMap(cancellationToken).ConfigureAwait(false);
                Solution newSolution = _fixAllContext.Solution;
                foreach (KeyValuePair<Document, HashSet<SyntaxNode>> pair in nodesToRemoveMap)
                {
                    var document = pair.Key;
                    var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

                    var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
                    var newDocument = await _nodesProvider.RemoveNodes(document, pair.Value, cancellationToken).ConfigureAwait(false);
                    var newRoot = await newDocument.GetSyntaxRootAsync().ConfigureAwait(false);
                    newSolution = newSolution.WithDocumentSyntaxRoot(document.Id, newRoot);
                }

                return newSolution;
            }

            private async Task<Dictionary<Document, HashSet<SyntaxNode>>> BuildNodesToRemoveMap(CancellationToken cancellationToken)
            {
                var nodesToRemoveMap = new Dictionary<Document, HashSet<SyntaxNode>>();
                foreach (var document in _fixAllContext.Project.Documents)
                {
                    ImmutableArray<Diagnostic> diagnostics = await _fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
                    var allNodesToRemove = new HashSet<SyntaxNode>();
                    foreach (var diagnostic in diagnostics)
                    {
                        var nodesToRemove = await _nodesProvider.GetNodesToRemoveAsync(document, diagnostic, cancellationToken).ConfigureAwait(false);
                        if (nodesToRemove != null)
                        {
                            foreach (var nodeToRemove in nodesToRemove)
                            {
                                allNodesToRemove.Add(nodeToRemove);
                            }
                        }
                    }

                    _nodesProvider.RemoveAllUnusedLocalDeclarations(allNodesToRemove);
                    nodesToRemoveMap.Add(document, allNodesToRemove);
                }

                return nodesToRemoveMap;
            }
        }

        private sealed class RemoveLocalFixAllProvider : FixAllProvider
        {
            private readonly NodesProvider _nodesProvider;

            public RemoveLocalFixAllProvider(NodesProvider nodesProvider) => _nodesProvider = nodesProvider;

            public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext) =>
                Task.FromResult((CodeAction)new RemoveLocalFixAllAction(fixAllContext, _nodesProvider));
        }
    }
}