// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
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

            if (operation is not IInvocationOperation replaceInvocation ||
                replaceInvocation.Instance is not IInvocationOperation instanceInvocation ||
                !PreferConvertToHexStringOverBitConverterAnalyzer.RequiredSymbols.TryGetSymbols(semanticModel.Compilation, out var symbols) ||
                !symbols.TryGetBitConverterToStringInvocation(instanceInvocation, out var bitConverterInvocation, out var toLowerInvocation))
            {
                return;
            }

            var codeAction = CodeAction.Create(
                MicrosoftNetCoreAnalyzersResources.PreferConvertToHexStringOverBitConverterCodeFixTitle,
                ReplaceWithConvertToHexStringCall,
                nameof(MicrosoftNetCoreAnalyzersResources.PreferConvertToHexStringOverBitConverterCodeFixTitle));

            context.RegisterCodeFix(codeAction, context.Diagnostics);

            async Task<Document> ReplaceWithConvertToHexStringCall(CancellationToken cancellationToken)
            {
                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                var generator = editor.Generator;
                var bitConverterArgumentsInParameterOrder = bitConverterInvocation.Arguments.GetArgumentsInParameterOrder();

                var convertToHexStringSymbol = bitConverterArgumentsInParameterOrder.Length == 1
                    ? symbols.ConvertToHexString!
                    : symbols.ConvertToHexStringStartLength!;
                var typeExpression = generator.TypeExpressionForStaticMemberAccess(convertToHexStringSymbol.ContainingType);
                var methodExpression = generator.MemberAccessExpression(typeExpression, convertToHexStringSymbol.Name);
                var methodInvocation = bitConverterArgumentsInParameterOrder.Length switch
                {
                    // BitConverter.ToString(data).Replace("-", "") => Convert.ToHexString(data)
                    1 => generator.InvocationExpression(methodExpression, bitConverterArgumentsInParameterOrder[0].Value.Syntax),
                    // BitConverter.ToString(data, start).Replace("-", "") => Convert.ToHexString(data, start, data.Length - start)
                    2 => generator.InvocationExpression(
                        methodExpression,
                        bitConverterArgumentsInParameterOrder[0].Value.Syntax,
                        bitConverterArgumentsInParameterOrder[1].Value.Syntax,
                        generator.SubtractExpression(
                            generator.MemberAccessExpression(
                                bitConverterArgumentsInParameterOrder[0].Value.Syntax,
                                WellKnownMemberNames.LengthPropertyName),
                            bitConverterArgumentsInParameterOrder[1].Value.Syntax)),
                    // BitConverter.ToString(data, start, length).Replace("-", "") => Convert.ToHexString(data, start, length)
                    3 => generator.InvocationExpression(methodExpression, bitConverterArgumentsInParameterOrder.Select(a => a.Value.Syntax).ToArray()),
                    _ => throw new NotImplementedException()
                };

                // BitConverter.ToString(data).ToLower().Replace("-", "") => Convert.ToHexString(data).ToLower()
                if (toLowerInvocation is not null)
                {
                    methodInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(methodInvocation, toLowerInvocation.TargetMethod.Name),
                        toLowerInvocation.Arguments.Select(a => a.Value.Syntax).ToArray());
                }

                editor.ReplaceNode(replaceInvocation.Syntax, methodInvocation.WithTriviaFrom(replaceInvocation.Syntax));

                return context.Document.WithSyntaxRoot(editor.GetChangedRoot());
            }
        }
    }
}
