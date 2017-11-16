// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// NEEDCODE
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class UseNameOfInPlaceOfStringFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseNameofInPlaceOfStringAnalyzer<SyntaxKind>.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers'
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(CancellationToken.None);

            var literalExpression = root.FindNode(context.Span, getInnermostNodeForTie: true) as LiteralExpressionSyntax;
            if (literalExpression != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create("Use NameOf", c => ReplaceWithNameOf(context.Document, literalExpression, c), equivalenceKey: nameof(UseNameOfInPlaceOfStringFixer)),
                    context.Diagnostics);
            }
        }

        private async Task<Document> ReplaceWithNameOf(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken)
        {
            var stringText = literalExpression.Token.ValueText;

            var nameOfExpression = InvocationExpression(
                expression: IdentifierName("nameof"),
                argumentList: ArgumentList(
                    arguments: SingletonSeparatedList(Argument(IdentifierName(stringText)))));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(literalExpression, nameOfExpression);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}