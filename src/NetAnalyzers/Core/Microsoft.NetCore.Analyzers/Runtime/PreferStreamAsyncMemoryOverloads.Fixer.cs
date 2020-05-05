// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1835: Prefer Memory/ReadOnlyMemory overloads for Stream ReadAsync/WriteAsync methods.
    ///
    /// Undesired methods (available since .NET Framework 4.5):
    ///
    /// - Stream.WriteAsync(Byte[], Int32, Int32)
    /// - Stream.WriteAsync(Byte[], Int32, Int32, CancellationToken)
    /// - Stream.ReadAsync(Byte[], Int32, Int32)
    /// - Stream.ReadAsync(Byte[], Int32, Int32, CancellationToken)
    ///
    /// Preferred methods (available since .NET Standard 2.1 and .NET Core 2.1):
    ///
    /// - Stream.WriteAsync(ReadOnlyMemory{Byte}, CancellationToken)
    /// - Stream.ReadAsync(Memory{Byte}, CancellationToken)
    ///
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class PreferStreamAsyncMemoryOverloadsFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(PreferStreamAsyncMemoryOverloads.RuleId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document doc = context.Document;
            CancellationToken ct = context.CancellationToken;
            SyntaxNode root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

            if (!(root.FindNode(context.Span, getInnermostNodeForTie: true) is SyntaxNode node))
            {
                return;
            }

            SemanticModel model = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false);

            if (!(model.GetOperation(node, ct) is IInvocationOperation invocation))
            {
                return;
            }

            // Defensive check to ensure the fix is only attempted on one of the 4 specific undesired overloads
            if (invocation.Arguments.Length < 3 || invocation.Arguments.Length > 4)
            {
                return;
            }

            IArgumentOperation bufferArgumentOperation = invocation.Arguments.FirstOrDefault(a => a.Value.Type.Equals(SpecialType.System_Byte));
            if (bufferArgumentOperation == null)
            {
                return;
            }
            IArgumentOperation offsetArgumentOperation = invocation.Arguments.FirstOrDefault(a => a.Value.Type.Equals(SpecialType.System_Int32));

            string title = MicrosoftNetCoreAnalyzersResources.PreferStreamAsyncMemoryOverloadsTitle;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => FixInvocation(doc, root, invocation, invocation.TargetMethod.Name),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static Task<Document> FixInvocation(Document doc, SyntaxNode root, IInvocationOperation invocation, string methodName)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            // The stream object
            SyntaxNode instanceNode = invocation.Instance.Syntax;

            // Need the byte array object so we can invoke its AsMemory() method
            
            SyntaxNode bufferInstanceNode = invocation.Arguments[0].Value.Syntax; // byte[] buffer

            SyntaxNode offsetNode = invocation.Arguments[1].Syntax; // int offset
            SyntaxNode countNode = invocation.Arguments[2].Syntax;  // int count

            // Generate an invocation of the AsMemory() method from the byte array object
            SyntaxNode asMemoryExpressionNode = generator.MemberAccessExpression(bufferInstanceNode, "AsMemory");
            SyntaxNode asMemoryInvocationNode = generator.InvocationExpression(asMemoryExpressionNode, offsetNode, countNode);

            // Create a new async method call for the stream object, no arguments yet
            SyntaxNode asyncMethodNode = generator.MemberAccessExpression(instanceNode, methodName);

            // Add the arguments to the async method call, with or without CancellationToken
            SyntaxNode newInvocationExpression;
            if (invocation.Arguments.Length == 4)
            {
                newInvocationExpression = generator.InvocationExpression(
                    asyncMethodNode, asMemoryInvocationNode,
                    invocation.Arguments[3].Syntax /* CancellationToken */);
            }
            else
            {
                newInvocationExpression = generator.InvocationExpression(asyncMethodNode, asMemoryInvocationNode);
            }

            SyntaxNode newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocationExpression);
            return Task.FromResult(doc.WithSyntaxRoot(newRoot));
        }
    }
}
