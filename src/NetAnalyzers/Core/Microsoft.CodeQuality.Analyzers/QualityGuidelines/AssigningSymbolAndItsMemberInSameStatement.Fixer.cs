// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#nullable enable

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = AssigningSymbolAndItsMemberInSameStatement.RuleId), Shared]
    public sealed class AssigningSymbolAndItsMemberInSameStatementFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AssigningSymbolAndItsMemberInSameStatement.RuleId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // For something like `a.x = a = b;`, we offer 3 code fixes:
            // First:
            //     a = b;
            //     a.x = b;
            // Second:
            //     a = b;
            //     a.x = a;
            // Third: (Not currently implemented)
            //     var temp = a;
            //     a = b;
            //     temp.x = b;

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root.FindNode(context.Span).Parent is not AssignmentExpressionSyntax assignment)
            {
                return;
            }

            var members = GetAssignmentMembers(assignment);
            if (members.Count < 3)
            {
                return;
            }

            var mostRightMember = members.Peek(); // Don't move this near its usage. The result will be different.
            var leadingTrivia = assignment.Parent.GetLeadingTrivia();
            var trailingTrivia = assignment.Parent.GetTrailingTrivia();
            var replacements = new List<SyntaxNode>(members.Count - 1);

            while (members.Count > 2)
            {
                replacements.Add(GetAssignmentExpressionStatement(members, leadingTrivia, trailingTrivia));
                trailingTrivia = SyntaxTriviaList.Empty; // Take the trailing trivia on the first assignment only.
            }

            var title = MicrosoftCodeQualityAnalyzersResources.AssigningSymbolAndItsMemberInSameStatementTitle;

            var replacements1 = replacements.Concat(GetAssignmentExpressionStatement(assignment.Left, mostRightMember, leadingTrivia, trailingTrivia));
            var replacements2 = replacements.Concat(GetAssignmentExpressionStatement(members, leadingTrivia, trailingTrivia));

            var nestedCodeAction = CodeAction.Create(title, ImmutableArray.Create<CodeAction>(
                new MyCodeAction($"{title} 1", ct => GetDocument(context.Document, root, assignment.Parent, replacements1)),
                new MyCodeAction($"{title} 2", ct => GetDocument(context.Document, root, assignment.Parent, replacements2))
                ), isInlinable: false);

            context.RegisterCodeFix(nestedCodeAction, context.Diagnostics);
        }

        /// <summary>
        /// If the assignment expression is:  a = b = c = d
        /// Return a stack containing a, b, c, d with `a` at the bottom and `d` at the top.
        /// </summary>
        private static Stack<ExpressionSyntax> GetAssignmentMembers(AssignmentExpressionSyntax node)
        {
            var stack = new Stack<ExpressionSyntax>();
            ExpressionSyntax current = node;
            while (current is AssignmentExpressionSyntax assignment)
            {
                stack.Push(assignment.Left);
                current = assignment.Right;
            }
            stack.Push(current);
            return stack;
        }

        private static ExpressionStatementSyntax GetAssignmentExpressionStatement(Stack<ExpressionSyntax> stack, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
        {
            var right = stack.Pop();
            var left = stack.Peek();
            return GetAssignmentExpressionStatement(left, right, leadingTrivia, trailingTrivia);
        }

        private static ExpressionStatementSyntax GetAssignmentExpressionStatement(ExpressionSyntax left, ExpressionSyntax right, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
            => SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right))
                    .WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);

        private static Task<Document> GetDocument(Document document, SyntaxNode root, SyntaxNode oldNode, IEnumerable<SyntaxNode> replacements)
            => Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(oldNode, replacements)));

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument, title)
            {
            }
        }
    }
}
