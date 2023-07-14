﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpDoNotGuardSetAddOrRemoveByContainsFixer : DoNotGuardSetAddOrRemoveByContainsFixer
    {
        protected override bool SyntaxSupportedByFixer(SyntaxNode conditionalSyntax)
        {
            if (conditionalSyntax is ConditionalExpressionSyntax conditionalExpressionSyntax)
                return conditionalExpressionSyntax.WhenTrue.ChildNodes().Count() == 1;

            if (conditionalSyntax is IfStatementSyntax)
                return true;

            return false;
        }

        protected override Document ReplaceConditionWithChild(Document document, SyntaxNode root, SyntaxNode conditionalOperationNode, SyntaxNode childOperationNode)
        {
            SyntaxNode newRoot;

            if (conditionalOperationNode is ConditionalExpressionSyntax conditionalExpressionSyntax &&
                conditionalExpressionSyntax.WhenFalse.ChildNodes().Any())
            {
                var expression = GetNegatedExpression(document, childOperationNode);

                SyntaxNode newConditionalOperationNode = conditionalExpressionSyntax
                    .WithCondition((ExpressionSyntax)expression)
                    .WithWhenTrue(conditionalExpressionSyntax.WhenFalse)
                    .WithWhenFalse(null!)
                    .WithAdditionalAnnotations(Formatter.Annotation).WithTriviaFrom(conditionalOperationNode);

                newRoot = root.ReplaceNode(conditionalOperationNode, newConditionalOperationNode);
            }
            else if (conditionalOperationNode is IfStatementSyntax ifStatementSyntax && ifStatementSyntax.Else != null)
            {
                var expression = GetNegatedExpression(document, childOperationNode);

                SyntaxNode newConditionalOperationNode = ifStatementSyntax
                    .WithCondition((ExpressionSyntax)expression)
                    .WithStatement(ifStatementSyntax.Else.Statement)
                    .WithElse(null)
                    .WithAdditionalAnnotations(Formatter.Annotation).WithTriviaFrom(conditionalOperationNode);

                newRoot = root.ReplaceNode(conditionalOperationNode, newConditionalOperationNode);
            }
            else
            {
                SyntaxNode newConditionNode = childOperationNode
                    .WithAdditionalAnnotations(Formatter.Annotation)
                    .WithTriviaFrom(conditionalOperationNode);

                newRoot = root.ReplaceNode(conditionalOperationNode, newConditionNode);
            }

            return document.WithSyntaxRoot(newRoot);
        }

        private static SyntaxNode GetNegatedExpression(Document document, SyntaxNode newConditionNode)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            return generator.LogicalNotExpression(((ExpressionStatementSyntax)newConditionNode).Expression.WithoutTrivia());
        }
    }
}
