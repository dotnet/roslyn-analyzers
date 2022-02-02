// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class DoNotGuardDictionaryRemoveByContainsKeyFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DoNotGuardDictionaryRemoveByContainsKey.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            if (node is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.FirstOrDefault();

            if (!TryParseLocationInfo(diagnostic, DoNotGuardDictionaryRemoveByContainsKey.ConditionalOperation, out var conditionalOperationSpan) ||
                !TryParseLocationInfo(diagnostic, DoNotGuardDictionaryRemoveByContainsKey.ChildStatementOperation, out var childStatementOperationSpan) ||
                root.FindNode(conditionalOperationSpan) is not SyntaxNode conditionalSyntax ||
                root.FindNode(childStatementOperationSpan) is not SyntaxNode childStatementSyntax)
            {
                return;
            }

            // we only offer a fixer if 'Remove' is the _only_ statement
            if (!SyntaxSupportedByFixer(conditionalSyntax))
                return;

            context.RegisterCodeFix(new DoNotGuardDictionaryRemoveByContainsKeyCodeAction(_ =>
                Task.FromResult(ReplaceConditionWithChild(context.Document, root, conditionalSyntax, childStatementSyntax))),
                diagnostic);
        }

        protected abstract bool SyntaxSupportedByFixer(SyntaxNode conditionalSyntax);

        protected abstract Document ReplaceConditionWithChild(Document document, SyntaxNode root,
                                                              SyntaxNode conditionalOperationNode,
                                                              SyntaxNode childOperationNode);

        private static bool TryParseLocationInfo(Diagnostic diagnostic, string propertyKey, out TextSpan span)
        {
            span = default;

            if (!diagnostic.Properties.TryGetValue(propertyKey, out var locationInfo))
                return false;

            var parts = locationInfo.Split(DoNotGuardDictionaryRemoveByContainsKey.AdditionalDocumentLocationInfoSeparatorArray, StringSplitOptions.None);
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out var spanStart) ||
                !int.TryParse(parts[1], out var spanLength))
            {
                return false;
            }

            span = new TextSpan(spanStart, spanLength);
            return true;
        }

        private class DoNotGuardDictionaryRemoveByContainsKeyCodeAction : DocumentChangeAction
        {
            public DoNotGuardDictionaryRemoveByContainsKeyCodeAction(Func<CancellationToken, Task<Document>> action)
            : base(MicrosoftNetCoreAnalyzersResources.RemoveRedundantGuardCallCodeFixTitle, action,
                   DoNotGuardDictionaryRemoveByContainsKey.RuleId)
            { }
        }
    }
}
