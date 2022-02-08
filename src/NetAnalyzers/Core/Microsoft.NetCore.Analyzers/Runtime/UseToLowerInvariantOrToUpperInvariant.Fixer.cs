// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class UseToLowerInvariantOrToUpperInvariantFixerBase : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(UseToLowerInvariantOrToUpperInvariantAnalyzer.RuleId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var title = MicrosoftNetCoreAnalyzersResources.UseToLowerInvariantOrToUpperInvariantTitle;

            if (ShouldFix(node))
            {
                var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
                context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => await FixInvocationAsync(context.Document, syntaxGenerator, root, node).ConfigureAwait(false),
                                                         equivalenceKey: title),
                                        context.Diagnostics);
            }
        }

        protected abstract bool ShouldFix(SyntaxNode node);

        protected abstract Task<Document> FixInvocationAsync(Document document, SyntaxGenerator syntaxGenerator, SyntaxNode root, SyntaxNode node);

        protected static string GetReplacementMethodName(string currentMethodName) => currentMethodName switch
        {
            UseToLowerInvariantOrToUpperInvariantAnalyzer.ToLowerMethodName => "ToLowerInvariant",
            UseToLowerInvariantOrToUpperInvariantAnalyzer.ToUpperMethodName => "ToUpperInvariant",
            _ => currentMethodName,
        };

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
