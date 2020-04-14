// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    public abstract class InstantiateArgumentExceptionsCorrectlyFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(InstantiateArgumentExceptionsCorrectlyAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            string messagePositionString = diagnostic.Properties.GetValueOrDefault(InstantiateArgumentExceptionsCorrectlyAnalyzer.MessagePosition);
            if (messagePositionString != null)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                SyntaxNode node = root.FindNode(context.Span, getInnermostNodeForTie: true);
                if (node is ObjectCreationExpressionSyntax creation)
                {
                    int messagePosition = int.Parse(messagePositionString, CultureInfo.InvariantCulture);
                    switch (creation.ArgumentList.Arguments.Count)
                    {
                        case 1: // Add null message
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    title: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyChangeToTwoArgument,
                                    createChangedDocument: c => AddNullMessageToArgumentList(context.Document, creation, c),
                                    equivalenceKey: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyChangeToTwoArgument),
                                diagnostic);
                            break;
                        case 2: // Swap message and paramete name
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    title: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyFlipArgumentOrder,
                                    createChangedDocument: c => SwapArgumentsOrder(context.Document, creation, messagePosition, c),
                                    equivalenceKey: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyFlipArgumentOrder),
                                diagnostic);
                            break;
                        case 3: // 3 parameter, message can be 1st or 3rd parameter
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    title: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyFlipArgumentOrder,
                                    createChangedDocument: c => SwapArgumentsOrderThreeParameters(context.Document, creation, messagePosition, c),
                                    equivalenceKey: MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyFlipArgumentOrder),
                                diagnostic);
                            break;

                    }
                }
            }
        }

        private static async Task<Document> SwapArgumentsOrder(Document document, ObjectCreationExpressionSyntax creation, int messagePosition, CancellationToken token)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);
            SyntaxNode parameter = AddNameOfIfLiteral(creation.ArgumentList.Arguments[messagePosition].Expression, editor.Generator);
            SyntaxNode newCreation;
            if (messagePosition == 0)
            {
                newCreation = editor.Generator.ObjectCreationExpression(creation.Type, creation.ArgumentList.Arguments[1], parameter);
            }
            else
            {
                newCreation = editor.Generator.ObjectCreationExpression(creation.Type, parameter, creation.ArgumentList.Arguments[0]);
            }
            editor.ReplaceNode(creation, newCreation);
            return editor.GetChangedDocument();
        }

        private static async Task<Document> SwapArgumentsOrderThreeParameters(Document document, ObjectCreationExpressionSyntax creation, int messagePosition, CancellationToken token)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);
            SyntaxNode parameter = AddNameOfIfLiteral(creation.ArgumentList.Arguments[messagePosition].Expression, editor.Generator);
            SyntaxNode newCreation;
            if (messagePosition == 0)
            {
                newCreation = editor.Generator.ObjectCreationExpression(creation.Type, creation.ArgumentList.Arguments[1], parameter, creation.ArgumentList.Arguments[2]);
            }
            else
            {
                newCreation = editor.Generator.ObjectCreationExpression(creation.Type, parameter, creation.ArgumentList.Arguments[1], creation.ArgumentList.Arguments[0]);
            }
            editor.ReplaceNode(creation, newCreation);
            return editor.GetChangedDocument();
        }

        private static async Task<Document> AddNullMessageToArgumentList(Document document, ObjectCreationExpressionSyntax creation, CancellationToken token)
        {
            DocumentEditor editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);
            SyntaxNode argument = AddNameOfIfLiteral(creation.ArgumentList.Arguments.First().Expression, editor.Generator);
            SyntaxNode newCreation = editor.Generator.ObjectCreationExpression(
                creation.Type,
                editor.Generator.Argument(editor.Generator.NullLiteralExpression()),
                argument);
            editor.ReplaceNode(creation, newCreation);
            return editor.GetChangedDocument();
        }

        private static SyntaxNode AddNameOfIfLiteral(ExpressionSyntax expression, SyntaxGenerator generator)
        {
            if (expression is LiteralExpressionSyntax literal)
            {
                return GetNameOfExpression(generator, literal.Token.ValueText);
            }
            return expression;
        }

        private static SyntaxNode GetNameOfExpression(SyntaxGenerator generator, string identifierNameArgument) =>
            generator.NameOfExpression(generator.IdentifierName(identifierNameArgument));
    }
}