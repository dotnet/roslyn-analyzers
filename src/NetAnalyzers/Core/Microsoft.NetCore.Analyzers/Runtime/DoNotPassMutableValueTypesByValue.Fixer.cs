// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CodeActions;
using System.Linq;
using Resx = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class DoNotPassMutableValueTypesByValueFixer : CodeFixProvider
    {
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var codeAction = CodeAction.Create(
                Resx.DoNotPassMutableValueTypesByValueCodeFixTitle,
                ChangeParameterToByReference,
                Resx.DoNotPassMutableValueTypesByValueCodeFixTitle);
            context.RegisterCodeFix(codeAction, diagnostic);

            return Task.CompletedTask;

            //  Local functions

            async Task<Document> ChangeParameterToByReference(CancellationToken token)
            {
                var editor = await DocumentEditor.CreateAsync(context.Document, token).ConfigureAwait(false);
                var root = await context.Document.GetSyntaxRootAsync(token).ConfigureAwait(false);
                var parameterNode = root.FindNode(diagnostic.Location.SourceSpan);
                editor.ReplaceNode(parameterNode, ConvertToByRefParameter(parameterNode));

                return editor.GetChangedDocument();
            }
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotPassMutableValueTypesByValueAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private protected abstract SyntaxNode ConvertToByRefParameter(SyntaxNode parameterNode);
    }
}
