// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AvoidConstArraysFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AvoidConstArraysAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            SyntaxTree tree = await document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);

            // Apply fix depending on how original constant was written
            // Name of static field can be [type]Array
            string title = MicrosoftNetCoreAnalyzersResources.AvoidConstArraysTitle;
            context.RegisterCodeFix(
                new MyCodeAction(
                    title,
                    async cancellationToken =>
                    {
                        DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
                        // Make changes here
                        return editor.GetChangedDocument();
                    },
                    equivalenceKey: title
                ),
                context.Diagnostics
            );
            await Task.Run(() => { }).ConfigureAwait(false);
        }

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