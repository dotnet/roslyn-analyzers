// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.NetAnalyzers;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;
    using RuleKind = UseCrossPlatformIntrinsicsAnalyzer.RuleKind;

    public abstract class UseCrossPlatformIntrinsicsFixer : OrderedCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(UseCrossPlatformIntrinsicsAnalyzer.RuleId);

        protected sealed override string CodeActionTitle => UseCrossPlatformIntrinsicsTitle;

        protected sealed override string CodeActionEquivalenceKey => nameof(UseCrossPlatformIntrinsicsFixer);

        protected abstract SyntaxNode CreateExclusiveOrExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode CreateLeftShiftExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode CreateRightShiftExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode? CreateUnsignedRightShiftExpression(SyntaxNode left, SyntaxNode right);

        protected sealed override Task FixAllCoreAsync(SyntaxEditor editor, SyntaxGenerator generator, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode node = editor.OriginalRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

            // We shouldn't get here for a diagnostic that doesn't have the expected properties.

            if (!diagnostic.Properties.TryGetValue(nameof(RuleKind), out string? ruleKindName))
            {
                Debug.Fail($"Found diagnostic without an associated {nameof(RuleKind)} property.");
                return Task.CompletedTask;
            }

            if (!Enum.TryParse(ruleKindName, out RuleKind ruleKind))
            {
                Debug.Fail($"Found diagnostic with an unrecognized {nameof(RuleKind)} property: {ruleKindName}.");
                return Task.CompletedTask;
            }

            editor.ReplaceNode(node, (currentNode, generator) => ReplaceNode(currentNode, generator, ruleKind));
            return Task.CompletedTask;
        }

        protected abstract SyntaxNode ReplaceWithUnaryOperator(SyntaxNode currentNode, Func<SyntaxNode, SyntaxNode?> unaryOpFunc);

        protected abstract SyntaxNode ReplaceWithBinaryOperator(SyntaxNode currentNode, bool isCommutative, Func<SyntaxNode, SyntaxNode, SyntaxNode?> binaryOpFunc);

        private SyntaxNode ReplaceNode(SyntaxNode currentNode, SyntaxGenerator generator, RuleKind ruleKind)
        {
            return ruleKind switch
            {
                RuleKind.op_Addition => ReplaceWithBinaryOperator(currentNode, isCommutative: true, generator.AddExpression),
                RuleKind.op_BitwiseAnd => ReplaceWithBinaryOperator(currentNode, isCommutative: true, generator.BitwiseAndExpression),
                RuleKind.op_BitwiseOr => ReplaceWithBinaryOperator(currentNode, isCommutative: true, generator.BitwiseOrExpression),
                RuleKind.op_Division => ReplaceWithBinaryOperator(currentNode, isCommutative: false, generator.DivideExpression),
                RuleKind.op_ExclusiveOr => ReplaceWithBinaryOperator(currentNode, isCommutative: true, CreateExclusiveOrExpression),
                RuleKind.op_LeftShift => ReplaceWithBinaryOperator(currentNode, isCommutative: false, CreateLeftShiftExpression),
                RuleKind.op_Multiply => ReplaceWithBinaryOperator(currentNode, isCommutative: true, generator.MultiplyExpression),
                RuleKind.op_OnesComplement => ReplaceWithUnaryOperator(currentNode, generator.BitwiseNotExpression),
                RuleKind.op_RightShift => ReplaceWithBinaryOperator(currentNode, isCommutative: false, CreateRightShiftExpression),
                RuleKind.op_Subtraction => ReplaceWithBinaryOperator(currentNode, isCommutative: false, generator.SubtractExpression),
                RuleKind.op_UnaryNegation => ReplaceWithUnaryOperator(currentNode, generator.NegateExpression),
                RuleKind.op_UnsignedRightShift => ReplaceWithBinaryOperator(currentNode, isCommutative: false, CreateUnsignedRightShiftExpression),
                _ => currentNode,
            };
        }
    }
}