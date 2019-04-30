// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Diagnostics.Analyzers
{
    public abstract class AbstractMainThreadDependencyShouldBeVerifiedCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RoslynDiagnosticIds.MainThreadDependencyShouldBeVerifiedRuleId);

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        protected abstract bool IsAttributeArgumentNamedVerified(SyntaxNode attributeArgument);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        RoslynDiagnosticsAnalyzersResources.MainThreadDependencyShouldBeVerifiedFix,
                        cancellationToken => VerifyMainThreadDependencyAsync(context.Document, diagnostic.Location.SourceSpan, cancellationToken),
                        equivalenceKey: nameof(AbstractMainThreadDependencyShouldBeVerifiedCodeFix)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> VerifyMainThreadDependencyAsync(Document document, TextSpan sourceSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var noMainThreadDependencyAttributeApplication = root.FindNode(sourceSpan, getInnermostNodeForTie: true);

            var generator = SyntaxGenerator.GetGenerator(document);

            var declaration = noMainThreadDependencyAttributeApplication;
            var declarationKind = generator.GetDeclarationKind(declaration);
            while (declarationKind != DeclarationKind.Attribute)
            {
                declaration = generator.GetDeclaration(declaration.Parent);
                if (declaration is null)
                {
                    return document;
                }

                declarationKind = generator.GetDeclarationKind(declaration);
            }

            foreach (var attributeArgument in generator.GetAttributeArguments(declaration))
            {
                if (IsAttributeArgumentNamedVerified(attributeArgument))
                {
                    return document.WithSyntaxRoot(generator.RemoveNode(root, attributeArgument));
                }
            }

            return document;
        }
    }
}
