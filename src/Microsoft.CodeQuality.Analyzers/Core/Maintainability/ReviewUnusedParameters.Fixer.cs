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
                    async ct => await RemoveNodes(context.Document, diagnostic, ct).ConfigureAwait(false), diagnostic.Id),
                diagnostic);

            return Task.CompletedTask;
        }

        protected abstract SyntaxNode GetOperationNode(SyntaxNode node);

        protected abstract SyntaxNode GetParameterNode(SyntaxNode node);

        private async Task<Solution> RemoveNodes(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var pairs = await GetNodesToRemoveAsync(document, diagnostic, cancellationToken).ConfigureAwait(false);
            foreach (var group in pairs.GroupBy(p => p.Key))
            {
                DocumentEditor editor = await DocumentEditor.CreateAsync(solution.GetDocument(group.Key), cancellationToken).ConfigureAwait(false);
                // Start removing from bottom to top to keep spans of nodes that are removed later.
                foreach (var value in group.OrderByDescending(v => v.Value.SpanStart))
                {
                    editor.RemoveNode(value.Value);
                }

                solution = solution.WithDocumentSyntaxRoot(group.Key, editor.GetChangedRoot());
            }

            return solution;
        }

        private async Task<ImmutableArray<KeyValuePair<DocumentId, SyntaxNode>>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);
            node = GetParameterNode(node);

            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var parametersDeclarartionNode = node.Parent;
            var parameterSymbol = editor.SemanticModel.GetDeclaredSymbol(node);
            // TODO add check for type
            var methodDeclarationNode = parametersDeclarartionNode.Parent;
            ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(methodDeclarationNode);
            var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

            var nodesToRemoveBuilder = ImmutableArray.CreateBuilder<KeyValuePair<DocumentId, SyntaxNode>>();
            foreach (var referencedSymbol in referencedSymbols)
            {
                if (referencedSymbol.Locations != null)
                {
                    foreach (var location in referencedSymbol.Locations)
                    {
                        var referencedSymbolNode = location.Location.SourceTree.GetRoot().FindNode(location.Location.SourceSpan).Parent;
                        referencedSymbolNode = GetOperationNode(referencedSymbolNode);
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
    }
}
