// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    /// <summary>
    /// CA1822: Mark members as static
    /// </summary>
    public abstract class MarkMembersAsStaticFixer : CodeFixProvider
    {
        protected abstract IEnumerable<SyntaxNode> GetTypeArguments(SyntaxNode node);
        protected abstract SyntaxNode GetExpressionOfInvocation(SyntaxNode invocation);
        protected virtual SyntaxNode GetSyntaxNodeToReplace(IMemberReferenceOperation memberReference)
            => memberReference.Syntax;

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MarkMembersAsStaticAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            if (node == null)
            {
                return;
            }

            context.RegisterCodeFix(
                new MarkMembersAsStaticAction(
                    MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticCodeFix,
                    ct => MakeStaticAsync(context.Document, root, node, ct)),
                context.Diagnostics);
        }

        private async Task<Solution> MakeStaticAsync(Document document, SyntaxNode root, SyntaxNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Update definition to add static modifier.
            var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            var madeStatic = syntaxGenerator.WithModifiers(node, DeclarationModifiers.Static);
            document = document.WithSyntaxRoot(root.ReplaceNode(node, madeStatic));
            var solution = document.Project.Solution;

            // Update references, if any.
            root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            node = root.DescendantNodes().Single(n => n.SpanStart == node.SpanStart && n.Span.Length == madeStatic.Span.Length);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel.GetDeclaredSymbol(node, cancellationToken);
            if (symbol != null)
            {
                solution = await UpdateReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false);
            }

            return solution;
        }

        private async Task<Solution> UpdateReferencesAsync(ISymbol symbol, Solution solution, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var references = await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false);
            if (references.Count() != 1)
            {
                return solution;
            }

            // Group references by document and fix references in each document.
            foreach (var referenceLocationGroup in references.Single().Locations.GroupBy(r => r.Document))
            {
                // Get document in current solution
                var document = solution.GetDocument(referenceLocationGroup.Key.Id);

                // Skip references in projects with different language.
                // https://github.com/dotnet/roslyn-analyzers/issues/1986 tracks handling them.
                if (!document.Project.Language.Equals(symbol.Language, StringComparison.Ordinal))
                {
                    continue;
                }

                var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                // Compute replacements
                var editor = new SyntaxEditor(root, solution.Workspace);
                foreach (var referenceLocation in referenceLocationGroup)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var referenceNode = root.FindNode(referenceLocation.Location.SourceSpan, getInnermostNodeForTie: true);
                    if (referenceNode != null)
                    {
                        var operation = semanticModel.GetOperationWalkingUpParentChain(referenceNode, cancellationToken);
                        SyntaxNode nodeToReplaceOpt = null;
                        switch (operation)
                        {
                            case IMemberReferenceOperation memberReference:
                                if (IsReplacableOperation(memberReference.Instance))
                                {
                                    nodeToReplaceOpt = GetSyntaxNodeToReplace(memberReference);
                                }

                                break;

                            case IInvocationOperation invocation:
                                if (IsReplacableOperation(invocation.Instance))
                                {
                                    nodeToReplaceOpt = GetExpressionOfInvocation(invocation.Syntax);
                                }

                                break;
                        }

                        if (nodeToReplaceOpt != null)
                        {
                            // Fetch the symbol for the node to replace - note that this might be
                            // different from the original symbol due to generic type arguments.
                            var symbolInfo = semanticModel.GetSymbolInfo(nodeToReplaceOpt, cancellationToken);
                            if (symbolInfo.Symbol == null)
                            {
                                continue;
                            }

                            SyntaxNode memberName;
                            var typeArgumentsOpt = GetTypeArguments(referenceNode);
                            memberName = typeArgumentsOpt != null ?
                                editor.Generator.GenericName(symbolInfo.Symbol.Name, typeArgumentsOpt) :
                                editor.Generator.IdentifierName(symbolInfo.Symbol.Name);

                            var newNode = editor.Generator.MemberAccessExpression(
                                    expression: editor.Generator.TypeExpression(symbolInfo.Symbol.ContainingType),
                                    memberName: memberName)
                                .WithLeadingTrivia(nodeToReplaceOpt.GetLeadingTrivia())
                                .WithTrailingTrivia(nodeToReplaceOpt.GetTrailingTrivia())
                                .WithAdditionalAnnotations(Formatter.Annotation);

                            editor.ReplaceNode(nodeToReplaceOpt, newNode);
                        }
                    }
                }

                document = document.WithSyntaxRoot(editor.GetChangedRoot());
                solution = document.Project.Solution;
            }

            return solution;

            // Local functions.
            bool IsReplacableOperation(IOperation operation)
            {
                // We only replace reference operations whose removal cannot change semantics.
                if (operation != null)
                {
                    switch (operation.Kind)
                    {
                        case OperationKind.InstanceReference:
                        case OperationKind.ParameterReference:
                        case OperationKind.LocalReference:
                            return true;

                        case OperationKind.FieldReference:
                        case OperationKind.PropertyReference:
                            return IsReplacableOperation(((IMemberReferenceOperation)operation).Instance);
                    }
                }

                return false;
            }
        }

        private class MarkMembersAsStaticAction : SolutionChangeAction
        {
            public MarkMembersAsStaticAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution)
                : base(title, createChangedSolution, equivalenceKey: title)
            {
            }
        }
    }
}