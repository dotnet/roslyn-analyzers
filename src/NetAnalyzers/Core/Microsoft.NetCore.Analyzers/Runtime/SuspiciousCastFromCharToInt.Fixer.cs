// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.NetCore.Analyzers.Runtime.SuspiciousCastFromCharToIntAnalyzer;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class SuspiciousCastFromCharToIntFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RuleId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var doc = context.Document;
            var token = context.CancellationToken;
            var root = await doc.GetSyntaxRootAsync(token).ConfigureAwait(false);
            var model = await doc.GetSemanticModelAsync(token).ConfigureAwait(false);

            if (model.GetOperation(root.FindNode(context.Span), token) is not IArgumentOperation argumentOperation)
                return;

            var targetMethod = argumentOperation.Parent switch
            {
                IInvocationOperation invocation => invocation.TargetMethod,
                IObjectCreationOperation objectCreation => objectCreation.Constructor,
                _ => default
            };

            if (targetMethod is null)
                return;

            var apiHandlerCache = GetApiHandlerCache(model.Compilation);

            if (!apiHandlerCache.TryGetValue(targetMethod, out var handler))
                return;

            var codeAction = CodeAction.Create(
                MicrosoftNetCoreAnalyzersResources.SuspiciousCastFromCharToIntCodeFixTitle,
                token => handler.CreateChangedDocumentAsync(new FixCharCastContext(context, argumentOperation.Parent, this, token)),
                MicrosoftNetCoreAnalyzersResources.SuspiciousCastFromCharToIntCodeFixTitle);
            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        internal abstract SyntaxNode GetMemberAccessExpressionSyntax(SyntaxNode invocationExpressionSyntax);

        internal abstract SyntaxNode GetDefaultValueExpression(SyntaxNode parameterSyntax);
    }
}
