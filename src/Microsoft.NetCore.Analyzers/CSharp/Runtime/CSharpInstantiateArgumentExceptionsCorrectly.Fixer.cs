// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.NetCore.Analyzers.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpInstantiateArgumentExceptionsCorrectlyFixer : InstantiateArgumentExceptionsCorrectlyFixer
    {
        protected override SyntaxNode GetNameOfExpression(SyntaxGenerator generator, string identifierNameArgument)
        {
            // Workaround for https://github.com/dotnet/roslyn/issues/24212
            string nameofString = SyntaxFacts.GetText(SyntaxKind.NameOfKeyword);
            SyntaxToken nameofIdentifierToken = SyntaxFactory.Identifier(leading: default(SyntaxTriviaList), contextualKind: SyntaxKind.NameOfKeyword,
                text: nameofString, valueText: nameofString, trailing: default(SyntaxTriviaList));
            var nameofIdentifierNode = SyntaxFactory.IdentifierName(nameofIdentifierToken);
            var nameofArgumentNode = SyntaxFactory.IdentifierName(identifierNameArgument);
            return generator.InvocationExpression(expression: nameofIdentifierNode, arguments: nameofArgumentNode);
        }

        protected override SyntaxNode GetParameterUsageAnalysisScope(SyntaxNode creation)
        {
            // if (s == null) { throw new ArgumentNullException(...); }
            // => The 's' parameter should be a nameof candidate because it was used in the if condition.
            var ifStatement = creation.FirstAncestorOrSelf<IfStatementSyntax>();
            if (ifStatement != null)
            {
                return ifStatement.Condition;
            }

            // _s = s ?? throw new ArgumentNullException(...);
            // => The 's' parameter should be a nameof candidate because it was checked for null prior to a throw-expression.
            var coalesceExpression = creation.FirstAncestorOrSelf<BinaryExpressionSyntax>(be => be.IsKind(SyntaxKind.CoalesceExpression));
            if (coalesceExpression?.Right.IsKind(SyntaxKind.ThrowExpression) == true)
            {
                return coalesceExpression.Left;
            }

            return null;
        }

        protected override SyntaxNode MoveArgumentToNextParameter(
            SyntaxNode creation,
            SyntaxNode argument,
            string newArgument)
        {
            var typedCreation = (ObjectCreationExpressionSyntax)creation;

            SyntaxNode newArgumentNode = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(newArgument)));

            ArgumentListSyntax newArgumentList = typedCreation.ArgumentList.InsertNodesBefore(argument, new[] { newArgumentNode });
            return typedCreation.WithArgumentList(newArgumentList);
        }
    }
}