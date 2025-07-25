// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Composition;
using System.Diagnostics;
using Analyzer.Utilities.Lightup;
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
            const LanguageVersion CSharp11 = (LanguageVersion)1100;

            if (!Enum.IsDefined(typeof(SyntaxKind), SyntaxKindEx.UnsignedRightShiftExpression))
            {
                return null;
            }

            if ((left.SyntaxTree.Options is not CSharpParseOptions csharpParseOptions) || (csharpParseOptions.LanguageVersion < CSharp11))
            {
                return null;
            }

            return SyntaxFactory.BinaryExpression(SyntaxKindEx.UnsignedRightShiftExpression, (ExpressionSyntax)left, (ExpressionSyntax)right);
        }

        protected override SyntaxNode ReplaceWithBinaryOperator(SyntaxNode currentNode, bool isCommutative, Func<SyntaxNode, SyntaxNode, SyntaxNode?> binaryOpFunc)
        {
            if (currentNode is not InvocationExpressionSyntax invocationExpression)
            {
                Debug.Fail($"Found unexpected node kind: {currentNode.RawKind}");
                return currentNode;
            }

            SeparatedSyntaxList<ArgumentSyntax> arguments = invocationExpression.ArgumentList.Arguments;

            if (arguments.Count != 2)
            {
                Debug.Fail($"Found unexpected number of arguments for binary operator replacement: {arguments.Count}");
                return currentNode;
            }

            if (binaryOpFunc(arguments[0].Expression, arguments[1].Expression) is not ExpressionSyntax replacementExpression)
            {
                return currentNode;
            }

            return SyntaxFactory.ParenthesizedExpression(replacementExpression);
        }

        protected override SyntaxNode ReplaceWithUnaryOperator(SyntaxNode currentNode, Func<SyntaxNode, SyntaxNode?> unaryOpFunc)
        {
            if (currentNode is not InvocationExpressionSyntax invocationExpression)
            {
                Debug.Fail($"Found unexpected node kind: {currentNode.RawKind}");
                return currentNode;
            }

            SeparatedSyntaxList<ArgumentSyntax> arguments = invocationExpression.ArgumentList.Arguments;

            if (arguments.Count != 1)
            {
                Debug.Fail($"Found unexpected number of arguments for unary operator replacement: {arguments.Count}");
                return currentNode;
            }

            if (unaryOpFunc(arguments[0].Expression) is not ExpressionSyntax replacementExpression)
            {
                return currentNode;
            }

            return SyntaxFactory.ParenthesizedExpression(replacementExpression);
        }
    }
}
