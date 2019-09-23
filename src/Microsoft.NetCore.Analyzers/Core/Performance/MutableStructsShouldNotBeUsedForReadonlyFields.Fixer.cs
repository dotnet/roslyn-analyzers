// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.NetCore.Analyzers.Performance
{
    public abstract class MutableStructsShouldNotBeUsedForReadonlyFieldsFixer : CodeFixProvider
    {
        protected const string EquivalencyKey = "MutableStructsShouldNotBeUserForReadonlyFields";

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic == null)
            {
                return;
            }

            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var node = root.FindNode(context.Span);

            AnalyzeCodeFix(context, node);
        }

        protected abstract void AnalyzeCodeFix(CodeFixContext context, SyntaxNode targetNode);

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MutableStructsShouldNotBeUsedForReadonlyFieldsAnalyzer.RuleId);

        public override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}