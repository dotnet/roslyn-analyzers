// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#nullable enable

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = AssigningSymbolAndItsMemberInSameStatement.RuleId), Shared]
    public sealed class AssigningSymbolAndItsMemberInSameStatementFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AssigningSymbolAndItsMemberInSameStatement.RuleId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
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

            var title = MicrosoftCodeQualityAnalyzersResources.AssigningSymbolAndItsMemberInSameStatementTitle;
            context.RegisterCodeFix(new MyCodeAction(title,
                 async ct => await SplitAssignmentFirstOption(context.Document, context.Span, ct).ConfigureAwait(false),
                 equivalenceKey: title + "0"),
            context.Diagnostics);
            context.RegisterCodeFix(new MyCodeAction(title,
                 async ct => await SplitAssignmentSecondOption(context.Document, context.Span, ct).ConfigureAwait(false),
                 equivalenceKey: title + "1"),
            context.Diagnostics);
            return Task.CompletedTask;
        }

        private static async Task<Document> SplitAssignmentFirstOption(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            // This method splits `a.x = a = b` to:
            // a = b;
            // a.x = b;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // a.x = a = b;
            var parentAssignment = root.FindNode(span).Parent;

            if (!TryGetAssignmentExpressionParts(parentAssignment, out _, out var right) ||
                !TryGetAssignmentExpressionParts(right, out _, out var rightOfRight))
            {
                return document;
            }

            // a = b;
            right = GetExpressionFromAssignment(right).WithTriviaFrom(parentAssignment.Parent);

            // a.x = b;
            var firstEqualsLastAssignment = GetExpressionFromAssignment(GetAssignmentWithRight(parentAssignment, rightOfRight));

            root = root.ReplaceNode(parentAssignment.Parent, new[] { right, firstEqualsLastAssignment });
            return document.WithSyntaxRoot(root);
        }

        private static async Task<Document> SplitAssignmentSecondOption(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            // This method splits `a.x = a = b` to:
            // a = b;
            // a.x = a;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // a.x = a = b;
            var parentAssignment = root.FindNode(span).Parent;

            if (!TryGetAssignmentExpressionParts(parentAssignment, out _, out var right) ||
                !TryGetAssignmentExpressionParts(right, out var leftOfRight, out _))
            {
                return document;
            }

            // a = b;
            right = GetExpressionFromAssignment(right).WithTriviaFrom(parentAssignment.Parent);

            // a.x = a;
            var firstEqualsSecondAssignment = GetExpressionFromAssignment(GetAssignmentWithRight(parentAssignment, leftOfRight));

            root = root.ReplaceNode(parentAssignment.Parent, new[] { right, firstEqualsSecondAssignment });
            return document.WithSyntaxRoot(root);
        }

        private static SyntaxNode GetAssignmentWithRight(SyntaxNode assignmentExpression, SyntaxNode newRight)
        {
            return ((AssignmentExpressionSyntax)assignmentExpression).WithRight((ExpressionSyntax)newRight);
        }

        private static SyntaxNode GetExpressionFromAssignment(SyntaxNode assignmentExpression)
        {
            return SyntaxFactory.ExpressionStatement((AssignmentExpressionSyntax)assignmentExpression);
        }

        private static bool TryGetAssignmentExpressionParts(SyntaxNode assignmentExpression, [NotNullWhen(true)] out SyntaxNode? left, [NotNullWhen(true)] out SyntaxNode? right)
        {
            if (assignmentExpression is AssignmentExpressionSyntax assignment)
            {
                left = assignment.Left;
                right = assignment.Right;
                return true;
            }
            left = null;
            right = null;
            return false;
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}
