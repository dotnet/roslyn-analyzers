// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferHashDataOverComputeHashFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferHashDataOverComputeHashAnalyzer.CA1849);

        public sealed override FixAllProvider? GetFixAllProvider() => null;

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

            var args = GetArgNodes(root, diagnostic, computeType);
            if (args is null)
            {
                return;
            }
            var hashTypeName = diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.TargetHashTypeDiagnosticPropertyKey];

            if (!diagnostic.Properties.ContainsKey(PreferHashDataOverComputeHashAnalyzer.DeleteHashCreationPropertyKey))
            {
                // chained method SHA256.Create().ComputeHash(arg)
                // instance.ComputeHash(arg) xN where N > 1
                var codeActionChain = new ReplaceNodeHashDataCodeAction(context.Document,
                    hashTypeName,
                    computeHashNode,
                    computeType,
                    args);
                context.RegisterCodeFix(codeActionChain, diagnostic);
                return;
            }

            var nodeToRemove = root.FindNode(diagnostic.AdditionalLocations[args.Length].SourceSpan);
            if (nodeToRemove is null)
            {
                return;
            }

            if (!TryGetCodeAction(context.Document,
                hashTypeName,
                computeHashNode,
                computeType,
                args,
                nodeToRemove,
                out HashDataCodeAction? codeAction))
            {
                return;
            }

            context.RegisterCodeFix(codeAction, diagnostic);

        }

        private static SyntaxNode[]? GetArgNodes(SyntaxNode root, Diagnostic diagnostic, PreferHashDataOverComputeHashAnalyzer.ComputeType computeType)
        {
            switch (computeType)
            {
                case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHash:
                    {
                        var bufferArgNode = root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan);
                        if (bufferArgNode is null)
                        {
                            return null;
                        }
                        return new[] { bufferArgNode };
                    }
                case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHashSection:
                    {
                        var bufferArgNode = root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan, getInnermostNodeForTie: true);
                        var startIndexArgNode = root.FindNode(diagnostic.AdditionalLocations[1].SourceSpan);
                        var endIndexArgNode = root.FindNode(diagnostic.AdditionalLocations[2].SourceSpan);
                        if (bufferArgNode is null || startIndexArgNode is null || endIndexArgNode is null)
                        {
                            return null;
                        }
                        return new[] { bufferArgNode, startIndexArgNode, endIndexArgNode };
                    }
                case PreferHashDataOverComputeHashAnalyzer.ComputeType.TryComputeHash:
                    {
                        var rosByte = root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan);
                        var spanDest = root.FindNode(diagnostic.AdditionalLocations[1].SourceSpan);
                        var intWrite = root.FindNode(diagnostic.AdditionalLocations[2].SourceSpan);
                        if (rosByte is null || spanDest is null || intWrite is null)
                        {
                            return null;
                        }
                        return new[] { rosByte, spanDest, intWrite };
                    }
                default:
                    return null;
            }
        }

        protected abstract bool TryGetCodeAction(Document document,
            string hashTypeName,
            SyntaxNode computeHashNode,
            PreferHashDataOverComputeHashAnalyzer.ComputeType computeType,
            SyntaxNode[] argNodes,
            SyntaxNode nodeToRemove,
            [NotNullWhen(true)] out HashDataCodeAction? codeAction);

        protected abstract class HashDataCodeAction : CodeAction
        {
            private readonly SyntaxNode[] _argNode;
            private readonly PreferHashDataOverComputeHashAnalyzer.ComputeType _computeType;
            protected HashDataCodeAction(Document document, string hashTypeName, SyntaxNode computeHashNode, PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, SyntaxNode[] argNode)
            {
                Document = document;
                HashTypeName = hashTypeName;
                _argNode = argNode;
                ComputeHashNode = computeHashNode;
                _computeType = computeType;
            }
            public override string Title => MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle;
            public override string EquivalenceKey => nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle);

            public Document Document { get; }
            public string HashTypeName { get; }
            public SyntaxNode ComputeHashNode { get; }

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                DocumentEditor editor = await DocumentEditor.CreateAsync(Document, cancellationToken).ConfigureAwait(false);
                SyntaxGenerator generator = editor.Generator;

                switch (_computeType)
                {
                    // hashTypeName.HashData(buffer)
                    case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHash:
                        {
                            var hashData = generator.MemberAccessExpression(generator.IdentifierName(HashTypeName), PreferHashDataOverComputeHashAnalyzer.HashDataMethodName);
                            var hashDataInvoked = generator.InvocationExpression(hashData, _argNode);
                            EditNodes(editor, hashDataInvoked);
                            break;
                        }
                    // hashTypeName.HashData(buffer.AsSpan(start, end))
                    case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHashSection:
                        {
                            var asSpan = generator.MemberAccessExpression(_argNode[0], "AsSpan");
                            var asSpanInvoked = generator.InvocationExpression(asSpan, _argNode[1], _argNode[2]);
                            var hashData = generator.MemberAccessExpression(generator.IdentifierName(HashTypeName), PreferHashDataOverComputeHashAnalyzer.HashDataMethodName);
                            var hashDataInvoked = generator.InvocationExpression(hashData, asSpanInvoked);
                            EditNodes(editor, hashDataInvoked);
                            break;
                        }
                    // hashTypeName.TryHashData(rosSpan, span, write)
                    case PreferHashDataOverComputeHashAnalyzer.ComputeType.TryComputeHash:
                        {
                            var hashData = generator.MemberAccessExpression(generator.IdentifierName(HashTypeName), PreferHashDataOverComputeHashAnalyzer.TryHashDataMethodName);
                            var hashDataInvoked = generator.InvocationExpression(hashData, _argNode);
                            EditNodes(editor, hashDataInvoked);
                            break;
                        }
                }

                return editor.GetChangedDocument();
            }

            protected abstract void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked);
        }

        private sealed class ReplaceNodeHashDataCodeAction : HashDataCodeAction
        {
            public ReplaceNodeHashDataCodeAction(Document document, string hashTypeName, SyntaxNode computeHashNode, PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, params SyntaxNode[] argNode) : base(document, hashTypeName, computeHashNode, computeType, argNode)
            {
            }

            protected override void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked)
            {
                documentEditor.ReplaceNode(ComputeHashNode, hashDataInvoked);
            }
        }

        protected sealed class RemoveNodeHashDataCodeAction : HashDataCodeAction
        {
            public RemoveNodeHashDataCodeAction(Document document, string hashTypeName, SyntaxNode computeHashNode, PreferHashDataOverComputeHashAnalyzer.ComputeType computeType, SyntaxNode[] argNode, SyntaxNode nodeToRemove) : base(document, hashTypeName, computeHashNode, computeType, argNode)
            {
                NodeToRemove = nodeToRemove;
            }

            public SyntaxNode NodeToRemove { get; }

            protected override void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked)
            {
                documentEditor.ReplaceNode(ComputeHashNode, hashDataInvoked);
                documentEditor.RemoveNode(NodeToRemove);
            }
        }
    }
}
