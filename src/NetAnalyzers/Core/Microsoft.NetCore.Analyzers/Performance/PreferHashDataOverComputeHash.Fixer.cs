// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferHashDataOverComputeHashFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(PreferHashDataOverComputeHashAnalyzer.CA1849);
        public override FixAllProvider? GetFixAllProvider() => null;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            if (!Enum.TryParse<PreferHashDataOverComputeHashAnalyzer.ComputeType>(diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.ComputeTypePropertyKey],
                out var computeType))
            {
                return;
            }
            var computeHashNode = root.FindNode(context.Span, getInnermostNodeForTie: true);
            if (computeHashNode is null)
            {
                return;
            }

            var hashTypeName = diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.TargetHashTypeDiagnosticPropertyKey];
            var hashDataNode = GetHashDataSyntaxNode(computeType, hashTypeName, computeHashNode);
            if (hashDataNode is null)
            {
                return;
            }

            if (!diagnostic.Properties.ContainsKey(PreferHashDataOverComputeHashAnalyzer.DeleteHashCreationPropertyKey))
            {
                // chained method SHA256.Create().ComputeHash(arg)
                // instance.ComputeHash(arg) xN where N > 1
                var codeActionChain = new ReplaceNodeHashDataCodeAction(context.Document,
                    computeHashNode,
                    hashDataNode);
                context.RegisterCodeFix(codeActionChain, diagnostic);
                return;
            }

            if (!int.TryParse(diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.HashCreationIndexPropertyKey], out int hashCreationIndex))
            {
                return;
            }

            var createHashNode = root.FindNode(diagnostic.AdditionalLocations[hashCreationIndex].SourceSpan);
            if (createHashNode is null)
            {
                return;
            }
            var disposeNodes = GetDisposeNodes(root, diagnostic.AdditionalLocations, hashCreationIndex);

            if (!TryGetCodeAction(context.Document,
                computeHashNode,
                hashDataNode,
                createHashNode,
                disposeNodes,
                out HashDataCodeAction? codeAction))
            {
                return;
            }

            context.RegisterCodeFix(codeAction, diagnostic);

        }

        private static SyntaxNode[] GetDisposeNodes(SyntaxNode root, IReadOnlyList<Location> additionalLocations, int hashCreationIndex)
        {
            var disposeCount = additionalLocations.Count - hashCreationIndex - 1;
            if (disposeCount == 0)
            {
                return Array.Empty<SyntaxNode>();
            }
            var disposeNodes = new SyntaxNode[disposeCount];

            for (int i = 0; i < disposeNodes.Length; i++)
            {
                var node = root.FindNode(additionalLocations[hashCreationIndex + i + 1].SourceSpan);
                if (node is null)
                {
                    return Array.Empty<SyntaxNode>();
                }
                disposeNodes[i] = node;
            }

            return disposeNodes;
        }

        protected abstract SyntaxNode? GetHashDataSyntaxNode(PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, string hashTypeName, SyntaxNode computeHashNode);

        protected abstract bool TryGetCodeAction(Document document,
            SyntaxNode computeHashNode,
            SyntaxNode hashDataNode,
            SyntaxNode createHashNode,
            SyntaxNode[] disposeNodes,
            [NotNullWhen(true)] out HashDataCodeAction? codeAction);

        protected abstract class HashDataCodeAction : CodeAction
        {
            protected HashDataCodeAction(Document document, SyntaxNode computeHashNode, SyntaxNode hashDateNode)
            {
                Document = document;
                HashDataNode = hashDateNode;
                ComputeHashNode = computeHashNode;
            }
            public override string Title => MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle;
            public override string EquivalenceKey => nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle);

            public Document Document { get; }
            public SyntaxNode ComputeHashNode { get; }
            public SyntaxNode HashDataNode { get; }

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                DocumentEditor editor = await DocumentEditor.CreateAsync(Document, cancellationToken).ConfigureAwait(false);
                EditNodes(editor, HashDataNode);

                return editor.GetChangedDocument();
            }

            protected abstract void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked);
        }

        private sealed class ReplaceNodeHashDataCodeAction : HashDataCodeAction
        {
            public ReplaceNodeHashDataCodeAction(Document document, SyntaxNode computeHashNode, SyntaxNode hashDataNode) : base(document, computeHashNode, hashDataNode)
            {
            }

            protected override void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked)
            {
                documentEditor.ReplaceNode(ComputeHashNode, hashDataInvoked);
            }
        }

        protected sealed class RemoveNodeHashDataCodeAction : HashDataCodeAction
        {
            private readonly SyntaxNode[] _disposeNodes;
            public RemoveNodeHashDataCodeAction(
                Document document,
                SyntaxNode computeHashNode,
                SyntaxNode hashDataNode,
                SyntaxNode hashCreationNode,
                SyntaxNode[] disposeNodes) : base(document, computeHashNode, hashDataNode)
            {
                HashCreationNode = hashCreationNode;
                _disposeNodes = disposeNodes;
            }

            public SyntaxNode HashCreationNode { get; }

            protected override void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked)
            {
                documentEditor.ReplaceNode(ComputeHashNode, hashDataInvoked);
                documentEditor.RemoveNode(HashCreationNode);

                foreach (var disposeNode in _disposeNodes)
                {
                    documentEditor.RemoveNode(disposeNode);
                }
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

                        root = TrackTargets(root, hashInstanceTargets);

                        root = FixDocumentRoot(root, hashInstanceTargets);
                        root = Formatter.Format(root, Formatter.Annotation, newSolution.Workspace, cancellationToken: cancellationToken);
                        newSolution = document.WithSyntaxRoot(root).Project.Solution;
                    }
                }

                return newSolution;
            }

            public override string Title { get; }
            private static SyntaxNode TrackTargets(SyntaxNode root, HashInstanceTarget[] targets)
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
            internal abstract SyntaxNode FixDocumentRoot(SyntaxNode root, HashInstanceTarget[] hashInstanceTargets);
        }

        protected sealed class HashInstanceTarget
        {
            public HashInstanceTarget(SyntaxNode? createNode, SyntaxNode[]? disposeNodes)
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

        private static HashInstanceTarget[]? CollectTargets(SyntaxNode root, IGrouping<SyntaxTree, Diagnostic> grouping, CancellationToken cancellationToken)
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
                    if (!Enum.TryParse<PreferHashDataOverComputeHashAnalyzer.ComputeType>(d.Properties[PreferHashDataOverComputeHashAnalyzer.ComputeTypePropertyKey],
                        out var computeType))
                    {
                        return false;
                    }

                    var computeHashNode = root.FindNode(d.Location.SourceSpan, getInnermostNodeForTie: true);
                    if (computeHashNode is null)
                    {
                        return false;
                    }
                    var hashTypeName = d.Properties[PreferHashDataOverComputeHashAnalyzer.TargetHashTypeDiagnosticPropertyKey];
                    if (!d.Properties.TryGetValue(PreferHashDataOverComputeHashAnalyzer.HashCreationIndexPropertyKey, out var hashCreationIndexPropertyKey) ||
                        !int.TryParse(hashCreationIndexPropertyKey, out int hashCreationIndex))
                    {
                        chainedComputeHashList.Add(new ComputeHashSyntaxHolder(computeHashNode, computeType, hashTypeName));
                        continue;
                    }
                    var createNode = root.FindNode(d.AdditionalLocations[hashCreationIndex].SourceSpan);
                    if (createNode is null)
                    {
                        return false;
                    }
                    if (!dictionary.TryGetValue(createNode, out HashInstanceTarget hashInstanceTarget))
                    {
                        var disposeNodes = GetDisposeNodes(root, d.AdditionalLocations, hashCreationIndex);
                        hashInstanceTarget = new HashInstanceTarget(createNode, disposeNodes);
                        dictionary.Add(createNode, hashInstanceTarget);
                    }
                    var computeHashSyntaxHolder = new ComputeHashSyntaxHolder(computeHashNode, computeType, hashTypeName);
                    hashInstanceTarget.ComputeHashNodes.Add(computeHashSyntaxHolder);
                }
                return true;
            }
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
    }
}
