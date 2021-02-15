// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class CSharpPreferDictionaryTryGetValueFixer : PreferDictionaryTryGetValueFixer
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.FirstOrDefault();
            var dictionaryAccessLocation = diagnostic?.AdditionalLocations[0];
            if (dictionaryAccessLocation is null)
            {
                return;
            }
            
            Document document = context.Document;
            SyntaxNode root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
            
            var dictionaryAccessNode = root.FindNode(dictionaryAccessLocation.SourceSpan);
            var dictionaryAccess = dictionaryAccessNode as ElementAccessExpressionSyntax ?? (dictionaryAccessNode as ArgumentSyntax)?.Expression as ElementAccessExpressionSyntax;
            if (dictionaryAccess is null
                || root.FindNode(context.Span) is not InvocationExpressionSyntax containsKeyInvocation 
                || containsKeyInvocation.Expression is not MemberAccessExpressionSyntax containsKeyAccess)
            {
                return;       
            }

            var action = CodeAction.Create(PreferDictionaryTryGetValueCodeFixTitle, async ct =>
            {
                var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
                var generator = editor.Generator;
                
                var tryGetValueAccess = generator.MemberAccessExpression(containsKeyAccess.Expression, "TryGetValue");
                var keyArgument = containsKeyInvocation.ArgumentList.Arguments.FirstOrDefault();
                
                var outArgument = generator.Argument(RefKind.Out, SyntaxFactory.DeclarationExpression(SyntaxFactory.IdentifierName("var"), SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier("value"))));
                var tryGetValueInvocation = generator.InvocationExpression(tryGetValueAccess, keyArgument, outArgument);
                editor.ReplaceNode(containsKeyInvocation, tryGetValueInvocation);
            
                editor.ReplaceNode(dictionaryAccess, generator.IdentifierName("value"));
            
                return editor.GetChangedDocument();
            });
            
            context.RegisterCodeFix(action, context.Diagnostics);
        }
    }
}