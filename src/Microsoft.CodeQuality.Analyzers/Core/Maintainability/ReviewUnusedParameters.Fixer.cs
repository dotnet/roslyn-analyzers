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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        private sealed class RemoveParameterAction : SolutionChangeAction
        {
            private readonly string _equivalenceKey;

            public RemoveParameterAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey)
                : base(title, createChangedSolution)
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

            public async Task<ImmutableArray<KeyValuePair<DocumentId, SyntaxNode>>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);

                DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                var parametersDeclarartionNode = node.Parent;
                var parameterSymbol = editor.SemanticModel.GetDeclaredSymbol(node);
                // TODO add check for type
                var methodDeclarationNode = parametersDeclarartionNode.Parent;
                ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(methodDeclarationNode);
                var symbolCallerInfos = await SymbolFinder.FindReferencesAsync(symbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

                var nodesToRemoveBuilder = ImmutableArray.CreateBuilder<KeyValuePair<DocumentId, SyntaxNode>>();
                foreach (var symbolCallerInfo in symbolCallerInfos)
                {
                    if (symbolCallerInfo.Locations != null)
                    {
                        foreach (var location in symbolCallerInfo.Locations)
                        {
                            var referencedSymbolNode = location.Location.SourceTree.GetRoot().FindNode(location.Location.SourceSpan).Parent;
                            // TODO this is C# MemberAccessExpressionSyntax. Need to generalize this to both languages
                            if (referencedSymbolNode is MemberAccessExpressionSyntax)
                            {
                                referencedSymbolNode = referencedSymbolNode.Parent;
                            }
                            var localEditor = await DocumentEditor.CreateAsync(location.Document, cancellationToken).ConfigureAwait(false);
                            var operation = localEditor.SemanticModel.GetOperation(referencedSymbolNode, cancellationToken);
                            var arguments = (operation as IObjectCreationOperation)?.Arguments;
                            if (arguments == null)
                            {
                                arguments = (operation as IInvocationOperation)?.Arguments;
                            }

                            if (arguments != null)
                            {
                                foreach (IArgumentOperation argument in arguments)
                                {
                                    if (argument.Parameter.Equals(parameterSymbol))
                                    {
                                        if (argument.ArgumentKind == ArgumentKind.Explicit)
                                        {
                                            nodesToRemoveBuilder.Add(new KeyValuePair<DocumentId, SyntaxNode>(location.Document.Id, referencedSymbolNode.FindNode(argument.Syntax.GetLocation().SourceSpan)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                nodesToRemoveBuilder.Add(new KeyValuePair<DocumentId, SyntaxNode>(document.Id, node));

                var nodesToRemove = nodesToRemoveBuilder.ToImmutable();
                return nodesToRemove;
            }

            public async Task<Solution> RemoveNodes(Solution solution, IEnumerable<KeyValuePair<DocumentId, SyntaxNode>> pairs, CancellationToken cancellationToken)
            {
                // Start removing from bottom to top to keep spans of nodes that are removed later.
                var groupedPairs = pairs.GroupBy(p => p.Key);
                foreach(var group in groupedPairs)
                {
                    DocumentEditor editor = await DocumentEditor.CreateAsync(solution.GetDocument(group.Key), cancellationToken).ConfigureAwait(false);
                    foreach(var value in group.OrderByDescending(v => v.Value.SpanStart))
                    {
                        RemoveNode(editor, value.Value);
                        
                    }
                    solution = solution.WithDocumentSyntaxRoot(group.Key, editor.GetChangedRoot());
                }

                return solution;
            }

            public async Task<Solution> RemoveNodes(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
            {
                var nodesToRemove = await GetNodesToRemoveAsync(document, diagnostic, cancellationToken).ConfigureAwait(false);
                return await RemoveNodes(document.Project.Solution, nodesToRemove, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}