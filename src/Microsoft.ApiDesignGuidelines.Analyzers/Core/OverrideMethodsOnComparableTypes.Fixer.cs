// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class OverrideMethodsOnComparableTypesFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OverrideMethodsOnComparableTypesAnalyzer.RuleId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(context.Document);
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxNode declaration = root.FindNode(context.Span);
            declaration = generator.GetDeclaration(declaration);
            if (declaration == null)
            {
                return;
            }

            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var typeSymbol = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
            if (typeSymbol?.TypeKind != TypeKind.Class &&
                typeSymbol?.TypeKind != TypeKind.Struct)
            {
                return;
            }

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(
                new MyCodeAction(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementComparable,
                    async ct => await ImplementComparableAsync(context.Document, declaration, typeSymbol, ct).ConfigureAwait(false)), diagnostic);
        }

        private async Task<Document> ImplementComparableAsync(Document document, SyntaxNode declaration, INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;

            if (!typeSymbol.OverridesEquals())
            {
                var equalsMethod = generator.EqualsOverrideDeclaration(editor.SemanticModel.Compilation);

                editor.AddMember(declaration, equalsMethod);
            }

            if (!typeSymbol.OverridesGetHashCode())
            {
                var getHashCodeMethod = generator.GetHashCodeOverrideDeclaration(editor.SemanticModel.Compilation);

                editor.AddMember(declaration, getHashCodeMethod);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.EqualityOperatorName))
            {
                var equalityOperator = generator.OperatorEqualityDeclaration(typeSymbol);

                editor.AddMember(declaration, equalityOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.InequalityOperatorName))
            {
                var inequalityOperator = generator.OperatorInequalityDeclaration(typeSymbol);

                editor.AddMember(declaration, inequalityOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.LessThanOperatorName))
            {
                var lessThanOperator = generator.OperatorLessThanDeclaration(typeSymbol);

                editor.AddMember(declaration, lessThanOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.LessThanOrEqualOperatorName))
            {
                var lessThanOrEqualOperator = generator.OperatorLessThanOrEqualDeclaration(typeSymbol);

                editor.AddMember(declaration, lessThanOrEqualOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.GreaterThanOperatorName))
            {
                var greaterThanOperator = generator.OperatorGreaterThanDeclaration(typeSymbol);

                editor.AddMember(declaration, greaterThanOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.GreaterThanOrEqualOperatorName))
            {
                var greaterThanOrEqualOperator = generator.OperatorGreaterThanOrEqualDeclaration(typeSymbol);

                editor.AddMember(declaration, greaterThanOrEqualOperator);
            }

            return editor.GetChangedDocument();
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }
        }
    }
}
