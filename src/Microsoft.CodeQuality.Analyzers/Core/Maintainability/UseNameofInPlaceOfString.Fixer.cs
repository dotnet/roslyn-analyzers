// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;


namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    // TODO summary
    /// <summary>
    /// 
    /// </summary>
   // [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public abstract class UseNameOfInPlaceOfStringFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseNameofInPlaceOfStringAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers'
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {

            var root = await context.Document.GetSyntaxRootAsync(CancellationToken.None);
            var diagnostics = context.Diagnostics;
            TextSpan diagnosticSpan = diagnostics.First().Location.SourceSpan;
            SyntaxNode nodeToReplace = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            if (nodeToReplace != null)
            {
                if (!diagnostics.Single().Properties.TryGetValue("case", out var stringText))
                {
                    return;
                };
                context.RegisterCodeFix(
                    CodeAction.Create("Use NameOf", c => ReplaceWithNameOf(context.Document, nodeToReplace, stringText, c), equivalenceKey: nameof(UseNameOfInPlaceOfStringFixer)),
                    context.Diagnostics);
            }
        }

        private async Task<Document> ReplaceWithNameOf(Document document, SyntaxNode nodeToReplace, string stringText, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;

            var nameOfExpression = GetNameOfExpression(stringText);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(nodeToReplace, nameOfExpression);

            return document.WithSyntaxRoot(newRoot);
        }

        internal abstract SyntaxNode GetNameOfExpression(string stringText);
    }
}