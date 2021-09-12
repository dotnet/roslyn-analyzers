// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.CodeQuality.Analyzers.Usage
{
    using static MicrosoftCodeQualityAnalyzersResources;

    public abstract class DoNotCallOrderByMultipleTimesFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DoNotCallOrderByMultipleTimes.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            if (node is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.FirstOrDefault();

            context.RegisterCodeFix(new CodeAction(_ => Task.FromResult(ReplaceOrderByWithThenBy(context.Document, root, node))), diagnostic);
        }

        protected abstract Document ReplaceOrderByWithThenBy(Document document,
                                                             SyntaxNode root,
                                                             SyntaxNode node);

        private class CodeAction : DocumentChangeAction
        {
            public CodeAction(Func<CancellationToken, Task<Document>> action)
            : base(DoNotCallOrderByMultipleTimesTitle, action, DoNotCallOrderByMultipleTimes.RuleId)
            { }
        }
    }
}
