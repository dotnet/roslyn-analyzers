// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Analyzer.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public sealed class StringBuilderAppendShouldNotTakeSubstringFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray
            .Create(
                StringBuilderAppendShouldNotTakeSubstring.RuleIdOneParameterId,
                StringBuilderAppendShouldNotTakeSubstring.RuleIdTwoParameterId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == StringBuilderAppendShouldNotTakeSubstring.RuleIdOneParameterId)
                {
                    context
                        .RegisterCodeFix(
                            new MyCodeAction(
                                MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadFix,
                                ct => FixCodeOneParameter(context.Document, context.Span, ct)),
                            diagnostic);
                }
                else
                {
                    context
                        .RegisterCodeFix(
                            new MyCodeAction(
                                MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadFix,
                                ct => FixCodeTwoParameters(context.Document, context.Span, ct)),
                            diagnostic);
                }
            }
        }

        private static async Task<Document> FixCodeOneParameter(
            Document document,
            TextSpan span,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var nodeToFix = root.FindNode(span, getInnermostNodeForTie: true);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var typedNodeToFix =  semanticModel.GetOperation(nodeToFix) as IInvocationOperation;
            var generator = SyntaxGenerator.GetGenerator(document);

            var fixComponents = GetFixComponents(generator, typedNodeToFix);

            // generate text.Length (or whatever "text" is instead in the actual code)
            var lengthNode = generator
                .MemberAccessExpression(
                    fixComponents.StringArgument.Syntax, 
                    generator.IdentifierName(nameof(string.Length)));

            var startIndexNode = fixComponents.OriginalInnerArguments[0].Value.Syntax;

            // generate "text".Length - 2 (where 2 is the start index given in the original code
            var lengthArgument = generator
                .SubtractExpression(
                    lengthNode,
                    startIndexNode);

            // generate sb.Append(text, 2, text.Length - 2)
            var newNode = generator.InvocationExpression(
                fixComponents.TargetMethod, 
                fixComponents.StringArgument.Syntax,
                startIndexNode, 
                lengthArgument);

            // replace sb.Append(text.Substring(2)) by sb.Append(text, 2, text.Length - 2)
            var newRoot = root
                .ReplaceNode(
                    nodeToFix, 
                    newNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private struct FixComponents
        {
            public FixComponents(
                IOperation stringArgument, 
                SyntaxNode targetMethod, 
                ImmutableArray<IArgumentOperation> originalInnerArguments)
            {
                StringArgument = stringArgument;
                TargetMethod = targetMethod;
                OriginalInnerArguments = originalInnerArguments;
            }

            public IOperation StringArgument { get; }
            public SyntaxNode TargetMethod { get; }
            public ImmutableArray<IArgumentOperation> OriginalInnerArguments { get; }
        }

        private static async Task<Document> FixCodeTwoParameters(
            Document document,
            TextSpan span,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var nodeToFix = root.FindNode(span, getInnermostNodeForTie: true);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (semanticModel.GetOperation(nodeToFix) is IInvocationOperation typedNodeToFix)
            {
                var generator = SyntaxGenerator.GetGenerator(document);

                var fixComponents = GetFixComponents(generator, typedNodeToFix);

                // from sb.Append(text.Substring(2, 5)) generate sb.Append(text, 2, 5)
                var newNode = generator.InvocationExpression(
                    fixComponents.TargetMethod,
                    fixComponents.StringArgument.Syntax,
                    fixComponents.OriginalInnerArguments[0].Syntax,
                    fixComponents.OriginalInnerArguments[1].Syntax);
                var newRoot = root.ReplaceNode(
                    nodeToFix,
                    newNode);

                return document.WithSyntaxRoot(newRoot);
            }

            return document;
        }

        private static FixComponents GetFixComponents(
            SyntaxGenerator generator, 
            IInvocationOperation typedNodeToFix)
        {
            var argumentOperation = typedNodeToFix.Arguments[0].Value;
            if (argumentOperation is IInvocationOperation stringBuilderAppendInvocationCandidate)
            {
                // if the stringBuilder instance is sb, generate sb.Append()
                var append = generator.MemberAccessExpression(
                    typedNodeToFix.Instance.Syntax,
                    generator.IdentifierName(nameof(StringBuilder.Append)));

                return new FixComponents(
                    stringArgument: stringBuilderAppendInvocationCandidate.Instance,
                    targetMethod: append,
                    originalInnerArguments: stringBuilderAppendInvocationCandidate.Arguments);
            }

            return default(FixComponents);
        }

        // Needed for Telemetry (https://github.com/dotnet/roslyn-analyzers/issues/192)
        private class MyCodeAction : DocumentChangeAction
        {
            public MyCodeAction(
                string title, 
                Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(
                    title, 
                    createChangedDocument, 
                    title)
            {
            }
        }
    }
}
