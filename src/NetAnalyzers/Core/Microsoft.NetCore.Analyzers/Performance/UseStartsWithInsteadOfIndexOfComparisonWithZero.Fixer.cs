﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class UseStartsWithInsteadOfIndexOfComparisonWithZeroCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(UseStartsWithInsteadOfIndexOfComparisonWithZero.RuleId);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);
            var instance = root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan);
            var argument = root.FindNode(diagnostic.AdditionalLocations[1].SourceSpan);

            context.RegisterCodeFix(
                CodeAction.Create(MicrosoftNetCoreAnalyzersResources.UseStartsWithInsteadOfIndexOfComparisonWithZeroTitle,
                createChangedDocument: cancellationToken =>
                {
                    var generator = SyntaxGenerator.GetGenerator(document);
                    var expression = generator.InvocationExpression(generator.MemberAccessExpression(instance, "StartsWith"), argument);

                    var shouldNegate = diagnostic.Properties.TryGetValue(UseStartsWithInsteadOfIndexOfComparisonWithZero.ShouldNegateKey, out _);
                    if (shouldNegate)
                    {
                        expression = generator.LogicalNotExpression(expression);
                    }

                    return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(node, expression)));
                },
                equivalenceKey: nameof(MicrosoftNetCoreAnalyzersResources.UseStartsWithInsteadOfIndexOfComparisonWithZeroTitle)),
                context.Diagnostics);
        }
    }
}
