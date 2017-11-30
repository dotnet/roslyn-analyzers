// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1801: Review unused parameters
    /// </summary>
    public abstract class ReviewUnusedParametersFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ReviewUnusedParametersAnalyzer.RuleId);
        private readonly NodesProvider _nodesProvider;

        protected ReviewUnusedParametersFixer(NodesProvider nodesProvider) => _nodesProvider = nodesProvider;

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(
                new RemoveParameterAction(
                    MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersMessage,
                    async ct => await _nodesProvider.RemoveNodes(context.Document, diagnostic, ct).ConfigureAwait(false), diagnostic.Id),
                diagnostic);

            return Task.CompletedTask;
        }

        private sealed class RemoveParameterAction : DocumentChangeAction
        {
            private readonly string _equivalenceKey;

            public RemoveParameterAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument)
            {
                _equivalenceKey = equivalenceKey;
            }

            public override string EquivalenceKey => _equivalenceKey;
        }

        protected abstract class NodesProvider
        {
            public abstract void RemoveAllUnusedLocalDeclarations(HashSet<SyntaxNode> nodesToRemove);

            public abstract void RemoveNode(DocumentEditor editor, SyntaxNode node);

            public abstract SyntaxNode GetParameterNodeToRemove(DocumentEditor editor, SyntaxNode node, string name);

            public async Task<ImmutableArray<SyntaxNode>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);

                DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                var parametersDeclarartionNode = node.Parent;
                var parameterSymbol = editor.SemanticModel.GetDeclaredSymbol(node);
                // TODO add check for type
                var methodDeclarationNode = parametersDeclarartionNode.Parent;
                ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(methodDeclarationNode);
                var symbolCallerInfos = await SymbolFinder.FindCallersAsync(symbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

                var nodesToRemoveBuilder = ImmutableArray.CreateBuilder<SyntaxNode>();
                foreach (var symbolCallerInfo in symbolCallerInfos)
                {
                    if (symbolCallerInfo.Locations != null)
                    {
                        foreach (var location in symbolCallerInfo.Locations)
                        {
                            var referencedSymbolNode = root.FindNode(location.SourceSpan).Parent;
                            var operation = editor.SemanticModel.GetOperation(referencedSymbolNode, cancellationToken);
                            var arguments = (operation as IObjectCreationOperation)?.Arguments;
                            if (arguments == null)
                            {
                                arguments = (operation as IInvocationOperation)?.Arguments;
                            }

                            if (arguments != null)
                            {
                                foreach(IArgumentOperation argument in arguments)
                                {
                                    if (argument.Parameter.Equals(parameterSymbol))
                                    {
                                        if (argument.ArgumentKind == ArgumentKind.Explicit)
                                        {
                                            nodesToRemoveBuilder.Add(referencedSymbolNode.FindNode(argument.Syntax.GetLocation().SourceSpan));
                                        }
                                    }
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
        }
    }
}