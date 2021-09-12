﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferHashDataOverComputeHashFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(PreferHashDataOverComputeHashAnalyzer.CA1849);
        public abstract override FixAllProvider GetFixAllProvider();
        protected abstract PreferHashDataOverComputeHashFixHelper Helper { get; }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            if (!Helper.TryComputeHashNode(root, diagnostic, out var computeHashSyntaxHolder))
            {
                return;
            }

            if (!diagnostic.Properties.ContainsKey(PreferHashDataOverComputeHashAnalyzer.DeleteHashCreationPropertyKey) ||
                !Helper.TryGetHashCreationNodes(root, diagnostic, out var createHashNode, out var disposeNodes))
            {
                // chained method SHA256.Create().ComputeHash(arg)
                // instance.ComputeHash(arg) xN where N > 1
                var hashInstanceTarget = new HashInstanceTarget(new List<ComputeHashSyntaxHolder> { computeHashSyntaxHolder });
                var codeActionChain = new HashDataCodeAction(context.Document, hashInstanceTarget, Helper, root);
                context.RegisterCodeFix(codeActionChain, diagnostic);
                return;
            }
            else
            {
                var hashInstanceTarget = new HashInstanceTarget(createHashNode, disposeNodes);
                hashInstanceTarget.ComputeHashNodes.Add(computeHashSyntaxHolder);
                var codeAction = new HashDataCodeAction(context.Document, hashInstanceTarget, Helper, root);
                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private sealed class HashDataCodeAction : CodeAction
        {
            private readonly HashInstanceTarget _hashInstanceTarget;
            private readonly PreferHashDataOverComputeHashFixHelper _helper;
            private readonly SyntaxNode _root;
            public HashDataCodeAction(Document document, HashInstanceTarget hashInstanceTarget, PreferHashDataOverComputeHashFixHelper helper, SyntaxNode root)
            {
                Document = document;
                _hashInstanceTarget = hashInstanceTarget;
                _helper = helper;
                _root = root;
            }
            public override string Title => MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle;
            public override string EquivalenceKey => nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle);

            public Document Document { get; }

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var root = _helper.TrackTarget(_root, _hashInstanceTarget);
                root = _helper.FixHashInstanceTarget(root, _hashInstanceTarget);
                root = Formatter.Format(root, Formatter.Annotation, Document.Project.Solution.Workspace, cancellationToken: cancellationToken);

                return Task.FromResult(Document.WithSyntaxRoot(root));
            }
        }

        protected abstract class PreferHashDataOverComputeHashFixAllCodeAction : CodeAction
        {
            private readonly List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> _diagnosticsToFix;
            private readonly Solution _solution;

            protected PreferHashDataOverComputeHashFixAllCodeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                Title = title;
                _solution = solution;
                _diagnosticsToFix = diagnosticsToFix;
            }
            public override string EquivalenceKey => nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle);
            protected abstract PreferHashDataOverComputeHashFixHelper Helper { get; }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var newSolution = _solution;
                foreach (KeyValuePair<Project, ImmutableArray<Diagnostic>> pair in _diagnosticsToFix)
                {
                    Project project = pair.Key;
                    ImmutableArray<Diagnostic> diagnostics = pair.Value;

                    IEnumerable<IGrouping<SyntaxTree, Diagnostic>> groupedDiagnostics =
                        diagnostics
                            .Where(d => d.Location.IsInSource)
                            .GroupBy(d => d.Location.SourceTree);

                    foreach (IGrouping<SyntaxTree, Diagnostic> grouping in groupedDiagnostics)
                    {
                        Document? document = project.GetDocument(grouping.Key);

                        if (document == null)
                        {
                            continue;
                        }
                        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                        var hashInstanceTargets = CollectTargets(root, grouping, cancellationToken);

                        if (hashInstanceTargets is null)
                        {
                            continue;
                        }

                        root = Helper.TrackTargets(root, hashInstanceTargets);

                        root = FixDocumentRoot(root, hashInstanceTargets);
                        root = Formatter.Format(root, Formatter.Annotation, newSolution.Workspace, cancellationToken: cancellationToken);
                        newSolution = document.WithSyntaxRoot(root).Project.Solution;
                    }
                }

                return newSolution;
            }

            public override string Title { get; }

            private HashInstanceTarget[]? CollectTargets(SyntaxNode root, IGrouping<SyntaxTree, Diagnostic> grouping, CancellationToken cancellationToken)
            {
                var dictionary = PooledDictionary<SyntaxNode, HashInstanceTarget>.GetInstance();
                var chainedComputeHashList = new List<ComputeHashSyntaxHolder>();

                if (!CollectNodes())
                {
                    dictionary.Free(cancellationToken);
                    return null;
                }
                var hashInstanceTargets = dictionary.Values.Append(new HashInstanceTarget(chainedComputeHashList)).ToArray();
                dictionary.Free(cancellationToken);
                return hashInstanceTargets;

                bool CollectNodes()
                {
                    foreach (var d in grouping)
                    {
                        if (!Helper.TryComputeHashNode(root, d, out var computeHashSyntaxHolder))
                        {
                            return false;
                        }
                        if (!Helper.TryGetHashCreationNode(root, d, out var createNode, out var hashCreationIndex))
                        {
                            chainedComputeHashList.Add(computeHashSyntaxHolder);
                            continue;
                        }
                        if (!dictionary.TryGetValue(createNode, out HashInstanceTarget hashInstanceTarget))
                        {
                            var disposeNodes = Helper.GetDisposeNodes(root, d, hashCreationIndex);
                            hashInstanceTarget = new HashInstanceTarget(createNode, disposeNodes);
                            dictionary.Add(createNode, hashInstanceTarget);
                        }
                        hashInstanceTarget.ComputeHashNodes.Add(computeHashSyntaxHolder);
                    }
                    return true;
                }
            }

            internal SyntaxNode FixDocumentRoot(SyntaxNode root, HashInstanceTarget[] hashInstanceTargets)
            {
                foreach (var target in hashInstanceTargets)
                {
                    root = Helper.FixHashInstanceTarget(root, target);
                }
                return root;
            }
        }

        protected sealed class HashInstanceTarget
        {
            public HashInstanceTarget(SyntaxNode createNode, SyntaxNode[]? disposeNodes)
            {
                CreateNode = createNode;
                DisposeNodes = disposeNodes;
                ComputeHashNodes = new List<ComputeHashSyntaxHolder>();
            }
            public HashInstanceTarget(List<ComputeHashSyntaxHolder> computeHashNodes)
            {
                CreateNode = null;
                DisposeNodes = null;
                ComputeHashNodes = computeHashNodes;
            }
            public SyntaxNode? CreateNode { get; }
            public List<ComputeHashSyntaxHolder> ComputeHashNodes { get; } = new();
#pragma warning disable CA1819 // Properties should not return arrays
            public SyntaxNode[]? DisposeNodes { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        }

        protected sealed class ComputeHashSyntaxHolder
        {
            public ComputeHashSyntaxHolder(SyntaxNode computeHashNode, PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, string hashTypeName)
            {
                ComputeHashNode = computeHashNode;
                ComputeType = computeType;
                HashTypeName = hashTypeName;
            }
            public SyntaxNode ComputeHashNode { get; }
            public PreferHashDataOverComputeHashAnalyzer.ComputeType ComputeType { get; }
            public string HashTypeName { get; }
        }

        protected abstract class PreferHashDataOverComputeHashFixAllProvider : FixAllProvider
        {
            public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
            {
                var diagnosticsToFix = new List<KeyValuePair<Project, ImmutableArray<Diagnostic>>>();
                string title = MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle;
                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        {
                            ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                            break;
                        }
                    case FixAllScope.Project:
                        {
                            ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(fixAllContext.Project).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                            break;
                        }
                    case FixAllScope.Solution:
                        {
                            foreach (Project project in fixAllContext.Solution.Projects)
                            {
                                ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                                diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));
                            }
                            break;
                        }
                    default:
                        return null;
                }
                return GetCodeAction(title, fixAllContext.Solution, diagnosticsToFix);
            }

            protected abstract PreferHashDataOverComputeHashFixAllCodeAction GetCodeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix);
        }

#pragma warning disable CA1822 // Mark members as static
        protected abstract class PreferHashDataOverComputeHashFixHelper
        {
            public bool TryComputeHashNode(SyntaxNode root, Diagnostic diagnostic, [NotNullWhen(true)] out ComputeHashSyntaxHolder? computeHashHolder)
            {
                if (!Enum.TryParse<PreferHashDataOverComputeHashAnalyzer.ComputeType>(diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.ComputeTypePropertyKey],
                    out var computeType))
                {
                    computeHashHolder = null;
                    return false;
                }
                var computeHashNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                if (computeHashNode is null)
                {
                    computeHashHolder = null;
                    return false;
                }
                var hashTypeName = diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.TargetHashTypeDiagnosticPropertyKey];

                computeHashHolder = new ComputeHashSyntaxHolder(computeHashNode, computeType, hashTypeName);
                return true;
            }

            public bool TryGetHashCreationNodes(SyntaxNode root, Diagnostic diagnostic, [NotNullWhen(true)] out SyntaxNode? createNode, out SyntaxNode[]? disposeNodes)
            {
                if (!TryGetHashCreationNode(root, diagnostic, out createNode, out int hashCreationIndex))
                {
                    disposeNodes = null;
                    return false;
                }

                disposeNodes = GetDisposeNodes(root, diagnostic, hashCreationIndex);
                return true;
            }

            public bool TryGetHashCreationNode(SyntaxNode root, Diagnostic diagnostic, [NotNullWhen(true)] out SyntaxNode? createNode, out int hashCreationIndex)
            {
                if (!diagnostic.Properties.TryGetValue(PreferHashDataOverComputeHashAnalyzer.HashCreationIndexPropertyKey, out var hashCreationIndexPropertyKey) ||
                    !int.TryParse(hashCreationIndexPropertyKey, out hashCreationIndex))
                {
                    createNode = null;
                    hashCreationIndex = default;
                    return false;
                }

                createNode = root.FindNode(diagnostic.AdditionalLocations[hashCreationIndex].SourceSpan);
                return createNode is not null;
            }

            public SyntaxNode[]? GetDisposeNodes(SyntaxNode root, Diagnostic diagnostic, int hashCreationIndex)
            {
                var additionalLocations = diagnostic.AdditionalLocations;
                var disposeCount = additionalLocations.Count - hashCreationIndex - 1;
                if (disposeCount == 0)
                {
                    return null;
                }
                var disposeNodes = new SyntaxNode[disposeCount];

                for (int i = 0; i < disposeNodes.Length; i++)
                {
                    var node = root.FindNode(additionalLocations[hashCreationIndex + i + 1].SourceSpan);
                    if (node is null)
                    {
                        return null;
                    }
                    disposeNodes[i] = node;
                }
                return disposeNodes;
            }

            public SyntaxNode TrackTargets(SyntaxNode root, HashInstanceTarget[] targets)
            {
                var list = new List<SyntaxNode>();
                foreach (var t in targets)
                {
                    if (t.CreateNode is not null)
                    {
                        list.Add(t.CreateNode);
                    }
                    if (t.DisposeNodes is not null)
                    {
                        list.AddRange(t.DisposeNodes);
                    }

                    foreach (var computeNode in t.ComputeHashNodes)
                    {
                        list.Add(computeNode.ComputeHashNode);
                    }
                }

                return root.TrackNodes(list);
            }

            public SyntaxNode TrackTarget(SyntaxNode root, HashInstanceTarget target)
            {
                var list = new List<SyntaxNode>();
                if (target.CreateNode is not null)
                {
                    list.Add(target.CreateNode);
                }
                if (target.DisposeNodes is not null)
                {
                    list.AddRange(target.DisposeNodes);
                }

                foreach (var computeNode in target.ComputeHashNodes)
                {
                    list.Add(computeNode.ComputeHashNode);
                }

                return root.TrackNodes(list);
            }

            public SyntaxNode FixHashInstanceTarget(SyntaxNode root, HashInstanceTarget hashInstanceTarget)
            {
                foreach (var c in hashInstanceTarget.ComputeHashNodes)
                {
                    var tracked = root.GetCurrentNode(c.ComputeHashNode);
                    var hashDataNode = GetHashDataSyntaxNode(c.ComputeType, c.HashTypeName, tracked);
                    root = root.ReplaceNode(tracked, hashDataNode);
                }

                if (hashInstanceTarget.CreateNode is null)
                {
                    return root;
                }

                root = FixHashCreateNode(root, hashInstanceTarget.CreateNode);

                if (hashInstanceTarget.DisposeNodes is null)
                {
                    return root;
                }

                foreach (var disposeNode in hashInstanceTarget.DisposeNodes)
                {
                    var trackedDisposeNode = root.GetCurrentNode(disposeNode);
                    root = root.RemoveNode(trackedDisposeNode, SyntaxRemoveOptions.KeepNoTrivia);
                }
                return root;
            }
            protected abstract SyntaxNode GetHashDataSyntaxNode(PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, string hashTypeName, SyntaxNode computeHashNode);
            protected abstract SyntaxNode FixHashCreateNode(SyntaxNode root, SyntaxNode createNode);
        }
#pragma warning restore CA1822 // Mark members as static
    }
}
