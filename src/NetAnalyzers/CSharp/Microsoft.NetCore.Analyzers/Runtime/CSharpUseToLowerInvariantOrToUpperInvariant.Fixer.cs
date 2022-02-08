// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class CSharpUseToLowerInvariantOrToUpperInvariantFixer : UseToLowerInvariantOrToUpperInvariantFixerBase
    {
        protected override bool ShouldFix(SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.IdentifierName) &&
                (node.Parent?.IsKind(SyntaxKind.SimpleMemberAccessExpression) == true || node.Parent?.IsKind(SyntaxKind.MemberBindingExpression) == true);
        }

        protected override Task<Document> FixInvocationAsync(Document document, SyntaxGenerator syntaxGenerator, SyntaxNode root, SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.IdentifierName))
            {
                if (node.Parent?.IsKind(SyntaxKind.SimpleMemberAccessExpression) == true)
                {
                    var memberAccess = (MemberAccessExpressionSyntax)node.Parent;
                    var replacementMethodName = GetReplacementMethodName(memberAccess.Name.Identifier.Text);
                    var newMemberAccess = memberAccess.WithName((SimpleNameSyntax)syntaxGenerator.IdentifierName(replacementMethodName)).WithAdditionalAnnotations(Formatter.Annotation);
                    var newRoot = root.ReplaceNode(memberAccess, newMemberAccess);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }

                if (node.Parent?.IsKind(SyntaxKind.MemberBindingExpression) == true)
                {
                    var memberBinding = (MemberBindingExpressionSyntax)node.Parent;
                    var replacementMethodName = GetReplacementMethodName(memberBinding.Name.Identifier.Text);
                    var newMemberBinding = memberBinding.WithName((SimpleNameSyntax)syntaxGenerator.IdentifierName(replacementMethodName)).WithAdditionalAnnotations(Formatter.Annotation);
                    var newRoot = root.ReplaceNode(memberBinding, newMemberBinding);
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }
            }

            return Task.FromResult(document);
        }
    }
}
