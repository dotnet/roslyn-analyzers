// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class PreferStreamAsyncMemoryOverloadsFixer : CodeFixProvider
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
            if (root.FindNode(context.Span) is SyntaxNode node)
            {
                SemanticModel model = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false);
                if (model.GetOperation(node, ct) is IInvocationOperation invocation)
                {
                    string methodName = invocation.TargetMethod.Name;

                    string title;
                    if (methodName == "ReadAsync")
                    {
                        title = MicrosoftNetCoreAnalyzersResources.PreferStreamReadAsyncMemoryOverloadsTitle;
                    }
                    else if (methodName == "WriteAsync")
                    {
                        title = MicrosoftNetCoreAnalyzersResources.PreferStreamWriteAsyncMemoryOverloadsTitle;
                    }
                    else
                    {
                        return;
                    }

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: title,
                            createChangedDocument: c => FixInvocation(doc, root, invocation, methodName),
                            equivalenceKey: MicrosoftNetCoreAnalyzersResources.PreferStreamAsyncMemoryOverloadsMessage),
                        context.Diagnostics);
                }
            }
        }

        private static Task<Document> FixInvocation(Document doc, SyntaxNode root, IInvocationOperation invocation, string methodName)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            // The stream object
            SyntaxNode instanceNode = invocation.Instance.Syntax;

            // Need the byte array object so we can invoke its AsMemory() method
            SyntaxNode bufferInstanceNode = invocation.Arguments[0].Value.Syntax; // byte[] buffer

            // These arguments are not modified, just moved inside AsMemory
            SyntaxNode offsetNode = invocation.Arguments[1].Syntax; // int offset
            SyntaxNode countNode = invocation.Arguments[2].Syntax;  // int count

            // Generate an invocation of the AsMemory() method from the byte array object
            SyntaxNode asMemoryExpressionNode = generator.MemberAccessExpression(bufferInstanceNode, "AsMemory");
            SyntaxNode asMemoryInvocationNode = generator.InvocationExpression(asMemoryExpressionNode, offsetNode, countNode);

            // Create a new async method call for the stream object, no arguments yet
            SyntaxNode asyncMethodNode = generator.MemberAccessExpression(instanceNode, methodName);

            // Add the arguments to the async method call, with or without CancellationToken
            SyntaxNode newInvocationExpression;
            if (invocation.Arguments.Length > 3)
            {
                newInvocationExpression = generator.InvocationExpression(
                    asyncMethodNode, asMemoryInvocationNode,
                    invocation.Arguments[3].Syntax /* CancellationToken */);
            }
            else
            {
                newInvocationExpression = generator.InvocationExpression(asyncMethodNode, asMemoryInvocationNode);
            }

            SyntaxNode newInvocation = generator.ReplaceNode(root, invocation.Syntax, newInvocationExpression);
            return Task.FromResult(doc.WithSyntaxRoot(newInvocation));
        }
    }
}
