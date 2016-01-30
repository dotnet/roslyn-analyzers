// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Analyzer.Utilities;
using System.Composition;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1032: Implement standard exception constructors
    /// Cause: A type extends System.Exception and does not declare all the required constructors. 
    /// Description: Exception types must implement the following constructors. Failure to provide the full set of constructors can make it difficult to correctly handle exceptions
    /// For CSharp, all possible  missing Constructors would be 
    ///     public GoodException()
    ///     public GoodException(string)
    ///     public GoodException(string, Exception)
    /// For Basic, all possible  missing Constructors would be
    ///     Sub New()
    ///     Sub New(message As String)
    ///     Sub New(message As String, innerException As Exception)
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class ImplementStandardExceptionConstructorsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ImplementStandardExceptionConstructorsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // Provides for fix all occurrences within Document, Project, Solution
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // Get fix title string from resources
            var title = MicrosoftApiDesignGuidelinesAnalyzersResources.ImplementStandardExceptionConstructorsTitle;

            // Get syntax root node
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Register fixer - pass in the collection of diagnostics, since there could be more than one for this diagnostic
            context.RegisterCodeFix(CodeAction.Create(title, c => AddConstructorAsync(context.Document, context.Diagnostics, root, c), equivalenceKey: title), context.Diagnostics.First());
        }

        private async Task<Document> AddConstructorAsync(Document document, ImmutableArray<Diagnostic> diagnostics, SyntaxNode root, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var diagnosticSpan = diagnostics.First().Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan);
            var targetNode = editor.Generator.GetDeclaration(node, DeclarationKind.Class);
            var typeSymbol = model.GetDeclaredSymbol(targetNode) as INamedTypeSymbol;

            foreach (var diagnostic in diagnostics)
            {
                // Identify what is the signature of the missing constructor from diagnostic signature property that was filled in by the analyzer
                var missingCtorSignature = (ImplementStandardExceptionConstructorsAnalyzer.MissingCtorSignature)Enum.Parse(typeof(ImplementStandardExceptionConstructorsAnalyzer.MissingCtorSignature), diagnostic.Properties["Signature"]);

                switch (missingCtorSignature)
                {
                    case ImplementStandardExceptionConstructorsAnalyzer.MissingCtorSignature.CtorWithNoParameter:
                        // Add missing CtorWithNoParameter
                        var newConstructorNode1 = generator.ConstructorDeclaration(typeSymbol.Name, accessibility: Accessibility.Public);
                        editor.AddMember(targetNode, newConstructorNode1);
                        break;
                    case ImplementStandardExceptionConstructorsAnalyzer.MissingCtorSignature.CtorWithStringParameter:
                        // Add missing CtorWithStringParameter 
                        var newConstructorNode2 = generator.ConstructorDeclaration(
                                                    containingTypeName: typeSymbol.Name,
                                                    parameters: new[]
                                                    {
                                                    generator.ParameterDeclaration("message", generator.TypeExpression(WellKnownTypes.String(editor.SemanticModel.Compilation)))
                                                    },
                                                    accessibility: Accessibility.Public,
                                                    baseConstructorArguments: new[]
                                                    {
                                                    generator.Argument(generator.IdentifierName("message"))
                                                    });
                        editor.AddMember(targetNode, newConstructorNode2);
                        break;
                    case ImplementStandardExceptionConstructorsAnalyzer.MissingCtorSignature.CtorWithStringAndExceptionParameters:
                        // Add missing CtorWithStringAndExceptionParameters 
                        var newConstructorNode3 = generator.ConstructorDeclaration(
                                                    containingTypeName: typeSymbol.Name,
                                                    parameters: new[]
                                                    {
                                                    generator.ParameterDeclaration("message", generator.TypeExpression(WellKnownTypes.String(editor.SemanticModel.Compilation))),
                                                    generator.ParameterDeclaration("innerException", generator.TypeExpression(WellKnownTypes.Exception(editor.SemanticModel.Compilation)))
                                                    },
                                                    accessibility: Accessibility.Public,
                                                    baseConstructorArguments: new[]
                                                    {
                                                    generator.Argument(generator.IdentifierName("message")),
                                                    generator.Argument(generator.IdentifierName("innerException"))
                                                    });
                        editor.AddMember(targetNode, newConstructorNode3);
                        break;
                }
            }

            return editor.GetChangedDocument();
        }
    }
}