// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class PreferConvertToHexStringOverBitConverterFixer : CodeFixProvider
    {
        private static readonly SyntaxAnnotation s_asSpanSymbolAnnotation = new("SymbolId", WellKnownTypeNames.SystemMemoryExtensions);

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(PreferConvertToHexStringOverBitConverterAnalyzer.RuleId);

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

            var semanticModel = await context.Document.GetRequiredSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var operation = semanticModel.GetOperation(node, context.CancellationToken);

            if (operation is not IInvocationOperation invocation ||
                !PreferConvertToHexStringOverBitConverterAnalyzer.RequiredSymbols.TryGetSymbols(semanticModel.Compilation, out var symbols) ||
                !symbols.TryGetBitConverterToStringInvocationAndReplacement(invocation, out var bitConverterInvocation, out var convertToHexStringMethod, out var toLowerInvocation))
            {
                return;
            }

            var codeAction = CodeAction.Create(
                string.Format(CultureInfo.CurrentCulture, MicrosoftNetCoreAnalyzersResources.PreferConvertToHexStringOverBitConverterCodeFixTitle, convertToHexStringMethod.Name),
                ReplaceWithConvertToHexStringCall,
                nameof(MicrosoftNetCoreAnalyzersResources.PreferConvertToHexStringOverBitConverterCodeFixTitle));

            context.RegisterCodeFix(codeAction, context.Diagnostics);

            async Task<Document> ReplaceWithConvertToHexStringCall(CancellationToken cancellationToken)
            {
                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                var generator = editor.Generator;
                var bitConverterArgumentsInParameterOrder = bitConverterInvocation.Arguments.GetArgumentsInParameterOrder();

                var typeExpression = generator.TypeExpressionForStaticMemberAccess(convertToHexStringMethod.ContainingType);
                var methodExpression = generator.MemberAccessExpression(typeExpression, convertToHexStringMethod.Name);
                var methodInvocation = bitConverterArgumentsInParameterOrder.Length switch
                {
                    // BitConverter.ToString(data).Replace("-", "") => Convert.ToHexString(data)
                    1 => generator.InvocationExpression(methodExpression, bitConverterArgumentsInParameterOrder[0].Value.Syntax),
                    // BitConverter.ToString(data, start).Replace("-", "") => Convert.ToHexString(data.AsSpan().Slice(start))
                    2 => generator.InvocationExpression(
                        methodExpression,
                        generator.InvocationExpression(generator.MemberAccessExpression(
                            generator.InvocationExpression(generator.MemberAccessExpression(
                                bitConverterArgumentsInParameterOrder[0].Value.Syntax,
                                nameof(MemoryExtensions.AsSpan))),
                            WellKnownMemberNames.SliceMethodName),
                        bitConverterArgumentsInParameterOrder[1].Value.Syntax))
                            .WithAddImportsAnnotation()
                            .WithAdditionalAnnotations(s_asSpanSymbolAnnotation),
                    // BitConverter.ToString(data, start, length).Replace("-", "") => Convert.ToHexString(data, start, length)
                    3 => generator.InvocationExpression(methodExpression, bitConverterArgumentsInParameterOrder.Select(a => a.Value.Syntax).ToArray()),
                    _ => throw new NotImplementedException()
                };

                // This branch is hit when string.ToLower* is used and Convert.ToHexStringLower is not available.
                if (toLowerInvocation is not null)
                {
                    methodInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(methodInvocation, toLowerInvocation.TargetMethod.Name),
                        toLowerInvocation.Arguments.Select(a => a.Value.Syntax).ToArray());
                }

                editor.ReplaceNode(invocation.Syntax, methodInvocation.WithTriviaFrom(invocation.Syntax));

                return context.Document.WithSyntaxRoot(editor.GetChangedRoot());
            }
        }
    }
}
