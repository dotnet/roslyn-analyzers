// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.NetCore.Analyzers.Usage;

namespace Microsoft.NetCore.CSharp.Analyzers.Usage
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

            string? newMember = memberAccessExpression.Name.ToString() switch
            {
                "OrderBy" => "ThenBy",
                "OrderByDescending" => "ThenByDescending",
                _ => null
            };

            if (newMember is null)
                return document; // should we throw NotSupported at this point?

            var generatedSyntax = SyntaxGenerator.GetGenerator(document).IdentifierName(newMember);

            var newRoot = root.ReplaceNode(memberAccessExpression.Name, generatedSyntax);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
