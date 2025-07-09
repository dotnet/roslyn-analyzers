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
                foreach (string customTag in diagnostic.Descriptor.CustomTags)
                {
                    if (!Enum.TryParse(customTag, out RuleKind ruleKind))
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
        }

        private async Task<Document> ReplaceWithCrossPlatformIntrinsicAsync(Document document, RuleKind ruleKind, IInvocationOperation invocation, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            SyntaxNode? replacementNode = null;

            switch (ruleKind)
            {
                case RuleKind.opAddition:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = generator.AddExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opBitwiseAnd:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = generator.BitwiseAndExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opBitwiseOr:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = generator.BitwiseOrExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opDivision:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = generator.DivideExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opExclusiveOr:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = CreateExclusiveOrExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opLeftShift:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = CreateLeftShiftExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opMultiply:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = generator.MultiplyExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opOnesComplement:
                {
                    if (invocation.Arguments.Length != 1)
                    {
                        break;
                    }

                    replacementNode = generator.BitwiseNotExpression(
                        invocation.Arguments[0].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opRightShift:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = CreateRightShiftExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opSubtraction:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = generator.SubtractExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opUnaryNegation:
                {
                    if (invocation.Arguments.Length != 1)
                    {
                        break;
                    }

                    replacementNode = generator.NegateExpression(
                        invocation.Arguments[0].Value.Syntax
                    );
                    break;
                }

                case RuleKind.opUnsignedRightShift:
                {
                    if (invocation.Arguments.Length != 2)
                    {
                        break;
                    }

                    replacementNode = CreateUnsignedRightShiftExpression(
                        invocation.Arguments[0].Value.Syntax,
                        invocation.Arguments[1].Value.Syntax
                    );
                    break;
                }

                default:
                {
                    break;
                }
            }

            if (replacementNode is not null)
            {
                editor.ReplaceNode(invocation.Syntax, replacementNode.WithTriviaFrom(invocation.Syntax));
                document = document.WithSyntaxRoot(editor.GetChangedRoot());
            }

            return document;
        }

        protected abstract SyntaxNode CreateExclusiveOrExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode CreateLeftShiftExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode CreateRightShiftExpression(SyntaxNode left, SyntaxNode right);
        protected abstract SyntaxNode? CreateUnsignedRightShiftExpression(SyntaxNode left, SyntaxNode right);
    }
}