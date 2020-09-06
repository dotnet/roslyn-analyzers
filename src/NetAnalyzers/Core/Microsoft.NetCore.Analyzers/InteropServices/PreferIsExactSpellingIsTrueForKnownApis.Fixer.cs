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

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    /// <summary>
    /// CA1839: Prefer ExactSpelling=true for known Apis fixer
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = PreferIsExactSpellingIsTrueForKnownApisAnalyzer.RuleId), Shared]
    public sealed class PreferIsExactSpellingIsTrueForKnownApisFixer : CodeFixProvider
    {
        private const string ExactSpellingText = "ExactSpelling";
        private const string EntryPointText = "EntryPoint";
        internal const string RuleId = "CA1839";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferIsExactSpellingIsTrueForKnownApisAnalyzer.RuleId);

        private static SyntaxNode FindNamedArgument(IReadOnlyList<SyntaxNode> arguments, string argumentName)
        {
            return arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(arg => arg.NameEquals != null && arg.NameEquals.Name.Identifier.Text == argumentName);
        }

        private static string GetEntryPointName(IMethodSymbol methodSymbol, AttributeData dllImportAttribute)
        {
            var hasEntryPointParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("EntryPoint", StringComparison.Ordinal));
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
            var dllName = dllImportAttribute.ConstructorArguments.First().Value.ToString();
            string title = MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisTitle;

            var editor = await DocumentEditor.CreateAsync(context.Document, context.CancellationToken).ConfigureAwait(false);
            if (PreferIsExactSpellingIsTrueForKnownApisAnalyzer.KnownApis.Value.TryGetValue(dllName, out var methods))
            {
                var actualName = GetEntryPointName(methodSymbol, dllImportAttribute);
                if (methods.Contains(actualName))
                {
                    if (actualName.EndsWith("W", StringComparison.OrdinalIgnoreCase))
                    {
                        context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => { await AddExactSpelling(context.Document, methodDeclaration, editor, ct).ConfigureAwait(false); return editor.GetChangedDocument(); },
                                                         equivalenceKey: title),
                                        context.Diagnostics);
                        return;
                    }
                    if (actualName.EndsWith("A", StringComparison.OrdinalIgnoreCase))
                    {
                        context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => { await AddExactSpelling(context.Document, methodDeclaration, editor, ct).ConfigureAwait(false); return editor.GetChangedDocument(); },
                                                         equivalenceKey: title),
                                        context.Diagnostics);
                        return;
                    }
                }
                if (methods.Contains(actualName + "A"))
                {
                    context.RegisterCodeFix(new MyCodeAction(title,
                                                         async ct => { await AddASuffix(context.Document, methodDeclaration, actualName, editor, ct).ConfigureAwait(false); return editor.GetChangedDocument(); },
                                                         equivalenceKey: title),
                                        context.Diagnostics);
                }
            }
        }

        public static async Task AddExactSpelling(Document document, MethodDeclarationSyntax methodDeclaration, DocumentEditor editor, CancellationToken cancellationToken)
        {
            SyntaxGenerator generator = editor.Generator;
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var dllImportSyntax = methodDeclaration.AttributeLists.First(x => x.Attributes.Any(y => y.Name.ToString().Equals("DllImport", StringComparison.Ordinal)));
            IReadOnlyList<SyntaxNode> arguments = generator.GetAttributeArguments(dllImportSyntax);

            // [DllImport] attribute -> add or replace ExactSpelling parameter
            SyntaxNode argumentValue = generator.TrueLiteralExpression();
            SyntaxNode newExactSpellingArgument = generator.AttributeArgument(ExactSpellingText, argumentValue);
            SyntaxNode exactSpellingArgument = FindNamedArgument(arguments, ExactSpellingText);
            if (exactSpellingArgument == null)
            {
                editor.AddAttributeArgument(dllImportSyntax, newExactSpellingArgument);
            }
            else
            {
                editor.ReplaceNode(exactSpellingArgument, newExactSpellingArgument);
            }
        }

        public static async Task AddASuffix(Document document, MethodDeclarationSyntax methodDeclaration, string actualExternalName, DocumentEditor editor, CancellationToken cancellationToken)
        {
            await AddExactSpelling(document, methodDeclaration, editor, cancellationToken).ConfigureAwait(false);
            SyntaxGenerator generator = editor.Generator;
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var dllImportSyntax = methodDeclaration.AttributeLists.First(x => x.Attributes.Any(y => y.Name.ToString().Equals("DllImport", StringComparison.Ordinal)));
            IReadOnlyList<SyntaxNode> arguments = generator.GetAttributeArguments(dllImportSyntax);

            // [DllImport] attribute -> add or replace ExactSpelling parameter
            SyntaxNode argumentValue = generator.LiteralExpression(actualExternalName + "A");
            SyntaxNode newExactSpellingArgument = generator.AttributeArgument(EntryPointText, argumentValue);

            SyntaxNode entryPointNode = FindNamedArgument(arguments, EntryPointText);
            if (entryPointNode == null)
            {
                editor.AddAttributeArgument(dllImportSyntax, newExactSpellingArgument);
            }
            else
            {
                editor.ReplaceNode(entryPointNode, newExactSpellingArgument);
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