// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class PreferHashDataOverComputeHashFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferHashDataOverComputeHashAnalyzer.CA1847);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var computeHashNode = root.FindNode(context.Span, getInnermostNodeForTie: true);
            var diagnostics = context.Diagnostics[0];
            var bufferArgNode = root.FindNode(diagnostics.AdditionalLocations[0].SourceSpan);
            if (computeHashNode is null || bufferArgNode is null)
            {
                return;
            }
            var hashTypeName = diagnostics.Properties[PreferHashDataOverComputeHashAnalyzer.TargetHashTypeName];

            switch (diagnostics.AdditionalLocations.Count)
            {
                case 1:
                    {
                        //chained method SHA256.Create().ComputeHash(buffer)
                        var codeActionChain = GetCodeAction(context, hashTypeName, bufferArgNode, (editor, hashDataInvoked) =>
                        {
                            editor.ReplaceNode(computeHashNode, hashDataInvoked);
                        });
                        context.RegisterCodeFix(codeActionChain, diagnostics);
                        return;
                    }
                case 2:
                    {
                        var nodeToRemove = root.FindNode(diagnostics.AdditionalLocations[1].SourceSpan);
                        if (nodeToRemove is null || !TryGetCodeFixer(computeHashNode, bufferArgNode, nodeToRemove, out ApplyCodeFixAction? codeFixer))
                        {
                            return;
                        }

                        context.RegisterCodeFix(GetCodeAction(context, hashTypeName, bufferArgNode, codeFixer), diagnostics);
                        return;
                    }
            }
        }

        private static CodeAction GetCodeAction(CodeFixContext context, string hashTypeName, SyntaxNode bufferArgNode, ApplyCodeFixAction codeFixer)
        {
            return CodeAction.Create(
                   title: MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerTitle,
                   createChangedDocument: async cancellationToken =>
                   {
                       DocumentEditor editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                       SyntaxGenerator generator = editor.Generator;

                       // hashTypeName.HashData
                       var hashData = generator.MemberAccessExpression(generator.IdentifierName(hashTypeName), PreferHashDataOverComputeHashAnalyzer.HashDataMethodName);
                       var hashDataInvoked = generator.InvocationExpression(hashData, bufferArgNode);
                       codeFixer(editor, hashDataInvoked);

                       return editor.GetChangedDocument();
                   },
                   equivalenceKey: MicrosoftNetCoreAnalyzersResources.PreferHashDataOverComputeHashAnalyzerTitle);
        }

        protected abstract bool TryGetCodeFixer(SyntaxNode computeHashNode, SyntaxNode bufferArgNode, SyntaxNode nodeToRemove,
            [NotNullWhen(true)] out ApplyCodeFixAction? codeFixer);

        protected delegate void ApplyCodeFixAction(DocumentEditor editor, SyntaxNode hashDataInvoked);
    }
}
