// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;


namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    public abstract class InstantiateArgumentExceptionsCorrectlyFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(InstantiateArgumentExceptionsCorrectlyAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            string paramPositionString = diagnostic.Properties.GetValueOrDefault(InstantiateArgumentExceptionsCorrectlyAnalyzer.MessagePosition);
            if (paramPositionString != null)
            {
                SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                SyntaxNode node = root.FindNode(context.Span, getInnermostNodeForTie: true);
                PopulateCodeFix(context, diagnostic, paramPositionString, node);
            }
        }

        protected abstract void PopulateCodeFix(CodeFixContext context, Diagnostic diagnostic, string paramPositionString, SyntaxNode node);
    }
}