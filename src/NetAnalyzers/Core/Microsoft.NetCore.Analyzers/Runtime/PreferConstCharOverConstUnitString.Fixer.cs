// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    // TODO: Add VisualBasic to this too?
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class PreferConstCharOverConstUnitStringFixer : CodeFixProvider
    {
        private const string title = "Replace const unit string with const char";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferConstCharOverConstUnitStringAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostics = context.Diagnostics;
            var diagnosticsSpan = diagnostics.First().Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declarations = root.FindToken(diagnosticsSpan.Start).Parent.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>();
            LocalDeclarationStatementSyntax declaration = declarations.First();
            VariableDeclarationSyntax variableDeclaration = declaration.Declaration;

            // Bail out if there are multiple variable declarations
            if (variableDeclaration.Variables.Count == 1)
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => ConvertStringToChar(context.Document, variableDeclaration, c),
                        equivalenceKey: title),
                    diagnostics);
            }
        }

        private static async Task<Document?> ConvertStringToChar(Document document, VariableDeclarationSyntax variableDeclaration, CancellationToken cancellationToken)
        {
            IEnumerable<SyntaxNode> childNodes = variableDeclaration.ChildNodes();
            if (!(childNodes.First() is PredefinedTypeSyntax predefinedStringType))
            {
                return null;
            }

            // Replace the string type with char type
            SyntaxToken charToken = SyntaxFactory.Token(SyntaxKind.CharKeyword);
            PredefinedTypeSyntax predefinedCharTypeWithTrailingTrivia = SyntaxFactory.PredefinedType(charToken).WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
            VariableDeclarationSyntax newCharVariableDeclaration = variableDeclaration.ReplaceNode(predefinedStringType, predefinedCharTypeWithTrailingTrivia);

            SeparatedSyntaxList<VariableDeclaratorSyntax> variables = newCharVariableDeclaration.Variables;

            // Replace the string value with a char value
            VariableDeclaratorSyntax variable = variables.First();
            string? stringValue = (variable.Initializer.Value as LiteralExpressionSyntax)?.Token.ValueText;
            if (stringValue == null)
            {
                return null;
            }

            char charValue = stringValue[0];
            EqualsValueClauseSyntax equalsValueClause = variable.Initializer;
            ExpressionSyntax? stringLiteralExpression = equalsValueClause.Value;
            LiteralExpressionSyntax? charLiteralExpression = SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(charValue));
            newCharVariableDeclaration = newCharVariableDeclaration.ReplaceNode(stringLiteralExpression, charLiteralExpression);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(variableDeclaration, newCharVariableDeclaration);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
