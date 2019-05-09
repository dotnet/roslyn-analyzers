// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Diagnostics.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(LinkedEnumerationCodeFix))]
    [Shared]
    internal class LinkedEnumerationCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            LinkedEnumerationAnalyzer.MissingMembersRule.Id,
            LinkedEnumerationAnalyzer.MismatchedValueRule.Id,
            LinkedEnumerationAnalyzer.MismatchedNameRule.Id);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case RoslynDiagnosticIds.LinkedEnumerationShouldHaveAllMembersRuleId:
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldHaveAllMembersFix,
                                cancellationToken => AddMissingEnumerationMembersAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                                equivalenceKey: diagnostic.Id),
                            diagnostic);
                        break;

                    case RoslynDiagnosticIds.LinkedEnumerationShouldMatchNameRuleId:
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchValueFix,
                                cancellationToken => FixEnumerationMemberNameAsync(context.Document, cancellationToken),
                                equivalenceKey: diagnostic.Id),
                            diagnostic);
                        break;

                    case RoslynDiagnosticIds.LinkedEnumerationShouldMatchValueRuleId:
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchValueFix,
                                cancellationToken => FixEnumerationValueAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                                equivalenceKey: diagnostic.Id),
                            diagnostic);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private async Task<Document> AddMissingEnumerationMembersAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var reportedNode = root.FindNode(sourceSpan, getInnermostNodeForTie: true);
            var generator = SyntaxGenerator.GetGenerator(document);

            var linkedEnumerationNode = TryGetContainingDeclarationOfKind(generator, reportedNode, DeclarationKind.Enum);
            var linkedEnumeration = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(linkedEnumerationNode, cancellationToken);
            var sourceEnumeration = GetSourceEnumType(semanticModel, linkedEnumerationNode, cancellationToken);

            var linkedEnumMembers = new HashSet<string>();
            foreach (var enumMember in linkedEnumeration.GetMembers())
            {
                if (!(enumMember is IFieldSymbol field))
                {
                    continue;
                }

                linkedEnumMembers.Add(field.Name);
            }

            var newMembers = new List<SyntaxNode>();
            foreach (var enumMember in sourceEnumeration.GetMembers())
            {
                if (!(enumMember is IFieldSymbol field) || linkedEnumMembers.Contains(field.Name))
                {
                    continue;
                }

                newMembers.Add(generator.EnumMember(
                    field.Name,
                    generator.MemberAccessExpression(
                        GetSourceEnumExpression(generator, semanticModel, linkedEnumerationNode, cancellationToken),
                        field.Name)));
            }

            return document.WithSyntaxRoot(root.ReplaceNode(linkedEnumerationNode, generator.AddMembers(linkedEnumerationNode, newMembers)));
        }

        private Task<Document> FixEnumerationMemberNameAsync(Document document, CancellationToken cancellationToken)
        {
            _ = cancellationToken;

            // Currently doesn't do anything
            return Task.FromResult(document);
        }

        private async Task<Document> FixEnumerationValueAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var member = root.FindNode(sourceSpan, getInnermostNodeForTie: true);
            var generator = SyntaxGenerator.GetGenerator(document);

            var declaration = TryGetContainingDeclarationOfKind(generator, member, DeclarationKind.EnumMember);
            var containingEnum = TryGetContainingDeclarationOfKind(generator, declaration, DeclarationKind.Enum);
            if (containingEnum is null)
            {
                return document;
            }

            var newNode = generator.EnumMember(
                generator.GetName(declaration),
                generator.MemberAccessExpression(
                    GetSourceEnumExpression(generator, semanticModel, containingEnum, cancellationToken),
                    generator.GetName(declaration)));

            return document.WithSyntaxRoot(root.ReplaceNode(declaration, newNode));
        }

        private static SyntaxNode GetSourceEnumExpression(SyntaxGenerator generator, SemanticModel semanticModel, SyntaxNode linkedEnumeration, CancellationToken cancellationToken)
        {
            var sourceEnumeration = GetSourceEnumType(semanticModel, linkedEnumeration, cancellationToken);
            return generator.TypeExpressionForStaticMemberAccess(sourceEnumeration);
        }

        private static INamedTypeSymbol GetSourceEnumType(SemanticModel semanticModel, SyntaxNode linkedEnumeration, CancellationToken cancellationToken)
        {
            var linkedEnumerationAttribute = semanticModel.Compilation.GetTypeByMetadataName("Roslyn.Utilities.LinkedEnumerationAttribute");
            var attributeUsageAttribute = WellKnownTypes.AttributeUsageAttribute(semanticModel.Compilation);
            var symbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(linkedEnumeration, cancellationToken);
            var namedTypeAttributes = symbol.GetApplicableAttributes(attributeUsageAttribute);
            var sourceEnumeration = LinkedEnumerationAnalyzer.TryGetLinkedEnumerationType(namedTypeAttributes, linkedEnumerationAttribute);
            return sourceEnumeration;
        }

        private static SyntaxNode TryGetContainingDeclarationOfKind(SyntaxGenerator generator, SyntaxNode syntaxNode, DeclarationKind kind)
        {
            if (syntaxNode is null)
            {
                return null;
            }

            var declaration = syntaxNode;
            var declarationKind = generator.GetDeclarationKind(declaration);
            while (declarationKind != kind)
            {
                declaration = generator.GetDeclaration(declaration.Parent);
                if (declaration is null)
                {
                    return null;
                }

                declarationKind = generator.GetDeclarationKind(declaration);
            }

            return declaration;
        }
    }
}
