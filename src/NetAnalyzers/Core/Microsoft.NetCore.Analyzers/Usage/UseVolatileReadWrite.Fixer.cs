// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.NetCore.Analyzers.Usage
{
    public abstract class UseVolatileReadWriteFixer : CodeFixProvider
    {
        private const string ThreadVolatileReadMethodName = nameof(Thread.VolatileRead);
        private const string ThreadVolatileWriteMethodName = nameof(Thread.VolatileWrite);
        private const string VolatileReadMethodName = nameof(Volatile.Read);
        private const string VolatileWriteMethodName = nameof(Volatile.Write);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);
            if (TryGetThreadVolatileReadWriteArguments(node, ThreadVolatileReadMethodName, out var arguments))
            {
                var codeAction = CodeAction.Create(
                    MicrosoftNetCoreAnalyzersResources.DoNotUseThreadVolatileReadWriteCodeFixTitle,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, CreateVolatileMemberAccess(context.Document, VolatileReadMethodName, arguments).WithTriviaFrom(node)))),
                    nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseThreadVolatileReadWriteCodeFixTitle)
                );
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
            else if (TryGetThreadVolatileReadWriteArguments(node, ThreadVolatileWriteMethodName, out arguments))
            {
                var codeAction = CodeAction.Create(
                    MicrosoftNetCoreAnalyzersResources.DoNotUseThreadVolatileReadWriteCodeFixTitle,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, CreateVolatileMemberAccess(context.Document, VolatileWriteMethodName, arguments).WithTriviaFrom(node)))),
                    nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseThreadVolatileReadWriteCodeFixTitle)
                );
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }

        private static SyntaxNode CreateVolatileMemberAccess(Document document, string methodName, IEnumerable<SyntaxNode> arguments)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var memberAccess = generator.MemberAccessExpression(
                generator.IdentifierName(nameof(Volatile)),
                generator.IdentifierName(methodName)
            );

            return generator.InvocationExpression(memberAccess, arguments);
        }

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected abstract bool TryGetThreadVolatileReadWriteArguments(SyntaxNode invocation, string methodName, [NotNullWhen(true)] out IEnumerable<SyntaxNode>? arguments);

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("SYSLIB0054");
    }
}