// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    /// <summary>
    /// CA1815: Override equals and operator equals on value types
    /// </summary>
    public abstract class OverrideEqualsAndOperatorEqualsOnValueTypesFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(context.Document);
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxNode enclosingNode = root.FindNode(context.Span);
            SyntaxNode declaration = generator.GetDeclaration(enclosingNode);
            if (declaration == null)
            {
                return;
            }

            SemanticModel model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var typeSymbol = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
            if (typeSymbol == null)
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                new MyCodeAction(
                    MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesTitle,
                    async ct => await ImplementMissingMembersAsync(declaration, typeSymbol, context.Document, context.CancellationToken)),
                diagnostic);
        }

        private async Task<Document> ImplementMissingMembersAsync(
            SyntaxNode declaration,
            INamedTypeSymbol typeSymbol,
            Document document,
            CancellationToken ct)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            string language = document.Project.Language;

            if (!typeSymbol.OverridesEquals())
            {
                SyntaxNode equalsMethod = generator.MethodDeclaration(WellKnownMemberNames.ObjectEquals,
                                        new[] { generator.ParameterDeclaration("obj", generator.TypeExpression(SpecialType.System_Object)) },
                                        returnType: generator.TypeExpression(SpecialType.System_Boolean),
                                        accessibility: Accessibility.Public,
                                        modifiers: DeclarationModifiers.Override,
                                        statements: new[] { generator.ThrowStatement(generator.ObjectCreationExpression(generator.DottedName("System.NotImplementedException"))) });

                editor.AddMember(declaration, equalsMethod);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.EqualityOperatorName))
            {
                SyntaxNode equalityOperator = generator.OperatorDeclaration(OperatorKind.Equality,
                                                                   new SyntaxNode[]
                                                                   {
                                                                       generator.ParameterDeclaration("left", generator.TypeExpression(typeSymbol)),
                                                                       generator.ParameterDeclaration("right", generator.TypeExpression(typeSymbol)),
                                                                   },
                                                                   generator.TypeExpression(SpecialType.System_Boolean),
                                                                   Accessibility.Public,
                                                                   DeclarationModifiers.Static,
                                                                   new SyntaxNode[]
                                                                   {
                                                                       generator.ThrowStatement(generator.ObjectCreationExpression(generator.DottedName("System.NotImplementedException")))
                                                                   });


                editor.AddMember(declaration, equalityOperator);
            }

            if (!typeSymbol.ImplementsOperator(WellKnownMemberNames.InequalityOperatorName))
            {
                var inequalityOperator = generator.OperatorDeclaration(OperatorKind.Inequality,
                                                                   new SyntaxNode[]
                                                                   {
                                                                       generator.ParameterDeclaration("left", generator.TypeExpression(typeSymbol)),
                                                                       generator.ParameterDeclaration("right", generator.TypeExpression(typeSymbol)),
                                                                   },
                                                                   generator.TypeExpression(SpecialType.System_Boolean),
                                                                   Accessibility.Public,
                                                                   DeclarationModifiers.Static,
                                                                   new SyntaxNode[]
                                                                   {
                                                                       generator.ThrowStatement(generator.ObjectCreationExpression(generator.DottedName("System.NotImplementedException")))
                                                                   });


                editor.AddMember(declaration, inequalityOperator);
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