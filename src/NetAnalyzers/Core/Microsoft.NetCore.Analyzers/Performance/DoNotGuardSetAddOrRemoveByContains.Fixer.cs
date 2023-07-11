﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class DoNotGuardSetAddOrRemoveByContainsFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DoNotGuardSetAddOrRemoveByContains.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            if (node is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var conditionalLocation = diagnostic.AdditionalLocations[0];
            var childLocation = diagnostic.AdditionalLocations[1];

            if (root.FindNode(conditionalLocation.SourceSpan) is not SyntaxNode conditionalSyntax ||
                root.FindNode(childLocation.SourceSpan) is not SyntaxNode childStatementSyntax)
            {
                return;
            }

            // We only offer a fixer if the conditonal true branch has a single statement, either 'Add' or 'Delete'
            if (!SyntaxSupportedByFixer(conditionalSyntax))
            {
                return;
            }

            var title = MicrosoftNetCoreAnalyzersResources.DoNotGuardSetAddOrRemoveByContainsTitle;
            var codeAction = CodeAction.Create(title,
                ct => Task.FromResult(ReplaceConditionWithChild(context.Document, root, conditionalSyntax, childStatementSyntax)),
                title);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        protected abstract bool SyntaxSupportedByFixer(SyntaxNode conditionalSyntax);

        protected abstract Document ReplaceConditionWithChild(Document document, SyntaxNode root,
                                                              SyntaxNode conditionalOperationNode,
                                                              SyntaxNode childOperationNode);
    }
}
