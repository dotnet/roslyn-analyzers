// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
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
    public abstract class PreferStreamAsyncMemoryOverloadsFixer : CodeFixProvider
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

            IArgumentOperation? bufferOperation = GetArgumentByPositionOrName(invocation.Arguments, 0, "buffer", out bool isBufferNamed);
            if (bufferOperation == null)
            {
                return;
            }

            IArgumentOperation? offsetOperation = GetArgumentByPositionOrName(invocation.Arguments, 1, "offset", out bool isOffsetNamed);
            if (offsetOperation == null)
            {
                return;
            }

            IArgumentOperation? countOperation = GetArgumentByPositionOrName(invocation.Arguments, 2, "count", out bool isCountNamed);
            if (countOperation == null)
            {
                return;
            }

            // No nullcheck for this, because there is an overload that may not contain it
            IArgumentOperation? cancellationTokenOperation = GetArgumentByPositionOrName(invocation.Arguments, 3, "cancellationToken", out bool isCancellationTokenNamed);

            string title = MicrosoftNetCoreAnalyzersResources.PreferStreamAsyncMemoryOverloadsTitle;

            Task<Document> fixInvocation = FixInvocation(doc, root, invocation, invocation.TargetMethod.Name,
                                                         bufferOperation.Value.Syntax, isBufferNamed,
                                                         offsetOperation.Value.Syntax, isOffsetNamed,
                                                         countOperation.Value.Syntax, isCountNamed,
                                                         cancellationTokenOperation?.Value.Syntax, isCancellationTokenNamed);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => fixInvocation,
                    equivalenceKey: title),
                context.Diagnostics);
        }

        // Checks if the argument in the specified index has a name. If it doesn't, returns that arguments. If it does, then looks for the argument using the specified name, and returns it, or null if not found.
        protected abstract IArgumentOperation? GetArgumentByPositionOrName(ImmutableArray<IArgumentOperation> args, int index, string name, out bool isNamed);

        private static Task<Document> FixInvocation(Document doc, SyntaxNode root, IInvocationOperation invocation, string methodName,
            SyntaxNode bufferValueNode, bool isBufferNamed,
            SyntaxNode offsetValueNode, bool isOffsetNamed,
            SyntaxNode countValueNode, bool isCountNamed,
            SyntaxNode? cancellationTokenValueNode, bool isCancellationTokenNamed)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(doc);

            // The stream object
            SyntaxNode streamInstanceNode = invocation.Instance.Syntax;

            // buffer.AsMemory(int start, int length)
            // offset should become start
            // count should become length
            SyntaxNode namedStartNode = isOffsetNamed ? generator.Argument("start", RefKind.None, offsetValueNode) : offsetValueNode;
            SyntaxNode namedLengthNode = isCountNamed ? generator.Argument("length", RefKind.None, countValueNode) : countValueNode;

            // Generate an invocation of the AsMemory() method from the byte array object, using the correct named arguments
            SyntaxNode asMemoryExpressionNode = generator.MemberAccessExpression(bufferValueNode, "AsMemory");
            SyntaxNode asMemoryInvocationNode = generator.InvocationExpression(asMemoryExpressionNode, namedStartNode, namedLengthNode);

            // Generate the new buffer argument, ensuring we include the argument name if the user originally indicated one
            SyntaxNode namedBufferNode = isBufferNamed ? generator.Argument("buffer", RefKind.None, asMemoryInvocationNode) : asMemoryInvocationNode;

            // Create an async method call for the stream object with no arguments
            SyntaxNode asyncMethodNode = generator.MemberAccessExpression(streamInstanceNode, methodName);

            // Add the arguments to the async method call, with or without CancellationToken
            SyntaxNode[] nodeArguments;
            if (cancellationTokenValueNode != null)
            {
                SyntaxNode namedCancellationTokenNode = isCancellationTokenNamed ? generator.Argument("cancellationToken", RefKind.None, cancellationTokenValueNode) : cancellationTokenValueNode;
                nodeArguments = new SyntaxNode[] { namedBufferNode, namedCancellationTokenNode };
            }
            else
            {
                nodeArguments = new SyntaxNode[] { namedBufferNode };
            }
            SyntaxNode newInvocationExpression = generator.InvocationExpression(asyncMethodNode, nodeArguments);

            SyntaxNode newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocationExpression);
            return Task.FromResult(doc.WithSyntaxRoot(newRoot));
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class PreferStreamAsyncMemoryOverloadsCSharpFixer : PreferStreamAsyncMemoryOverloadsFixer
    {
        protected override IArgumentOperation? GetArgumentByPositionOrName(ImmutableArray<IArgumentOperation> args, int index, string name, out bool isNamed)
        {
            isNamed = false;

            // The expected position is beyond the total arguments, so we don't expect to find the argument in the array
            if (index >= args.Length)
            {
                return null;
            }
            // If the argument in the specified index does not have a name, then it is in its expected position
            else if (args[index].Syntax is CodeAnalysis.CSharp.Syntax.ArgumentSyntax argNode && argNode.NameColon == null)
            {
                return args[index];
            }
            // Otherwise, find it by name
            else
            {
                isNamed = true;
                return args.FirstOrDefault(argOperation =>
                {
                    return argOperation.Syntax is CodeAnalysis.CSharp.Syntax.ArgumentSyntax argNode &&
                           argNode.NameColon != null &&
                           argNode.NameColon.Name != null &&
                           argNode.NameColon.Name.Identifier != null &&
                           argNode.NameColon.Name.Identifier.ValueText == name;
                });
            }
        }

    }

    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class PreferStreamAsyncMemoryOverloadsVisualBasicFixer : PreferStreamAsyncMemoryOverloadsFixer
    {
        protected override IArgumentOperation? GetArgumentByPositionOrName(ImmutableArray<IArgumentOperation> args, int index, string name, out bool isNamed)
        {
            isNamed = false;

            // The expected position is beyond the total arguments, so we don't expect to find the argument in the array
            if (index >= args.Length)
            {
                return null;
            }
            // If the argument in the specified index does not have a name, then it is in its expected position
            else if (args[index].Syntax is CodeAnalysis.VisualBasic.Syntax.SimpleArgumentSyntax argNode && argNode.NameColonEquals == null)
            {
                return args[index];
            }
            // Otherwise, find it by name
            else
            {
                isNamed = true;
                return args.FirstOrDefault(argOperation =>
                {
                    return argOperation.Syntax is CodeAnalysis.VisualBasic.Syntax.SimpleArgumentSyntax simpleArgNode &&
                           simpleArgNode.NameColonEquals != null &&
                           simpleArgNode.NameColonEquals.Name != null &&
                           simpleArgNode.NameColonEquals.Name.Identifier != null &&
                           simpleArgNode.NameColonEquals.Name.Identifier.ValueText == name;
                });
            }
        }
    }
}
