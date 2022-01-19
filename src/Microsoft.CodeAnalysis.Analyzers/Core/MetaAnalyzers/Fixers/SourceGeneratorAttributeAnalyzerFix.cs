// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(SourceGeneratorAttributeAnalyzerFix))]
    [Shared]
    public sealed class SourceGeneratorAttributeAnalyzerFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            DiagnosticIds.MissingSourceGeneratorAttributeId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            if (node is null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                AddFix(
                    string.Format(CodeAnalysisDiagnosticsResources.AddGeneratorAttribute_1, LanguageNames.CSharp),
                    context, document, node, diagnostic, LanguageNames.CSharp);

                AddFix(
                    string.Format(CodeAnalysisDiagnosticsResources.AddGeneratorAttribute_1, LanguageNames.VisualBasic),
                    context, document, node, diagnostic, LanguageNames.VisualBasic);

                AddFix(
                    string.Format(CodeAnalysisDiagnosticsResources.AddGeneratorAttribute_2, LanguageNames.CSharp, LanguageNames.VisualBasic),
                    context, document, node, diagnostic, LanguageNames.CSharp, LanguageNames.VisualBasic);
            }
        }

        private static void AddFix(string title, CodeFixContext context, Document document, SyntaxNode node, Diagnostic diagnostic, params string[] languageNames)
        {
            var codeAction = CodeAction.Create(
                title,
                (cancellationToken) => FixDocumentAsync(document, node, languageNames, cancellationToken),
                equivalenceKey: nameof(SourceGeneratorAttributeAnalyzerFix));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        public override FixAllProvider? GetFixAllProvider() => null;

        private static async Task<Document> FixDocumentAsync(Document document, SyntaxNode node, string[] languageNames, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;

            SyntaxNode? generatorAttribute;

            if (languageNames.Length == 1 && languageNames[0] == LanguageNames.CSharp)
            {
                // CSharp is the only language, which is the default paramterless 
                // constructor for the Generator attribute
                generatorAttribute = generator.Attribute(WellKnownTypeNames.MicrosoftCodeAnalysisGeneratorAttribute).WithAddImportsAnnotation();
            }
            else
            {
                // For cases where the language is VB or VB and CSharp, add 
                // an argument to signify that
                var languageNamesFullName = typeof(LanguageNames).FullName;
                var splitLanguageNames = languageNamesFullName.Split('.');

                var baseLanguageNameExpression = generator.IdentifierName(splitLanguageNames[0]);
                foreach (var identifier in splitLanguageNames.Skip(1))
                {
                    baseLanguageNameExpression = generator.MemberAccessExpression(baseLanguageNameExpression, identifier);
                }

                var arguments = new SyntaxNode[languageNames.Length];
                for (var i = 0; i < languageNames.Length; i++)
                {
                    var language = languageNames[i] switch
                    {
                        LanguageNames.CSharp => nameof(LanguageNames.CSharp),
                        LanguageNames.VisualBasic => nameof(LanguageNames.VisualBasic),
                        _ => throw new InvalidOperationException()
                    };

                    var finalExpression = generator.MemberAccessExpression(baseLanguageNameExpression, language);
                    arguments[i] = finalExpression;
                }

                generatorAttribute = generator.Attribute(WellKnownTypeNames.MicrosoftCodeAnalysisGeneratorAttribute, arguments).WithAddImportsAnnotation();
            }

            editor.ReplaceNode(node, generator.AddAttributes(node, generatorAttribute));

            return editor.GetChangedDocument();
        }
    }
}
