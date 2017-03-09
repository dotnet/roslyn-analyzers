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

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA1822: Mark members as static
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class MarkMembersAsStaticFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MarkMembersAsStaticAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var symbol = model.GetDeclaredSymbol(node, context.CancellationToken);

            context.RegisterCodeFix(new MarkMembersAsStaticAction(
                    string.Format(MicrosoftQualityGuidelinesAnalyzersResources.MarkMembersAsStaticCodeFix, symbol.Name),
                    async ct => await RemoveStaticModifier(context.Document, root, node, ct).ConfigureAwait(false)
                ), context.Diagnostics.Single());
        }

        private Task<Document> RemoveStaticModifier(Document document, SyntaxNode root, SyntaxNode node, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
            var madeStatic = syntaxGenerator.WithModifiers(node, DeclarationModifiers.Static);
            SyntaxNode newRoot = root.ReplaceNode(node, madeStatic);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        private class MarkMembersAsStaticAction : DocumentChangeAction
        {
            public MarkMembersAsStaticAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument) : base(title, createChangedDocument)
            {
            }

            public override string EquivalenceKey => Title;
        }
    }
}