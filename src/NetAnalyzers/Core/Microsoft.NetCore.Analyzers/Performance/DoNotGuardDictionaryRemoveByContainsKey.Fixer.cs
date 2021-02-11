// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class DoNotGuardDictionaryRemoveByContainsKeyFixer : CodeFixProvider
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
            var node = root.FindNode(context.Span);

            if (node is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.FirstOrDefault();

            if (TryParseLocationInfo(diagnostic, DoNotGuardDictionaryRemoveByContainsKey.ConditionalOperation, out var conditionalOperationSpan) &&
                TryParseLocationInfo(diagnostic, DoNotGuardDictionaryRemoveByContainsKey.ChildStatementOperation, out var childStatementOperationSpan))
            {
                context.RegisterCodeFix(
                    new DoNotGuardDictionaryRemoveByContainsKeyCodeAction(
                        context.Document,
                        conditionalOperationSpan,
                        childStatementOperationSpan),
                    diagnostic);
            }
        }

        private static bool TryParseLocationInfo(Diagnostic diagnostic, string propertyKey, out TextSpan span)
        {
            span = default;

            if (!diagnostic.Properties.TryGetValue(propertyKey, out var locationInfo))
                return false;

            var parts = locationInfo.Split(new[] { DoNotGuardDictionaryRemoveByContainsKey.AdditionalDocumentLocationInfoSeparator }, StringSplitOptions.None);
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out var spanStart) ||
                !int.TryParse(parts[1], out var spanLength))
            {
                return false;
            }

            span = new TextSpan(spanStart, spanLength);
            return true;
        }

        private class DoNotGuardDictionaryRemoveByContainsKeyCodeAction : CodeAction
        {
            private readonly Document _document;
            private readonly TextSpan _conditionalOperationSpan;
            private readonly TextSpan _childStatementOperationSpan;

            public override string Title { get; }

            public override string EquivalenceKey { get; }

            public DoNotGuardDictionaryRemoveByContainsKeyCodeAction(
                Document document,
                TextSpan conditionalOperationSpan,
                TextSpan childStatementOperationSpan)
            {
                _document = document;
                _conditionalOperationSpan = conditionalOperationSpan;
                _childStatementOperationSpan = childStatementOperationSpan;
                EquivalenceKey = DoNotGuardDictionaryRemoveByContainsKey.RuleId;
                Title = MicrosoftNetCoreAnalyzersResources.DoNotGuardDictionaryRemoveByContainsKeyTitle;
            }

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var documentText = await _document.GetTextAsync(cancellationToken).ConfigureAwait(false);

                var childStatementText = documentText.GetSubText(_childStatementOperationSpan);
                documentText = documentText.Replace(_conditionalOperationSpan, childStatementText.ToString());

                return _document.WithText(documentText);
            }
        }
    }
}
