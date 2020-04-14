// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Globalization;
using Microsoft.NetCore.Analyzers.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Threading;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.NetCore.Analyzers;
using System.Threading.Tasks;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpInstantiateArgumentExceptionsCorrectlyFixer : InstantiateArgumentExceptionsCorrectlyFixer
    {
        protected override void PopulateCodeFix(CodeFixContext context, Diagnostic diagnostic, string paramPositionString, SyntaxNode node)
        {
            if (node is ObjectCreationExpressionSyntax creation)
            {
                int paramPosition = int.Parse(paramPositionString, CultureInfo.InvariantCulture);
                CodeAction? codeAction = null;
                if (creation.ArgumentList.Arguments.Count == 1)
                { // Add null message
                    codeAction = CodeAction.Create(
                        title: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyChangeToTwoArgument,
                        createChangedDocument: c => AddNullMessageToArgumentList(context.Document, creation, c),
                        equivalenceKey: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyChangeToTwoArgument);
                }
                else
                { // Swap message and paramete name
                    codeAction = CodeAction.Create(
                        title: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyFlipArgumentOrder,
                        createChangedDocument: c => SwapArgumentsOrder(context.Document, creation, paramPosition, creation.ArgumentList.Arguments.Count, c),
                        equivalenceKey: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyFlipArgumentOrder);
                }
                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static async Task<Document> SwapArgumentsOrder(Document document, ObjectCreationExpressionSyntax creation, int paramPosition, int argumentCount, CancellationToken token)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);
            SyntaxNode parameter = AddNameOfIfLiteral(creation.ArgumentList.Arguments[paramPosition].Expression, editor.Generator);
            SyntaxNode newCreation;
            if (argumentCount == 2)
            {
                if (paramPosition == 0)
                {
                    newCreation = editor.Generator.ObjectCreationExpression(creation.Type, creation.ArgumentList.Arguments[1], parameter);
                }
                else
                {
                    newCreation = editor.Generator.ObjectCreationExpression(creation.Type, parameter, creation.ArgumentList.Arguments[0]);
                }
            }
            else // 3 arguments
            {
                if (paramPosition == 0)
                {
                    newCreation = editor.Generator.ObjectCreationExpression(creation.Type, creation.ArgumentList.Arguments[1], parameter, creation.ArgumentList.Arguments[2]);
                }
                else
                {
                    newCreation = editor.Generator.ObjectCreationExpression(creation.Type, parameter, creation.ArgumentList.Arguments[1], creation.ArgumentList.Arguments[0]);
                }
            }
            editor.ReplaceNode(creation, newCreation);
            return editor.GetChangedDocument();
        }

        private static async Task<Document> AddNullMessageToArgumentList(Document document, ObjectCreationExpressionSyntax creation, CancellationToken token)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);
            SyntaxNode argument = AddNameOfIfLiteral(creation.ArgumentList.Arguments.First().Expression, editor.Generator);
            SyntaxNode newCreation = editor.Generator.ObjectCreationExpression(creation.Type, editor.Generator.Argument(editor.Generator.NullLiteralExpression()), argument);
            editor.ReplaceNode(creation, newCreation);
            return editor.GetChangedDocument();
        }

        private static SyntaxNode AddNameOfIfLiteral(ExpressionSyntax expression, SyntaxGenerator generator)
        {
            if (expression is LiteralExpressionSyntax literal)
            {
                return generator.NameOfExpression(generator.IdentifierName(literal.Token.ValueText));
            }
            return expression;
        }
    }
}