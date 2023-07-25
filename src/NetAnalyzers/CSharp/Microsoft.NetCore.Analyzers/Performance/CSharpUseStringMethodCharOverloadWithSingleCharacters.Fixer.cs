// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpUseStringMethodCharOverloadWithSingleCharactersFixer : UseStringMethodCharOverloadWithSingleCharactersFixer
    {
        protected override bool TryGetChar(SemanticModel model, SyntaxNode argumentListNode, out char c)
        {
            c = default;

            if (argumentListNode is not ArgumentListSyntax argumentList)
            {
                return false;
            }

            ArgumentSyntax? stringArgumentNode = null;
            foreach (var argument in argumentList.Arguments)
            {
                var argumentOperation = model.GetOperation(argument) as IArgumentOperation;
                if (argumentOperation?.Parameter != null && argumentOperation.Parameter.Ordinal == 0)
                {
                    stringArgumentNode = argument;
                    break;
                }
            }

            if (stringArgumentNode != null &&
                stringArgumentNode.Expression is LiteralExpressionSyntax containedLiteralExpressionSyntax)
            {
                return TryGetCharFromLiteralExpressionSyntax(containedLiteralExpressionSyntax, out c);
            }

            return false;

            static bool TryGetCharFromLiteralExpressionSyntax(LiteralExpressionSyntax sourceLiteralExpressionSyntax, out char parsedCharLiteral)
            {
                parsedCharLiteral = default;
                if (sourceLiteralExpressionSyntax.Token.Value is string sourceLiteralValue && char.TryParse(sourceLiteralValue, out parsedCharLiteral))
                {
                    return true;
                }

                return false;
            }
        }

        protected override CodeAction CreateCodeAction(Document document, SyntaxNode argumentListNode, char sourceCharLiteral)
        {
            return new CSharpReplaceStringLiteralWithCharLiteralCodeAction(document, argumentListNode, sourceCharLiteral);
        }

        private sealed class CSharpReplaceStringLiteralWithCharLiteralCodeAction : ReplaceStringLiteralWithCharLiteralCodeAction
        {
            public CSharpReplaceStringLiteralWithCharLiteralCodeAction(Document document, SyntaxNode argumentListNode, char sourceCharLiteral) : base(document, argumentListNode, sourceCharLiteral)
            {
            }

            protected override void ApplyFix(
                DocumentEditor editor,
                SyntaxNode oldArgumentListNode,
                char c)
            {
                var argumentNode = editor.Generator.Argument(editor.Generator.LiteralExpression(c));
                var argumentListNode = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { argumentNode }));

                editor.ReplaceNode(oldArgumentListNode, argumentListNode);
            }
        }
    }
}
