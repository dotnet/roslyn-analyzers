// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferHashDataOverComputeHashAnalyzer.CA1848);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var computeHashNode = root.FindNode(context.Span, getInnermostNodeForTie: true);
            var diagnostic = context.Diagnostics[0];
            var bufferArgNode = root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan);

            if (computeHashNode is null || bufferArgNode is null)
            {
                return;
            }
            var hashTypeName = diagnostic.Properties[PreferHashDataOverComputeHashAnalyzer.TargetHashTypeDiagnosticPropertyKey];

            switch (diagnostic.AdditionalLocations.Count)
            {
                case 1:
                    //chained method SHA256.Create().ComputeHash(buffer)
                    var codeActionChain = new ReplaceNodeHashDataCodeAction(context.Document,
                        hashTypeName,
                        bufferArgNode,
                        computeHashNode);
                    context.RegisterCodeFix(codeActionChain, diagnostic);
                    return;
                case 2:
                    var nodeToRemove = root.FindNode(diagnostic.AdditionalLocations[1].SourceSpan);
                    if (nodeToRemove is null)
                    {
                        return;
                    }

                    if (!TryGetCodeAction(context.Document, hashTypeName, bufferArgNode, computeHashNode, nodeToRemove, out HashDataCodeAction? codeAction))
                    {
                        return;
                    }

                    context.RegisterCodeFix(codeAction, diagnostic);
                    return;
            }
        }

        protected abstract bool TryGetCodeAction(Document document, string hashTypeName, SyntaxNode bufferArgNode, SyntaxNode computeHashNode, SyntaxNode nodeToRemove,
            [NotNullWhen(true)] out HashDataCodeAction? codeAction);

        protected abstract class HashDataCodeAction : CodeAction
        {
            protected HashDataCodeAction(Document document, string hashTypeName, SyntaxNode bufferArgNode, SyntaxNode computeHashNode)
            {
                Document = document;
                HashTypeName = hashTypeName;
                BufferArgNode = bufferArgNode;
                ComputeHashNode = computeHashNode;
            }
            public override string Title => MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle;
            public override string EquivalenceKey => nameof(MicrosoftNetCoreAnalyzersResources.PreferHashDataCodefixTitle);

            public Document Document { get; }
            public string HashTypeName { get; }
            public SyntaxNode BufferArgNode { get; }
            public SyntaxNode ComputeHashNode { get; }

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                DocumentEditor editor = await DocumentEditor.CreateAsync(Document, cancellationToken).ConfigureAwait(false);
                SyntaxGenerator generator = editor.Generator;

                // hashTypeName.HashData
                var hashData = generator.MemberAccessExpression(generator.IdentifierName(HashTypeName), PreferHashDataOverComputeHashAnalyzer.HashDataMethodName);
                var hashDataInvoked = generator.InvocationExpression(hashData, BufferArgNode);
                EditNodes(editor, hashDataInvoked);

                return editor.GetChangedDocument();
            }

            protected abstract void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked);
        }

        private sealed class ReplaceNodeHashDataCodeAction : HashDataCodeAction
        {
            public ReplaceNodeHashDataCodeAction(Document document, string hashTypeName, SyntaxNode bufferArgNode, SyntaxNode computeHashNode) : base(document, hashTypeName, bufferArgNode, computeHashNode)
            {
            }

            protected override void EditNodes(DocumentEditor documentEditor, SyntaxNode hashDataInvoked)
            {
                documentEditor.ReplaceNode(ComputeHashNode, hashDataInvoked);
            }
        }

        protected sealed class RemoveNodeHashDataCodeAction : HashDataCodeAction
        {
            public RemoveNodeHashDataCodeAction(Document document, string hashTypeName, SyntaxNode bufferArgNode, SyntaxNode computeHashNode, SyntaxNode nodeToRemove) : base(document, hashTypeName, bufferArgNode, computeHashNode)
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
