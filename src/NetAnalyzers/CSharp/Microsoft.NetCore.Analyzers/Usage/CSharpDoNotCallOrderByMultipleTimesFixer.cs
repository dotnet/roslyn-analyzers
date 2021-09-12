// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeQuality.Analyzers.Usage;

namespace Microsoft.CodeQuality.CSharp.Analyzers.Usage
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpDoNotCallOrderByMultipleTimesFixer : DoNotCallOrderByMultipleTimesFixer
    {
        protected override Document ReplaceOrderByWithThenBy(Document document, SyntaxNode root, SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax invocation ||
                invocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
            {
                return document;
            }

            SyntaxNode newRoot;

            string newMember;

            switch (memberAccessExpression.Name.ToString())
            {
                case "OrderBy":
                    newMember = "ThenBy";
                    break;
                case "OrderByDescending":
                    newMember = "ThenByDescending";
                    break;
                default:
                    return document; // should we throw NotSupported at this point?
            }

            var generatedSyntax = SyntaxGenerator.GetGenerator(document).IdentifierName(newMember);

            newRoot = root.ReplaceNode(memberAccessExpression.Name, generatedSyntax);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
