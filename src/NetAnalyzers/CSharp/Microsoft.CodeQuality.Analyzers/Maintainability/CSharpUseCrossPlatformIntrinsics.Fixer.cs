// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeQuality.Analyzers.Maintainability;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Maintainability
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpUseCrossPlatformIntrinsicsFixer : UseCrossPlatformIntrinsicsFixer
    {
        protected override SyntaxNode CreateExclusiveOrExpression(SyntaxNode left, SyntaxNode right)
            => SyntaxFactory.BinaryExpression(SyntaxKind.ExclusiveOrExpression, (ExpressionSyntax)left, (ExpressionSyntax)right);

        protected override SyntaxNode CreateLeftShiftExpression(SyntaxNode left, SyntaxNode right)
            => SyntaxFactory.BinaryExpression(SyntaxKind.LeftShiftExpression, (ExpressionSyntax)left, (ExpressionSyntax)right);

        protected override SyntaxNode CreateRightShiftExpression(SyntaxNode left, SyntaxNode right)
            => SyntaxFactory.BinaryExpression(SyntaxKind.RightShiftExpression, (ExpressionSyntax)left, (ExpressionSyntax)right);

        protected override SyntaxNode? CreateUnsignedRightShiftExpression(SyntaxNode left, SyntaxNode right)
        {
            const SyntaxKind UnsignedRightShiftExpression = (SyntaxKind)8692;
            const LanguageVersion CSharp11 = (LanguageVersion)1100;

            if (!Enum.IsDefined(typeof(SyntaxKind), UnsignedRightShiftExpression))
            {
                return null;
            }

            if (((CSharpParseOptions)left.SyntaxTree.Options).LanguageVersion < CSharp11)
            {
                return null;
            }

            return SyntaxFactory.BinaryExpression(UnsignedRightShiftExpression, (ExpressionSyntax)left, (ExpressionSyntax)right);
        }
    }
}
