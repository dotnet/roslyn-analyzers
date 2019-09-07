// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.NetCore.Analyzers;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpMutableStructsShouldNotBeUsedForReadonlyFieldsFixer : NetCore.Analyzers.Performance.MutableStructsShouldNotBeUsedForReadonlyFieldsFixer
    {
        protected override void AnalyzeCodeFix(CodeFixContext context, SyntaxNode targetNode)
        {
            if (!(targetNode is FieldDeclarationSyntax fieldDeclarationSyntax))
            {
                return;
            }

            var readonlyModifiers =
                fieldDeclarationSyntax.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword)).ToArray();

            if (!readonlyModifiers.Any())
            {
                return;
            }

            var removeReadonlyAction = CodeAction.Create(
                MicrosoftNetCoreAnalyzersResources.MutableStructsShouldNotBeUsedForReadonlyFieldsTitle,
                async ct => await RemoveReadonlyKeyword(context, fieldDeclarationSyntax).ConfigureAwait(false),
                EquivalencyKey);

            context.RegisterCodeFix(removeReadonlyAction, context.Diagnostics);
        }

        private static async Task<Document> RemoveReadonlyKeyword(CodeFixContext context,
            FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            var editor = await DocumentEditor.CreateAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
            var withoutReadonly = fieldDeclarationSyntax.WithModifiers(
                new SyntaxTokenList(
                    fieldDeclarationSyntax.Modifiers.Where(modifier => !modifier.IsKind(SyntaxKind.ReadOnlyKeyword))));

            editor.ReplaceNode(fieldDeclarationSyntax, withoutReadonly);

            return editor.GetChangedDocument();
        }
    }
}