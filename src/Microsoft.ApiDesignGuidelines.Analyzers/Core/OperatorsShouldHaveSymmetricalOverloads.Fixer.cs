// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using System.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2226: Operators should have symmetrical overloads
    /// </summary>
    public abstract class OperatorsShouldHaveSymmetricalOverloadsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OperatorsShouldHaveSymmetricalOverloadsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(CodeAction.Create(MicrosoftApiDesignGuidelinesAnalyzersResources.Generate_missing_operators,
                c => CreateChangedDocument(context.Document, c), nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.Generate_missing_operators)),
                context.Diagnostics.First());
            return Task.FromResult(true);
        }

        private async Task<Document> CreateChangedDocument(CodeFixContext context, CancellationToken cancellationToken)
        {
            var document = context.Document;
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var containingOperator = semanticModel.GetEnclosingSymbol(context.Diagnostics.First().Location.SourceSpan.Start, cancellationToken);

            Debug.Assert(containingOperator.IsUserDefinedOperator());

            var generator = SyntaxGenerator.GetGenerator(document);

        }
    }
}