// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2259: Use null-suppression properly.
    /// </summary>
    public abstract class UseNullSuppressionCorrectlyFixer : CodeFixProvider
    {
        public abstract override ImmutableArray<string> FixableDiagnosticIds { get; }

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            SyntaxNode root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);

            context.RegisterCodeFix(
                new MyCodeAction(
                    MicrosoftNetCoreAnalyzersResources.UseNullSuppressionCorrectlyTitle,
                    c => RemoveNullSuppression(document, root, node, c),
                    equivalenceKey: nameof(MicrosoftNetCoreAnalyzersResources.UseNullSuppressionCorrectlyTitle)),
                context.Diagnostics);
        }

        public abstract Task<Document> RemoveNullSuppression(Document document, SyntaxNode root, SyntaxNode node, CancellationToken cancellationToken);

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private sealed class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey) :
                base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}