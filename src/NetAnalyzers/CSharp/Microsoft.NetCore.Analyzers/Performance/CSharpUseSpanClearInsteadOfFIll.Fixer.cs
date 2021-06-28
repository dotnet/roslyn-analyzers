// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    /// <summary>
    /// CA1849: C# implementation of use Span.Clear instead of Span.Fill(default)
    /// Implements the <see cref="CodeFixProvider" />
    /// </summary>
    /// <seealso cref="UseSpanClearInsteadOfFillFixer"/>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpUseSpanClearInsteadOfFillFixer : UseSpanClearInsteadOfFillFixer
    {
        protected override SyntaxNode? GetInvocationTarget(SyntaxNode node)
        {
            if (node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax memberAccess
                })
            {
                return memberAccess.Expression;
            }

            return null;
        }
    }
}