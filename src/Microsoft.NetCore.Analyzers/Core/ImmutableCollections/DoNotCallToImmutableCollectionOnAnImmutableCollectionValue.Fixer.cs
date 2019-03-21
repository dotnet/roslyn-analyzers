// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.NetCore.Analyzers.ImmutableCollections
{
    /// <summary>
    /// CA2009: Do not call ToImmutableCollection on an ImmutableCollection value
    /// </summary>
    public abstract class DoNotCallToImmutableCollectionOnAnImmutableCollectionValueFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotCallToImmutableCollectionOnAnImmutableCollectionValueAnalyzer.RuleId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        private static readonly ImmutableArray<string> ToImmutableMethodNames = new[]
        {
            "ToImmutableArray",
            "ToImmutableDictionary",
            "ToImmutableHashSet",
            "ToImmutableList",
            "ToImmutableSortedDictionary",
            "ToImmutableSortedSet"
        }.ToImmutableArray();

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic == null)
            {
                return;
            }

            Document document = context.Document;
            TextSpan span = context.Span;
            SyntaxNode root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode invocationNode = root.FindNode(span, getInnermostNodeForTie: true);
            if (invocationNode == null)
            {
                return;
            }

            SemanticModel semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var invocationOp = semanticModel.GetOperation(invocationNode) as IInvocationOperation;
            if (invocationOp == null || !ToImmutableMethodNames.Contains(invocationOp.TargetMethod.Name))
            {
                return;
            }

            string title = SystemCollectionsImmutableAnalyzersResources.RemoveRedundantCall;

            context.RegisterCodeFix(new MyCodeAction(title,
                                        async cancellationToken => await RemoveRedundantCall(document, root, invocationNode, invocationOp).ConfigureAwait(false),
                                        equivalenceKey: title),
                                    diagnostic);
        }

        private static Task<Document> RemoveRedundantCall(Document document, SyntaxNode root, SyntaxNode invocationNode, IInvocationOperation invocationOp)
        {
            SyntaxNode instance = GetInstance(invocationOp).WithTrailingTrivia(invocationNode.GetTrailingTrivia());
            SyntaxNode newRoot = root.ReplaceNode(invocationNode, instance);
            Document newDocument = document.WithSyntaxRoot(newRoot);
            return Task.FromResult(newDocument);
        }

        private static SyntaxNode GetInstance(IInvocationOperation invocationOp)
        {
            return invocationOp.TargetMethod.IsExtensionMethod && invocationOp.Language != LanguageNames.VisualBasic ?
                invocationOp.Arguments[0].Value.Syntax :
                invocationOp.Instance.Syntax;
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
                : base(title, createChangedDocument, equivalenceKey)
            {
            }
        }
    }
}