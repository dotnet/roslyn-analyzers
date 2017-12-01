// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    /// <summary>
    /// CA1801: Review unused parameters
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpReviewUnusedParametersFixer : ReviewUnusedParametersFixer
    {
        protected override SyntaxNode GetOperationNode(SyntaxNode node)
        {
            if (node.Kind() == SyntaxKind.SimpleMemberAccessExpression)
            {
                return node.Parent;
            }

            return node;
        }

        protected override SyntaxNode GetParameterNode(SyntaxNode node)
        {
            return node;
        }
    }
}