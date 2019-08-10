// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.NetCore.Analyzers.Performance

{
    /// <summary>
    /// CA1829: Use property instead of <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>, when available.
    /// Implements the <see cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    /// </summary>
    public abstract class UsePropertyInsteadOfCountMethodWhenAvailableFixer : CodeFixProvider
    {
        /// <summary>
        /// A list of diagnostic IDs that this provider can provider fixes for.
        /// </summary>
        /// <value>The fixable diagnostic ids.</value>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId);


        /// <summary>
        /// Gets an optional <see cref="FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
        /// Return null if the provider doesn't support fix all/multiple occurrences.
        /// Otherwise, you can return any of the well known fix all providers from <see cref="WellKnownFixAllProviders" /> or implement your own fix all provider.
        /// </summary>
        /// <returns>FixAllProvider.</returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Computes one or more fixes for the specified <see cref="CodeFixContext" />.
        /// </summary>
        /// <param name="context">A <see cref="CodeFixContext" /> containing context information about the diagnostics to fix.
        /// The context must only contain diagnostics with a <see cref="Diagnostic.Id" /> included in the <see cref="CodeFixProvider.FixableDiagnosticIds" /> 
        /// for the current provider.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous operation.</returns>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var propertyName = context.Diagnostics[0].Properties["PropertyName"];

            if (node is object && propertyName is object && GetExpression(node) is SyntaxNode expression)
            {
                context.RegisterCodeFix(
                    new UsePropertyInsteadOfCountMethodWhenAvailableCodeAction(context.Document, node, expression, propertyName),
                    context.Diagnostics);
            }
        }

        /// <summary>
        /// Gets the expression from the specified <paramref name="node"/> where to replace the invocation of the
        /// <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> method with a property invocation.
        /// </summary>
        /// <param name="node">The node to get a fixer for.</param>
        /// <returns>The expression from the specified <paramref name="node"/> where to replace the invocation of the
        /// <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> method with a property invocation
        /// if found; <see langword="null" /> otherwise.</returns>
        protected abstract SyntaxNode GetExpression(SyntaxNode node);

        private class UsePropertyInsteadOfCountMethodWhenAvailableCodeAction : CodeAction
        {
            private readonly Document document;
            private readonly SyntaxNode node;
            private readonly SyntaxNode expression;
            private readonly string propertyName;

            public UsePropertyInsteadOfCountMethodWhenAvailableCodeAction(
                Document document,
                SyntaxNode node,
                SyntaxNode expression,
                string propertyName)
            {
                this.document = document;
                this.node = node;
                this.expression = expression;
                this.propertyName = propertyName;
            }

            public override string Title { get; } = MicrosoftNetCoreAnalyzersResources.UsePropertyInsteadOfCountMethodWhenAvailableTitle;

            public override string EquivalenceKey { get; } = MicrosoftNetCoreAnalyzersResources.UsePropertyInsteadOfCountMethodWhenAvailableTitle;

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var editor = await DocumentEditor.CreateAsync(this.document, cancellationToken).ConfigureAwait(false);
                var generator = editor.Generator;
                var replacementSyntax = generator.MemberAccessExpression(this.expression.WithoutTrailingTrivia(), this.propertyName);

                replacementSyntax = replacementSyntax
                    .WithAdditionalAnnotations(Formatter.Annotation)
                    .WithTriviaFrom(this.node);

                editor.ReplaceNode(this.node, replacementSyntax);

                return editor.GetChangedDocument();
            }
        }
    }
}
