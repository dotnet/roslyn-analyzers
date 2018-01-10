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
            // Fixer not yet implemented.
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(
                new MyCodeAction(
                    MicrosoftMaintainabilityAnalyzersResources.ReviewUnusedParametersMessage,
                    async ct => await RemoveNodes(context.Document, diagnostic, ct).ConfigureAwait(false), diagnostic.Id),
                diagnostic);

            return Task.CompletedTask;
        }

        protected abstract SyntaxNode GetParameterNode(SyntaxNode node);

        protected abstract bool CanContinuouslyLeadToObjectCreationOrInvocation(SyntaxNode node);

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

        private ImmutableArray<IArgumentOperation>? GetOperationArguments(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            while (node != null)
            {
                // All nodes in the path should continuously lead to IObjectCreationOperation or IInvocationOperation.
                if (!CanContinuouslyLeadToObjectCreationOrInvocation(node))
                {
                    return null;
                }

                node = node.Parent;

                // For calls like A.B.C(0), it gets nulls for operations for first iterations. 
                var operation = semanticModel.GetOperation(node, cancellationToken);

                var arguments = (operation as IObjectCreationOperation)?.Arguments ?? (operation as IInvocationOperation)?.Arguments;

                if (arguments.HasValue)
                {
                    return arguments.Value;
                }
            }

            // Achieved the root and still could not find the node with parameters.
            // Need to cancel the action.
            return null;
        }

        private async Task<ImmutableArray<KeyValuePair<DocumentId, SyntaxNode>>> GetNodesToRemoveAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);
            node = GetParameterNode(node);

            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            ISymbol parameterSymbol = editor.SemanticModel.GetDeclaredSymbol(node);
            ISymbol methodDeclarationSymbol = parameterSymbol.ContainingSymbol;

            if (!IsSafeMethodToRemoveParameter(methodDeclarationSymbol))
            {
                // See https://github.com/dotnet/roslyn-analyzers/issues/1466
                return ImmutableArray<KeyValuePair<DocumentId, SyntaxNode>>.Empty;
            }

            var nodesToRemove = ImmutableArray.CreateBuilder<KeyValuePair<DocumentId, SyntaxNode>>();
            nodesToRemove.Add(new KeyValuePair<DocumentId, SyntaxNode>(document.Id, node));
            var referencedSymbols = await SymbolFinder.FindReferencesAsync(methodDeclarationSymbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);

            foreach (var referencedSymbol in referencedSymbols)
            {
                if (referencedSymbol.Locations != null)
                {
                    foreach (var referenceLocation in referencedSymbol.Locations)
                    {
                        Location location = referenceLocation.Location;
                        var referenceRoot = location.SourceTree.GetRoot();
                        var referencedSymbolNode = referenceRoot.FindNode(location.SourceSpan);
                        DocumentEditor localEditor = await DocumentEditor.CreateAsync(referenceLocation.Document, cancellationToken).ConfigureAwait(false);
                        var arguments = GetOperationArguments(referencedSymbolNode, localEditor.SemanticModel, cancellationToken);

                        if (arguments != null)
                        {
                            foreach (IArgumentOperation argument in arguments)
                            {
                                // The name comparison below looks fragile. However, symbol comparison does not work for Reduced Extension Methods. Need to consider more reliable options. 
                                if (string.Equals(argument.Parameter.Name, parameterSymbol.Name, StringComparison.Ordinal) && argument.ArgumentKind == ArgumentKind.Explicit)
                                {
                                    nodesToRemove.Add(new KeyValuePair<DocumentId, SyntaxNode>(referenceLocation.Document.Id, referenceRoot.FindNode(argument.Syntax.GetLocation().SourceSpan)));
                                }
                            }
                        }
                    }
                }
            }

            return nodesToRemove.ToImmutable();
        }

        private static bool IsSafeMethodToRemoveParameter(ISymbol methodDeclarationSymbol)
        {
            switch (methodDeclarationSymbol.Kind)
            {
                // Should not fix removing unused property indexer.
                case SymbolKind.Property:
                    return false;
                case SymbolKind.Method:
                    var methodSymbol = methodDeclarationSymbol as IMethodSymbol;
                    // Should not remove parameter for a conversion operator.
                    return (methodSymbol.MethodKind != MethodKind.Conversion);
                default:
                    return true;
            }
        }

        private sealed class MyCodeAction : SolutionChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey)
                : base(title, createChangedSolution, equivalenceKey) { }
        }

        private sealed class RemoveParameterAction : SolutionChangeAction
        {
            public RemoveParameterAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey)
                : base(title, createChangedSolution, equivalenceKey) { }
        }
    }
}
