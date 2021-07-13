// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;
using static Microsoft.NetCore.Analyzers.Runtime.ConstructorParametersShouldMatchPropertyAndFieldNamesAnalyzer;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1071: Constructor parameters should match property and field names.
    /// Based on <see cref="ParameterNamesShouldMatchBaseDeclarationFixer"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class ConstructorParametersShouldMatchPropertyAndFieldNamesFixer : CodeFixProvider
    {
        private static readonly Type DiagnosticReasonEnumType = typeof(ParameterDiagnosticReason);

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                SyntaxNode node = syntaxRoot.FindNode(context.Span);
                ISymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node, context.CancellationToken);

                if (declaredSymbol.Kind != SymbolKind.Parameter)
                {
                    continue;
                }

                var diagnosticReason = (ParameterDiagnosticReason)Enum.Parse(DiagnosticReasonEnumType, diagnostic.Properties[DiagnosticReasonKey]);

                switch (diagnosticReason)
                {
                    case ParameterDiagnosticReason.NameMismatch:
                        RegisterParameterRenameCodeFix(context, diagnostic, declaredSymbol);
                        break;
                    case ParameterDiagnosticReason.FieldInappropriateVisibility:
                    case ParameterDiagnosticReason.PropertyInappropriateVisibility:
                        SyntaxNode fieldOrProperty = syntaxRoot.FindNode(diagnostic.AdditionalLocations[0].SourceSpan);
                        RegisterMakeFieldOrPropertyPublicCodeFix(context, diagnostic, fieldOrProperty);
                        break;
                    default:
                        throw new InvalidOperationException();
                };
            }
        }

        private static void RegisterParameterRenameCodeFix(CodeFixContext context, Diagnostic diagnostic, ISymbol declaredSymbol)
        {
            // This approach is very naive. Most likely we want to support NamingStyleOptions, available in Roslyn.
            string newName = LowerFirstLetter(diagnostic.Properties[ReferencedFieldOrPropertyNameKey]);

            context.RegisterCodeFix(
                CodeAction.Create(
                    string.Format(CultureInfo.CurrentCulture, MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyOrFieldNamesTitle, newName),
                    cancellationToken => GetUpdatedDocumentForParameterRenameAsync(context.Document, declaredSymbol, newName, cancellationToken),
                    nameof(ConstructorParametersShouldMatchPropertyAndFieldNamesFixer)),
                diagnostic);
        }

        private static void RegisterMakeFieldOrPropertyPublicCodeFix(CodeFixContext context, Diagnostic diagnostic, SyntaxNode fieldOrProperty)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    string.Format(CultureInfo.CurrentCulture, MicrosoftNetCoreAnalyzersResources.ConstructorParametersShouldMatchPropertyOrFieldNamesTitle, fieldOrProperty),
                    cancellationToken => GetUpdatedDocumentForMakingFieldOrPropertyPublicAsync(context.Document, fieldOrProperty, cancellationToken),
                    nameof(ConstructorParametersShouldMatchPropertyAndFieldNamesFixer)),
                diagnostic);
        }

        private static string LowerFirstLetter(string targetName)
        {
            return $"{targetName[0].ToString().ToLower(CultureInfo.CurrentCulture)}{targetName[1..]}";
        }

        private static async Task<Document> GetUpdatedDocumentForParameterRenameAsync(Document document, ISymbol parameter, string newName, CancellationToken cancellationToken)
        {
            Solution newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, parameter, newName, null, cancellationToken).ConfigureAwait(false);
            return newSolution.GetDocument(document.Id)!;
        }

        private static async Task<Document> GetUpdatedDocumentForMakingFieldOrPropertyPublicAsync(Document document, SyntaxNode fieldOrProperty, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            editor.SetAccessibility(fieldOrProperty, Accessibility.Public);

            return editor.GetChangedDocument();
        }
    }
}