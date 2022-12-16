// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Runtime;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpDoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer : DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyFixer
    {
        private protected sealed override SyntaxNode? AdjustSyntaxNode(SyntaxNode? syntaxNode)
        {
            if (syntaxNode?.Parent.IsKind(SyntaxKind.SuppressNullableWarningExpression) == true)
            {
                return syntaxNode.Parent;
            }

            return syntaxNode;
        }

        private protected override bool IsConditionalAccess(SyntaxNode? syntaxNode)
            => syntaxNode is ConditionalAccessExpressionSyntax;

        private protected override SyntaxNode? ConditionalElementAccessExpression(SyntaxNode expression, SyntaxNode whenNotNull)
        {
            var arguments = SeparatedList(new ArgumentSyntax[] { Argument((ExpressionSyntax)whenNotNull) });
            var elementBinding = ElementBindingExpression(BracketedArgumentList(arguments));
            return ConditionalAccessExpression((ExpressionSyntax)expression, elementBinding);
        }
    }
}
