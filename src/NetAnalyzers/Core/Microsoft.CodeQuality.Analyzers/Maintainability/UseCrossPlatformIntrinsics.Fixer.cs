// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;
    using RuleKind = UseCrossPlatformIntrinsicsAnalyzer.RuleKind;

    public abstract class UseCrossPlatformIntrinsicsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            UseCrossPlatformIntrinsicsAnalyzer.RuleId
        );

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers'
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            if (node is null)
            {
                return;
            }

            SemanticModel model = await context.Document.GetRequiredSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            if (model.GetOperation(node, context.CancellationToken) is not IInvocationOperation invocation)
            {
                return;
            }

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Properties.TryGetValue(nameof(RuleKind), out string? ruleKindName))
                {
                    continue;
                }

                if (!Enum.TryParse(ruleKindName, out RuleKind ruleKind))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        UseCrossPlatformIntrinsicsTitle,
                        c => ReplaceWithCrossPlatformIntrinsicAsync(context.Document, ruleKind, invocation, c),
                        equivalenceKey: nameof(UseCrossPlatformIntrinsicsFixer)
                    ),
                    diagnostic
                );
            }
        }

        private async Task<Document> ReplaceWithCrossPlatformIntrinsicAsync(Document document, RuleKind ruleKind, IInvocationOperation invocation, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            SyntaxNode? replacementNode = ruleKind switch
            {
                RuleKind.opAddition => ReplaceBinaryOp(invocation, isCommutative: true, generator.AddExpression),
                RuleKind.opBitwiseAnd => ReplaceBinaryOp(invocation, isCommutative: true, generator.BitwiseAndExpression),
                RuleKind.opBitwiseOr => ReplaceBinaryOp(invocation, isCommutative: true, generator.BitwiseOrExpression),
                RuleKind.opDivision => ReplaceBinaryOp(invocation, isCommutative: false, generator.DivideExpression),
                RuleKind.opExclusiveOr => ReplaceBinaryOp(invocation, isCommutative: true, CreateExclusiveOrExpression),
                RuleKind.opLeftShift => ReplaceBinaryOp(invocation, isCommutative: false, CreateLeftShiftExpression),
                RuleKind.opMultiply => ReplaceBinaryOp(invocation, isCommutative: true, generator.MultiplyExpression),
                RuleKind.opOnesComplement => ReplaceUnaryOp(invocation, generator.BitwiseNotExpression),
                RuleKind.opRightShift => ReplaceBinaryOp(invocation, isCommutative: false, CreateRightShiftExpression),
                RuleKind.opSubtraction => ReplaceBinaryOp(invocation, isCommutative: false, generator.SubtractExpression),
                RuleKind.opUnaryNegation => ReplaceUnaryOp(invocation, generator.NegateExpression),
                RuleKind.opUnsignedRightShift => ReplaceBinaryOp(invocation, isCommutative: false, CreateUnsignedRightShiftExpression),
                _ => null,
            };

            if (replacementNode is not null)
            {
                editor.ReplaceNode(invocation.Syntax, replacementNode.WithTriviaFrom(invocation.Syntax));
                document = document.WithSyntaxRoot(editor.GetChangedRoot());
            }

            return document;

            static SyntaxNode? ReplaceUnaryOp(IInvocationOperation invocation, Func<SyntaxNode, SyntaxNode?> unaryOpFunc)
            {
                if (invocation.Arguments.Length != 1)
                {
                    return null;
                }

                return unaryOpFunc(
                    invocation.Arguments[0].Value.Syntax
                );
            }

            static SyntaxNode? ReplaceBinaryOp(IInvocationOperation invocation, bool isCommutative, Func<SyntaxNode, SyntaxNode, SyntaxNode?> binaryOpFunc)
            {
                if (invocation.Arguments.Length != 2)
                {
                    return null;
                }

                IArgumentOperation arg0 = invocation.Arguments[0];
                IArgumentOperation arg1 = invocation.Arguments[1];

                if (!isCommutative && (arg0.Parameter?.Ordinal != 0))
                {
                    (arg0, arg1) = (arg1, arg0);
                }

                return binaryOpFunc(
                    arg0.Value.Syntax,
                    arg1.Value.Syntax
                );
            }
        }

        protected abstract SyntaxNode CreateExclusiveOrExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode CreateLeftShiftExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode CreateRightShiftExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode? CreateUnsignedRightShiftExpression(SyntaxNode left, SyntaxNode right);
    }
}