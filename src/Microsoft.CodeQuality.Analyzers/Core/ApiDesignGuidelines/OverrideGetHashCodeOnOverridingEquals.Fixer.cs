// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA2218: Override GetHashCode on overriding Equals
    /// </summary>
    public abstract class OverrideGetHashCodeOnOverridingEqualsFixer : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxNode typeDeclaration = root.FindNode(context.Span);
            typeDeclaration = SyntaxGenerator.GetGenerator(context.Document).GetDeclaration(typeDeclaration);
            if (typeDeclaration == null)
            {
                return;
            }

            // CONSIDER: Do we need to confirm that System.Object.GetHashCode isn't shadowed in a base type?

            // We cannot have multiple overlapping diagnostics of this id.
            Diagnostic diagnostic = context.Diagnostics.Single();

            context.RegisterCodeFix(
                new MyCodeAction(
                    MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideGetHashCodeOnOverridingEqualsCodeActionTitle,
                    cancellationToken => OverrideObjectGetHashCode(context.Document, typeDeclaration, cancellationToken)),
                diagnostic);
        }

        private async Task<Document> OverrideObjectGetHashCode(Document document, SyntaxNode typeDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;

            var methodDeclaration = generator.DefaultGetHashCodeOverrideDeclaration(editor.SemanticModel.Compilation);

            editor.AddMember(typeDeclaration, methodDeclaration);
            return editor.GetChangedDocument();
        }

        /// <remarks>
        /// This type exists for telemetry purposes - it has the same functionality as 
        /// <see cref="DocumentChangeAction"/> but different metadata.
        /// </remarks>
        private sealed class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument)
            {
            }
        }
    }
}