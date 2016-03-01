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
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode expression = root.FindNode(context.Span);
            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            ISymbol symbol = model.GetDeclaredSymbol(expression);

            // An accessor without an explicit accessibility means that the parent property is actually the 
            // offending symbol. Therefore, don't offer the code fix on the accessor level.
            // Note that the declared accessibility on an accessor without an explicit accessibility will return that of the
            // property it's within, so it's necessary to compare the accessor's accessibility with the property's
            if (symbol.IsAccessorMethod() && symbol.DeclaredAccessibility == (symbol as IMethodSymbol)?.AssociatedSymbol?.DeclaredAccessibility)
            {
                return;
            }

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
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode declaration = editor.Generator.GetDeclaration(node);
            ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(node);
            System.Collections.Generic.IEnumerable<INamedTypeSymbol> ancestorTypes = symbol?.ContainingType?.GetBaseTypes() ?? Enumerable.Empty<INamedTypeSymbol>();
            System.Collections.Generic.IEnumerable<ISymbol> hiddenOrOverriddenMembers = ancestorTypes.SelectMany(t => t.GetMembers(symbol.Name));

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
            ISymbol symbol = editor.SemanticModel.GetDeclaredSymbol(declaration);
            var property = (symbol as IMethodSymbol)?.AssociatedSymbol as IPropertySymbol;
            if (property != null && IsMoreRestrictive(property.DeclaredAccessibility, targetAccessibility))
            {
                // Can't explicitly set an accessor to a visibility greater than or equal to that of the containing property
                editor.SetAccessibility(declaration, Accessibility.NotApplicable);
            }
            else
            {
                editor.SetAccessibility(declaration, targetAccessibility);
            }
        }

        // Returns true if a1 is equal to or less visible than a2
        private static bool IsMoreRestrictive(Accessibility a1, Accessibility a2)
        {
            switch (a2)
            {
                case Accessibility.Public:
                    return true;
                case Accessibility.ProtectedOrInternal:
                    return a1 != Accessibility.Public;
                case Accessibility.Protected:
                    return a1 != Accessibility.Public && a1 != Accessibility.ProtectedOrInternal && a1 != Accessibility.ProtectedOrFriend;
                case Accessibility.Internal:
                    return a1 != Accessibility.Public && a1 != Accessibility.ProtectedOrInternal && a1 != Accessibility.ProtectedOrFriend && a1 != Accessibility.Protected;
                case Accessibility.ProtectedAndInternal:
                    return a1 != Accessibility.Public && a1 != Accessibility.ProtectedOrInternal && a1 != Accessibility.ProtectedOrFriend && a1 != Accessibility.Protected && a1 != Accessibility.Internal;
                default:
                    return false;
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