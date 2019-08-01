// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <summary>
    /// CA1829: Use property instead of <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>, when available.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpUsePropertyInsteadOfCountMethodWhenAvailableFixer : UsePropertyInsteadOfCountMethodWhenAvailableFixer
    {
        /// <summary>
        /// Gets the expression from the specified <paramref name="node" /> where to replace the invocation of the
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> method with a property invocation.
        /// </summary>
        /// <param name="node">The node to get a fixer for.</param>
        /// <returns>The expression from the specified <paramref name="node" /> where to replace the invocation of the
        /// <see cref="System.Linq.Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> method with a property invocation
        /// if found; <see langword="null" /> otherwise.</returns>
        protected override SyntaxNode GetExpression(SyntaxNode node)
        {
            if (node is InvocationExpressionSyntax invocationExpression)
            {
                return ((MemberAccessExpressionSyntax)invocationExpression.Expression).Expression;
            }

            return null;
        }
    }
}
