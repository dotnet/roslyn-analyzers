// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.NetCore.Analyzers.Usage
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = DetectPLINQNopsAnalyzer.RuleId), Shared]
    public sealed class DetectPLINQNopsFixer : CodeFixProvider
    {
        private static readonly string[] removableEnds = new string[] { "ToList", "ToArray", "AsParallel" };

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DetectPLINQNopsAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root.FindNode(context.Span) is not InvocationExpressionSyntax declaration)
            {
                return;
            }

            context.RegisterCodeFix(
                new AsParallelCodeAction(
                    title: MicrosoftNetCoreAnalyzersResources.RemoveRedundantCall,
                    createChangedSolution: c => RemoveAsParallelCall(context.Document, declaration, c),
                    equivalenceKey: MicrosoftNetCoreAnalyzersResources.RemoveRedundantCall),
                context.Diagnostics);
        }

        private static async Task<Solution> RemoveAsParallelCall(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            var originalSolution = document.Project.Solution;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            ExpressionSyntax possibleInvocation = invocationExpression;

            do
            {
                var newExpression = ((possibleInvocation as InvocationExpressionSyntax)!.Expression as MemberAccessExpressionSyntax)!.Expression;
                possibleInvocation = newExpression;
            } while (possibleInvocation is InvocationExpressionSyntax nestedInvocation && nestedInvocation.Expression is MemberAccessExpressionSyntax member && removableEnds.Contains(member.Name.Identifier.ValueText));

            return originalSolution.WithDocumentSyntaxRoot(document.Id, root.ReplaceNode(invocationExpression, possibleInvocation));
        }

        private class AsParallelCodeAction : SolutionChangeAction
        {
            public AsPArallelCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey)
                : base(title, createChangedSolution, equivalenceKey)
            {
            }
        }
    }
}
