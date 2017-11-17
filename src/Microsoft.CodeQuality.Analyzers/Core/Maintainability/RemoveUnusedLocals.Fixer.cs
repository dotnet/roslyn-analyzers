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

        protected abstract SyntaxNode GetAssignmentStatement(SyntaxNode node);

        protected static async Task<List<KeyValuePair<Document, ImmutableArray<Diagnostic>>>> GetDiagnostics(FixAllContext fixAllContext)
        {
            var diagnostics = new List<KeyValuePair<Document, ImmutableArray<Diagnostic>>>();
            foreach (var document in fixAllContext.Project.Documents)
            {
                diagnostics.Add(new KeyValuePair<Document, ImmutableArray<Diagnostic>>(document, await fixAllContext.GetDocumentDiagnosticsAsync(document)));
            }

            return diagnostics;
        }

        internal abstract class NodesProvider
        {
            protected abstract SyntaxNode GetAssignmentStatement(SyntaxNode node);

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

        private class RemoveLocalAction : DocumentChangeAction
        {
            public RemoveLocalAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => null;
        }

        internal abstract class RemoveLocalFixAllAction : CodeAction
        {
            private readonly List<KeyValuePair<Document, ImmutableArray<Diagnostic>>> _diagnosticsToFix;
            private readonly Solution _solution;

            public RemoveLocalFixAllAction(Solution solution, List<KeyValuePair<Document, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                _solution = solution;
                _diagnosticsToFix = diagnosticsToFix;
            }

            public override string Title { get => MicrosoftMaintainabilityAnalyzersResources.RemoveUnusedLocalsMessage; }

            protected abstract void RemoveAllUnusedLocalDeclarations(HashSet<SyntaxNode> nodesToRemove);

            internal abstract NodesProvider GetNodesProvider();

            protected async Task<ImmutableArray<SyntaxNode>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                return await GetNodesProvider().GetNodesToRemoveAsync(document, diagnostic, cancellationToken);
            }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var nodesToRemoveMap = new Dictionary<Document, HashSet<SyntaxNode>>();
                foreach (KeyValuePair<Document, ImmutableArray<Diagnostic>> pair in _diagnosticsToFix)
                {
                    Document document = pair.Key;
                    ImmutableArray<Diagnostic> diagnostics = pair.Value;
                    var allNodesToRemove = new HashSet<SyntaxNode>();
                    foreach (var diagnostic in diagnostics)
                    {
                        var nodesToRemove = await GetNodesToRemoveAsync(document, diagnostic, cancellationToken).ConfigureAwait(false);
                        if (nodesToRemove != null)
                        {
                            foreach (var nodeToRemove in nodesToRemove)
                            {
                                allNodesToRemove.Add(nodeToRemove);
                            }
                        }
                    }

                    RemoveAllUnusedLocalDeclarations(allNodesToRemove);
                    nodesToRemoveMap.Add(document, allNodesToRemove);
                }

                Solution newSolution = _solution;

                foreach (KeyValuePair<Document, HashSet<SyntaxNode>> pair in nodesToRemoveMap)
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
    }
}