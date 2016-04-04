// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
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

            context.RegisterCodeFix(new MyCodeAction(MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementComparable,
                                                     async ct => await ImplementComparable(context.Document, declaration, typeSymbol, ct).ConfigureAwait(false)),
                                    diagnostic);
        }

        private async Task<Document> ImplementComparable(Document document, SyntaxNode declaration, INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;

            if (!typeSymbol.OverridesEquals())
            {
                SyntaxNode equalsMethod = generator.EqualsOverrideDeclaration();

                editor.AddMember(declaration, equalsMethod);
            }

            if (!typeSymbol.OverridesGetHashCode())
            {
                SyntaxNode getHashCodeMethod = generator.GetHashCodeOverrideDeclaration();

                editor.AddMember(declaration, getHashCodeMethod);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.EqualityOperatorName))
            {
                SyntaxNode equalityOperator = generator.ComparisonOperatorDeclaration(OperatorKind.Equality, typeSymbol);

                editor.AddMember(declaration, equalityOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.InequalityOperatorName))
            {
                SyntaxNode inequalityOperator = generator.ComparisonOperatorDeclaration(OperatorKind.Inequality, typeSymbol);

                editor.AddMember(declaration, inequalityOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.LessThanOperatorName))
            {
                SyntaxNode lessThanOperator = generator.ComparisonOperatorDeclaration(OperatorKind.LessThan, typeSymbol);

                editor.AddMember(declaration, lessThanOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.GreaterThanOperatorName))
            {
                SyntaxNode greaterThanOperator = generator.ComparisonOperatorDeclaration(OperatorKind.GreaterThan, typeSymbol);

                editor.AddMember(declaration, greaterThanOperator);
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
