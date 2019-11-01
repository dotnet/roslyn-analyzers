﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <summary>
    /// CA1827: Do not use Count()/LongCount() when Any() can be used.
    /// CA1828: Do not use CountAsync()/LongCountAsync() when AnyAsync() can be used.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpDoNotUseCountWhenAnyCanBeUsedFixer : DoNotUseCountWhenAnyCanBeUsedFixer
    {
        /// <summary>
        /// Tries the get a fixer the specified <paramref name="node" />.
        /// </summary>
        /// <param name="node">The node to get a fixer for.</param>
        /// <param name="operation">The operation to get the fixer from.</param>
        /// <param name="isAsync"><see langword="true" /> if it's an asynchronous method; <see langword="false" /> otherwise.</param>
        /// <param name="expression">If this method returns <see langword="true" />, contains the expression to be used to invoke <c>Any</c>.</param>
        /// <param name="arguments">If this method returns <see langword="true" />, contains the arguments from <c>Any</c> to be used on <c>Count</c>.</param>
        /// <returns><see langword="true" /> if a fixer was found., <see langword="false" /> otherwise.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool TryGetFixer(SyntaxNode node, string operation, bool isAsync, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments)
        {
            switch (operation)
            {
                case DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationEqualsInstance:
                    {
                        if (node is InvocationExpressionSyntax invocation &&
                            invocation.Expression is MemberAccessExpressionSyntax member)
                        {
                            GetExpressionAndInvocationArguments(
                                sourceExpression: member.Expression,
                                isAsync: isAsync,
                                expression: out expression,
                                arguments: out arguments);

                            return true;
                        }

                        break;
                    }
                case DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationEqualsArgument:
                    {
                        if (node is InvocationExpressionSyntax invocation &&
                            invocation.ArgumentList.Arguments.Count == 1)
                        {
                            GetExpressionAndInvocationArguments(
                                sourceExpression: invocation.ArgumentList.Arguments[0].Expression,
                                isAsync: isAsync,
                                expression: out expression,
                                arguments: out arguments);

                            return true;
                        }

                        break;
                    }
                case DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationBinaryLeft:
                    {
                        if (node is BinaryExpressionSyntax binary)
                        {
                            GetExpressionAndInvocationArguments(
                                sourceExpression: binary.Left,
                                isAsync: isAsync,
                                expression: out expression,
                                arguments: out arguments);

                            return true;
                        }

                        break;
                    }
                case DoNotUseCountWhenAnyCanBeUsedAnalyzer.OperationBinaryRight:
                    {
                        if (node is BinaryExpressionSyntax binary)
                        {
                            GetExpressionAndInvocationArguments(
                                sourceExpression: binary.Right,
                                isAsync: isAsync,
                                expression: out expression,
                                arguments: out arguments);

                            return true;
                        }

                        break;
                    }
            }

            expression = default;
            arguments = default;
            return false;
        }

        private static void GetExpressionAndInvocationArguments(ExpressionSyntax sourceExpression, bool isAsync, out SyntaxNode expression, out IEnumerable<SyntaxNode> arguments)
        {
            while (sourceExpression is ParenthesizedExpressionSyntax parenthesizedExpression)
            {
                sourceExpression = parenthesizedExpression.Expression;
            }

            InvocationExpressionSyntax invocationExpression = null;

            if (isAsync)
            {
                if (sourceExpression is AwaitExpressionSyntax awaitExpressionSyntax)
                {
                    invocationExpression = awaitExpressionSyntax.Expression as InvocationExpressionSyntax;
                }
            }
            else
            {
                invocationExpression = sourceExpression as InvocationExpressionSyntax;
            }

            if (invocationExpression is null)
            {
                expression = default;
                arguments = default;
                return;
            }

            expression = ((MemberAccessExpressionSyntax)invocationExpression.Expression).Expression;
            arguments = invocationExpression.ArgumentList.ChildNodes();
        }
    }
}
