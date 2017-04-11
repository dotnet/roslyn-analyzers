﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
    /// CA1012: Abstract classes should not have public constructors
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class AbstractTypesShouldNotHaveConstructorsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AbstractTypesShouldNotHaveConstructorsAnalyzer.RuleId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span);

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(new DocumentChangeAction(MicrosoftApiDesignGuidelinesAnalyzersResources.AbstractTypesShouldNotHavePublicConstructorsCodeFix,
                                        async ct => await ChangeAccessibilityCodeFix(context.Document, root, node, ct).ConfigureAwait(false)),
                                    diagnostic);
        }

        private static SyntaxNode GetDeclaration(ISymbol symbol)
        {
            return (symbol.DeclaringSyntaxReferences.Length > 0) ? symbol.DeclaringSyntaxReferences[0].GetSyntax() : null;
        }

        private async Task<Document> ChangeAccessibilityCodeFix(Document document, SyntaxNode root, SyntaxNode nodeToFix, CancellationToken cancellationToken)
        {
            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var classSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(nodeToFix, cancellationToken);
            System.Collections.Generic.List<SyntaxNode> instanceConstructors = classSymbol.InstanceConstructors.Where(t => t.DeclaredAccessibility == Accessibility.Public).Select(t => GetDeclaration(t)).Where(d => d != null).ToList();
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
            SyntaxNode newRoot = root.ReplaceNodes(instanceConstructors, (original, rewritten) => generator.WithAccessibility(original, Accessibility.Protected));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
