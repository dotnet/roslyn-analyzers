// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    /// <summary>
    /// CA1839: Prefer ExactSpelling=true for known Apis fixer
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = PreferIsExactSpellingIsTrueForKnownApisAnalyzer.RuleId), Shared]
    public sealed class PreferIsExactSpellingIsTrueForKnownApisFixer : CodeFixProvider
    {
        private const string ExactSpellingText = "ExactSpelling";
        internal const string RuleId = "CA1839";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferIsExactSpellingIsTrueForKnownApisAnalyzer.RuleId);

        private static SyntaxNode FindNamedArgument(IReadOnlyList<SyntaxNode> arguments, string argumentName)
        {
            return arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(arg => arg.NameEquals != null && arg.NameEquals.Name.Identifier.Text == argumentName);
        }

        private static string GetEntryPointName(IMethodSymbol methodSymbol, AttributeData dllImportAttribute)
        {

            var hasEntryPointParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("EntryPoint", StringComparison.Ordinal));
            var hasExactSpellingParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("ExactSpelling", StringComparison.Ordinal));

            return hasEntryPointParameter.Key is null ? methodSymbol.Name : hasEntryPointParameter.Value.Value.ToString();
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            INamedTypeSymbol? dllImportType = model.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesDllImportAttribute);
            if (dllImportType is null)
                return;
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)root.FindNode(context.Span);
            IMethodSymbol methodSymbol = (IMethodSymbol)model.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
            var dllImportAttribute = methodSymbol
                .GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.Name.Equals("DllImportAttribute", StringComparison.Ordinal));
            var dllName = dllImportAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
            string title = MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisTitle;
            if (PreferIsExactSpellingIsTrueForKnownApisAnalyzer.KnownApis.Value.TryGetValue(dllName, out var methods))
            {
                var actualName = GetEntryPointName(methodSymbol, dllImportAttribute);
                if (methods.Contains(actualName))
                {
                    if (methods.Contains(actualName + "W"))
                    {
                        context.RegisterCodeFix(new MySolutionCodeAction(title,
                                                         async ct => await AddWSuffix(context.Document, methodDeclaration, methodSymbol, dllImportType, ct).ConfigureAwait(false),
                                                         equivalenceKey: title),
                                        context.Diagnostics);
                    }
                    else
                    {
                        context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => await AddExactSpelling(context.Document, methodDeclaration, methodSymbol, dllImportType, ct).ConfigureAwait(false),
                                                         equivalenceKey: title),
                                        context.Diagnostics);
                    }
                }
            }
        }

        public static async Task<Document> AddExactSpelling(Document document, MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol, INamedTypeSymbol dllImport, CancellationToken cancellationToken)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            // could be either a [DllImport] or [MarshalAs] attribute

            if (!methodSymbol.GetAttributes().Any(x => x.AttributeClass.Equals(dllImport)))
                return document;
            var dllImportSyntax = methodDeclaration.AttributeLists.First(x => x.Attributes.Any(y => y.Name.ToString().Equals("DllImport", StringComparison.Ordinal)));
            IReadOnlyList<SyntaxNode> arguments = generator.GetAttributeArguments(dllImportSyntax);

            // [DllImport] attribute, add or replace ExactSpelling parameter
            SyntaxNode argumentValue = generator.TrueLiteralExpression();
            SyntaxNode newCharSetArgument = generator.AttributeArgument(ExactSpellingText, argumentValue);

            SyntaxNode exactSpellingArgument = FindNamedArgument(arguments, ExactSpellingText);
            if (exactSpellingArgument == null)
            {
                editor.AddAttributeArgument(dllImportSyntax, newCharSetArgument);
            }
            else
            {
                editor.ReplaceNode(exactSpellingArgument, newCharSetArgument);
            }

            return editor.GetChangedDocument();
        }

        public static async Task<Solution> AddWSuffix(Document document, MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol, INamedTypeSymbol dllImport, CancellationToken cancellationToken)
        {
            document = await AddExactSpelling(document, methodDeclaration, methodSymbol, dllImport, cancellationToken).ConfigureAwait(false);

            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, methodSymbol, methodSymbol.Name + "W", document.Project.Solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
            return newSolution;
        }

        private class MySolutionCodeAction : SolutionChangeAction
        {
            public MySolutionCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}