// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpPreferLengthCountIsEmptyOverAnyFixer : PreferLengthCountIsEmptyOverAnyFixer
    {
        protected override SyntaxNode? ReplaceAnyWithIsEmpty(SyntaxNode root, SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation)
            {
                return null;
            }

            var newMemberAccess = memberAccess.WithName(
                IdentifierName(PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyText)
            );
            if (invocation.Parent is PrefixUnaryExpressionSyntax prefixExpression && prefixExpression.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return root.ReplaceNode(prefixExpression, newMemberAccess.WithTriviaFrom(prefixExpression));
            }

            var negatedExpression = PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                newMemberAccess
            );

            return root.ReplaceNode(invocation, negatedExpression.WithTriviaFrom(invocation));
        }

        protected override SyntaxNode? ReplaceAnyWithLength(SyntaxNode root, SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation)
            {
                return null;
            }

            const string lengthMemberName = PreferLengthCountIsEmptyOverAnyAnalyzer.LengthText;
            if (invocation.Parent is PrefixUnaryExpressionSyntax prefixExpression && prefixExpression.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var binaryExpression = GetBinaryExpression(memberAccess, lengthMemberName, SyntaxKind.EqualsExpression);

                return root.ReplaceNode(prefixExpression, binaryExpression.WithTriviaFrom(prefixExpression));
            }

            return root.ReplaceNode(invocation, GetBinaryExpression(memberAccess, lengthMemberName, SyntaxKind.NotEqualsExpression).WithTriviaFrom(invocation));
        }

        protected override SyntaxNode? ReplaceAnyWithCount(SyntaxNode root, SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation)
            {
                return null;
            }

            const string countMemberName = PreferLengthCountIsEmptyOverAnyAnalyzer.CountText;
            if (invocation.Parent is PrefixUnaryExpressionSyntax prefixExpression && prefixExpression.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var binaryExpression = GetBinaryExpression(memberAccess, countMemberName, SyntaxKind.EqualsExpression);

                return root.ReplaceNode(prefixExpression, binaryExpression.WithTriviaFrom(prefixExpression));
            }

            return root.ReplaceNode(invocation, GetBinaryExpression(memberAccess, countMemberName, SyntaxKind.NotEqualsExpression).WithTriviaFrom(invocation));
        }

        private static BinaryExpressionSyntax GetBinaryExpression(MemberAccessExpressionSyntax originalMemberAccess, string member, SyntaxKind expressionKind)
        {
            return BinaryExpression(
                expressionKind,
                originalMemberAccess.WithName(
                    IdentifierName(member)
                ),
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(0)
                )
            );
        }
    }
}