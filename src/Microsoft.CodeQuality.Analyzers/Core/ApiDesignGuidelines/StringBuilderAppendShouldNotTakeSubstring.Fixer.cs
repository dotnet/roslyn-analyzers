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
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            SyntaxNode node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == StringBuilderAppendShouldNotTakeSubstring.RuleIdOneParameterId)
                {
                    context
                        .RegisterCodeFix(
                            new MyCodeAction(
                                MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterFix,
                                ctx => FixCodeOneParameter(context.Document, root, node, ctx),
                                equivalenceKey: MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterFix),
                            diagnostic);
                }
                else
                {
                    context
                        .RegisterCodeFix(
                            new MyCodeAction(
                                MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterFix,
                                ctx => FixCodeTwoParameters(context.Document, root, node, ctx),
                                equivalenceKey: MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterFix),
                            diagnostic);
                }
            }
        }

        private static async Task<Document> FixCodeOneParameter(
            Document document,
            SyntaxNode root,
            SyntaxNode nodeToFix,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var typedNodeToFix =  semanticModel.GetOperation(nodeToFix) as IInvocationOperation;
            var generator = SyntaxGenerator.GetGenerator(document);

            var fixComponents = GetFixComponents(generator, typedNodeToFix);

            SyntaxNode lengthNode = generator
                .MemberAccessExpression(
                    fixComponents.StringArgument.Syntax, 
                    generator.IdentifierName(nameof(string.Length)));

            var startIndexNode = fixComponents.OriginalInnerArguments[0].Value.Syntax;

            var lengthArgument = generator
                .SubtractExpression(
                    lengthNode,
                    startIndexNode);

            var newNode = generator.InvocationExpression(
                fixComponents.TargetMethod, 
                fixComponents.StringArgument.Syntax,
                startIndexNode, 
                lengthArgument);

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
            SyntaxNode root,
            SyntaxNode nodeToFix,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var typedNodeToFix = semanticModel.GetOperation(nodeToFix) as IInvocationOperation;
            var generator = SyntaxGenerator.GetGenerator(document);

            FixComponents fixComponents = GetFixComponents(generator, typedNodeToFix);
          
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

        private static FixComponents GetFixComponents(
            SyntaxGenerator generator, 
            IInvocationOperation typedNodeToFix)
        {
            var argumentOperation = typedNodeToFix.Arguments[0].Value;
            if (argumentOperation is IInvocationOperation stringBuilderAppendInvocationCandidate)
            {
                var innerArguments = stringBuilderAppendInvocationCandidate.Arguments;
                var instance = stringBuilderAppendInvocationCandidate.Instance;
                
                var append = generator.MemberAccessExpression(
                    typedNodeToFix.Instance.Syntax,
                    generator.IdentifierName(nameof(StringBuilder.Append)));
                var instanceType = instance.Type;

                return new FixComponents(
                    stringArgument: instance,
                    targetMethod: append,
                    originalInnerArguments: innerArguments);
            }

            return default(FixComponents);
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
