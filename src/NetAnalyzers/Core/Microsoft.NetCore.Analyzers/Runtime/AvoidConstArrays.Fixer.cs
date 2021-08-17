// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// using System;
using System.Collections.Immutable;
using System.Composition;
// using System.Threading;
using System.Threading.Tasks;
// using Analyzer.Utilities;
// using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
// using Microsoft.CodeAnalysis.Editing;
// using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AvoidConstArraysFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AvoidConstArraysAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // Document document = context.Document;
            // SemanticModel model = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            // SyntaxNode root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            // SyntaxNode node = root.FindNode(context.Span);
            await Task.Run(() => { }).ConfigureAwait(false);
        }
    }
}