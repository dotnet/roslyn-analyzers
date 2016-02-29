// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2222: Do not decrease inherited member visibility
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class DoNotDecreaseInheritedMemberVisibilityFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotDecreaseInheritedMemberVisibilityAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var expression = root.FindNode(context.Span);
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var symbol = model.GetDeclaredSymbol(expression);

            if (expression != null)
            {
                context.RegisterCodeFix(new MakeInheritedMemberVisibleCodeAction(
                    string.Format(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDecreaseInheritedMemberVisibilityCodeFix, symbol.Name), 
                    async c => await IncreaseVisibility(context.Document, expression, c).ConfigureAwait(false)), 
                    context.Diagnostics);
            }
        }

        private async Task<Document> IncreaseVisibility(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var declaration = editor.Generator.GetDeclaration(node);
            var symbol = editor.SemanticModel.GetDeclaredSymbol(node);
            var ancestorTypes = symbol?.ContainingType?.GetBaseTypes() ?? Enumerable.Empty<INamedTypeSymbol>();
            var hiddenOrOverriddenMembers = ancestorTypes.SelectMany(t => t.GetMembers(symbol.Name));

            // Check if a public member was overridden
            if (hiddenOrOverriddenMembers.Any(s => s.DeclaredAccessibility == Accessibility.Public))
            {
                IncreaseVisibility(editor, declaration, Accessibility.Public);
                return editor.GetChangedDocument();
            }

            // Otherwise, check if a protected or internal member was overridden
            if (hiddenOrOverriddenMembers.Any(s => s.DeclaredAccessibility == Accessibility.ProtectedOrInternal))
            {
                IncreaseVisibility(editor, declaration, Accessibility.ProtectedOrInternal);
                return editor.GetChangedDocument();
            }

            // Otherwise, check if a protected member was overridden
            if (hiddenOrOverriddenMembers.Any(s => s.DeclaredAccessibility == Accessibility.Protected))
            {
                IncreaseVisibility(editor, declaration, Accessibility.Protected);
                return editor.GetChangedDocument();
            }

            // Otherwise, make no change
            return document;
        }

        private static void IncreaseVisibility(DocumentEditor editor, SyntaxNode declaration, Accessibility targetAccessibility)
        {
            if (declaration == null) return;
            var symbol = editor.SemanticModel.GetDeclaredSymbol(declaration);
            var property = (symbol as IMethodSymbol)?.AssociatedSymbol as IPropertySymbol;
            if (property?.DeclaredAccessibility <= targetAccessibility)
            {
                // Can't explicitly set an accessor to a visibility greater than or equal to that of the containing property
                editor.SetAccessibility(declaration, Accessibility.NotApplicable);
            }
            else
            {
                editor.SetAccessibility(declaration, targetAccessibility);
            }
        }

        private class MakeInheritedMemberVisibleCodeAction : DocumentChangeAction
        {
            public MakeInheritedMemberVisibleCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument) :
                base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => Title;
        }
    }
}