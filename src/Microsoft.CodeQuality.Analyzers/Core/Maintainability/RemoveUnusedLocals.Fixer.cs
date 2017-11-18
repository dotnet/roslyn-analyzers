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
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1804: Remove unused locals
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = AvoidUnusedPrivateFieldsAnalyzer.RuleId), Shared]
    public abstract class RemoveUnusedLocalsFixer : CodeFixProvider
    {
        internal const string RuleId = "CA1804";
        private readonly NodesProvider _nodesProvider;

        protected RemoveUnusedLocalsFixer(NodesProvider nodesProvider) => _nodesProvider = nodesProvider;

        public sealed override FixAllProvider GetFixAllProvider() => new RemoveLocalFixAllProvider(_nodesProvider);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.Single();
            var nodesToRemove = await _nodesProvider.GetNodesToRemoveAsync(context.Document, diagnostic, context.CancellationToken).ConfigureAwait(false);

            context.RegisterCodeFix(
                new RemoveLocalAction(
                    MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage,
                    async ct => await RemoveNodes(context.Document, nodesToRemove, ct).ConfigureAwait(false)),
                diagnostic);

            return;
        }

        private static async Task<Document> RemoveNodes(Document document, IEnumerable<SyntaxNode> nodes, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            foreach (var node in nodes.Where(n => n != null).OrderByDescending(n => n.SpanStart))
            {
                editor.RemoveNode(node);
            }

            return editor.GetChangedDocument();
        }

        protected abstract class NodesProvider
        {
            protected abstract SyntaxNode GetAssignmentStatement(SyntaxNode node);

            public abstract void RemoveAllUnusedLocalDeclarations(HashSet<SyntaxNode> nodesToRemove);

            public async Task<ImmutableArray<SyntaxNode>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);

                DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(node);
                var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, document.Project.Solution, cancellationToken);

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
                                var assignmentNode = GetAssignmentStatement(referencedSymbolNode);
                                if (assignmentNode != null)
                                {
                                    nodesToRemoveBuilder.Add(assignmentNode);
                                }
                            }
                        }
                    }
                }

                var declarationNode = editor.Generator.GetDeclaration(node);
                nodesToRemoveBuilder.Add(node);

                var nodesToRemove = nodesToRemoveBuilder.ToImmutable();
                return nodesToRemove;
            }
        }

        private sealed class RemoveLocalAction : DocumentChangeAction
        {
            public RemoveLocalAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument) { }

            public override string EquivalenceKey => null;
        }

        private sealed class RemoveLocalFixAllAction : CodeAction
        {
            private readonly Solution _solution;
            private readonly Dictionary<Document, HashSet<SyntaxNode>> _nodesToRemoveMap;

            public RemoveLocalFixAllAction(Solution solution, Dictionary<Document, HashSet<SyntaxNode>> nodesToRemoveMap)
            {
                _solution = solution;
                _nodesToRemoveMap = nodesToRemoveMap;
            }

            public override string Title { get => MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage; }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                Solution newSolution = _solution;

                foreach (KeyValuePair<Document, HashSet<SyntaxNode>> pair in _nodesToRemoveMap)
                {
                    var document = pair.Key;
                    var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                    var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
                    var newRoot = syntaxGenerator.RemoveNodes(root, pair.Value.OrderByDescending(n => n.GetLocation().SourceSpan));
                    newSolution = newSolution.WithDocumentSyntaxRoot(document.Id, newRoot);
                }

                return newSolution;
            }
        }

        private sealed class RemoveLocalFixAllProvider : FixAllProvider
        {
            private readonly NodesProvider _nodesProvider;

            public RemoveLocalFixAllProvider(NodesProvider nodesProvider) => _nodesProvider = nodesProvider;

            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var nodesToRemoveMap = new Dictionary<Document, HashSet<SyntaxNode>>();
                foreach (var document in fixAllContext.Project.Documents)
                {
                    ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
                    var allNodesToRemove = new HashSet<SyntaxNode>();
                    foreach (var diagnostic in diagnostics)
                    {
                        var nodesToRemove = await _nodesProvider.GetNodesToRemoveAsync(document, diagnostic, fixAllContext.CancellationToken).ConfigureAwait(false);
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

                return new RemoveLocalFixAllAction(fixAllContext.Solution, nodesToRemoveMap);
            }
        }
    }
}