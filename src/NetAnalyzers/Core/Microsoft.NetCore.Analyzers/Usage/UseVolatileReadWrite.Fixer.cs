// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
            if (TryGetThreadVolatileReadWriteMemberAccess(node, ThreadVolatileReadMethodName, out var readAccess))
            {
                var codeAction = CodeAction.Create(
                    MicrosoftNetCoreAnalyzersResources.UseVolatileReadTitle,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(readAccess, CreateVolatileMemberAccess(context.Document, VolatileReadMethodName)))),
                    MicrosoftNetCoreAnalyzersResources.UseVolatileReadTitle
                );
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
            else if (TryGetThreadVolatileReadWriteMemberAccess(node, ThreadVolatileWriteMethodName, out var writeAccess))
            {
                var codeAction = CodeAction.Create(
                    MicrosoftNetCoreAnalyzersResources.UseVolatileWriteTitle,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(writeAccess, CreateVolatileMemberAccess(context.Document, VolatileWriteMethodName)))),
                    MicrosoftNetCoreAnalyzersResources.UseVolatileWriteTitle
                );
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }

        private static SyntaxNode CreateVolatileMemberAccess(Document document, string methodName)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            return generator.MemberAccessExpression(
                generator.IdentifierName(nameof(Volatile)),
                generator.IdentifierName(methodName)
            );
        }

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected abstract bool TryGetThreadVolatileReadWriteMemberAccess(SyntaxNode invocation, string methodName, [NotNullWhen(true)] out SyntaxNode? memberAccess);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("SYSLIB0054");
    }
}