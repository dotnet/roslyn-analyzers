// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <summary>
    /// CA1827: Do not use Count() when Any() can be used.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpDoNotUseCountAsyncWhenAnyAsyncCanBeUsedFixer : DoNotUseCountWhenAnyCanBeUsedFixer
    {
        /// <summary>
        /// Tries the get a fixer the specified <paramref name="node" />.
        /// </summary>
        /// <param name="node">The node to get a fixer for.</param>
        /// <param name="expression">If this method returns <see langword="true" />, contains the expression to be used to invoke <c>Any</c>.</param>
        /// <param name="arguments">If this method returns <see langword="true" />, contains the arguments from <c>Any</c> to be used on <c>Count</c>.</param>
        /// <param name="negate">If this method returns <see langword="true" />, indicates whether to negate the expression.</param>
        /// <returns><see langword="true" /> if a fixer was found., <see langword="false" /> otherwise.</returns>
        protected override bool TryGetFixer(SyntaxNode node, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments, out bool negate)
        {
            if (node.IsKind(SyntaxKind.InvocationExpression))
            {
                GetFixerForEqualsMethod((InvocationExpressionSyntax)node, out expression, out arguments);
                negate = true;
                return true;
            }

            if (node.IsKind(SyntaxKind.EqualsExpression))
            {
                GetFixerForEqualityExpression((BinaryExpressionSyntax)node, out expression, out arguments);
                negate = true;
                return true;
            }

            if (node.IsKind(SyntaxKind.NotEqualsExpression))
            {
                GetFixerForEqualityExpression((BinaryExpressionSyntax)node, out expression, out arguments);
                negate = false;
                return true;
            }

            if (node.IsKind(SyntaxKind.LessThanExpression))
            {
                GetFixerForLessThanExpression((BinaryExpressionSyntax)node, out expression, out arguments, out negate);
                return true;
            }

            if (node.IsKind(SyntaxKind.LessThanOrEqualExpression))
            {
                GetFixerForLessThanOrEqualExpression((BinaryExpressionSyntax)node, out expression, out arguments, out negate);
                return true;
            }

            if (node.IsKind(SyntaxKind.GreaterThanExpression))
            {
                GetFixerForGreaterThanExpression((BinaryExpressionSyntax)node, out expression, out arguments, out negate);
                return true;
            }

            if (node.IsKind(SyntaxKind.GreaterThanOrEqualExpression))
            {
                GetFixerForGreaterThanOrEqualExpression((BinaryExpressionSyntax)node, out expression, out arguments, out negate);
                return true;
            }

            expression = default;
            arguments = default;
            negate = default;
            return false;
        }

        private static void GetFixerForEqualsMethod(InvocationExpressionSyntax equalsMethodInvocation, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments)
        {
            var argument = equalsMethodInvocation.ArgumentList.Arguments[0].Expression;
            var awaitExpression = argument is LiteralExpressionSyntax
                ? ((ParenthesizedExpressionSyntax)((MemberAccessExpressionSyntax)equalsMethodInvocation.Expression).Expression).Expression
                : argument;
            GetExpressionAndInvocationArguments(
                sourceExpression: awaitExpression,
                expression: out expression,
                arguments: out arguments);
        }

        private static void GetFixerForEqualityExpression(BinaryExpressionSyntax binaryExpression, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments)
        {
            GetExpressionAndInvocationArguments(
                sourceExpression: binaryExpression.Left is LiteralExpressionSyntax ? binaryExpression.Right : binaryExpression.Left,
                expression: out expression,
                arguments: out arguments);
        }

        private static void GetFixerForLessThanOrEqualExpression(BinaryExpressionSyntax binaryExpression, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments, out bool negate)
        {
            negate = binaryExpression.Right is LiteralExpressionSyntax;
            GetExpressionAndInvocationArguments(
                sourceExpression: negate ? binaryExpression.Left : binaryExpression.Right,
                expression: out expression,
                arguments: out arguments);
        }

        private static void GetFixerForLessThanExpression(BinaryExpressionSyntax binaryExpression, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments, out bool negate)
        {
            if (binaryExpression.Left is LiteralExpressionSyntax)
            {
                GetFixerForBinaryExpression(
                    sourceExpression: binaryExpression.Right,
                    literalExpression: (LiteralExpressionSyntax)binaryExpression.Left,
                    value: 0,
                    expression: out expression,
                    arguments: out arguments,
                    negate: out negate);
            }
            else
            {
                GetFixerForBinaryExpression(
                    sourceExpression: binaryExpression.Left,
                    literalExpression: (LiteralExpressionSyntax)binaryExpression.Right,
                    value: 1,
                    expression: out expression,
                    arguments: out arguments,
                    negate: out negate);
            }
        }

        private static void GetFixerForGreaterThanExpression(BinaryExpressionSyntax binaryExpression, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments, out bool negate)
        {
            negate = binaryExpression.Left is LiteralExpressionSyntax;
            GetExpressionAndInvocationArguments(
                sourceExpression: negate ? binaryExpression.Right : binaryExpression.Left,
                expression: out expression,
                arguments: out arguments);
        }

        private static void GetFixerForGreaterThanOrEqualExpression(BinaryExpressionSyntax binaryExpression, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments, out bool negate)
        {
            if (binaryExpression.Left is LiteralExpressionSyntax)
            {
                GetFixerForBinaryExpression(
                    sourceExpression: binaryExpression.Right,
                    literalExpression: (LiteralExpressionSyntax)binaryExpression.Left,
                    value: 1,
                    expression: out expression,
                    arguments: out arguments,
                    negate: out negate);
            }
            else
            {
                GetFixerForBinaryExpression(
                    sourceExpression: binaryExpression.Left,
                    literalExpression: (LiteralExpressionSyntax)binaryExpression.Right,
                    value: 0,
                    expression: out expression,
                    arguments: out arguments,
                    negate: out negate);
            }
        }

        private static void GetFixerForBinaryExpression(ExpressionSyntax sourceExpression, LiteralExpressionSyntax literalExpression, int value, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments, out bool negate)
        {
            GetExpressionAndInvocationArguments(sourceExpression, out expression, out arguments);
            negate = (int)literalExpression.Token.Value == value;
        }

        private static void GetExpressionAndInvocationArguments(ExpressionSyntax sourceExpression, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments)
        {
            if (sourceExpression is AwaitExpressionSyntax awaitExpression)
            {
                GetExpressionAndInvocationArguments(awaitExpression.Expression, out expression, out arguments);
            }
            else
            {
                var invocationExpression = (InvocationExpressionSyntax)sourceExpression;
                expression = ((MemberAccessExpressionSyntax)invocationExpression.Expression).Expression;
                arguments = invocationExpression.ArgumentList.ChildNodes();
            }
        }
    }
}
