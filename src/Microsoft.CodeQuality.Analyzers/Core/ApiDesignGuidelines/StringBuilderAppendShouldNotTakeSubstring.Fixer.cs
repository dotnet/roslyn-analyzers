using System;
using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Analyzer.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    // TODO: VisualBasic as well?
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
            SyntaxNode node = root.FindNode(context.Span);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == StringBuilderAppendShouldNotTakeSubstring.RuleIdOneParameterId)
                {
                    context
                        .RegisterCodeFix(
                            new MyCodeAction(
                                MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterFix,
                                async ctx => await FixCodeOneParameter(context.Document, root, node, ctx).ConfigureAwait(false),
                                equivalenceKey: MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterFix),
                            diagnostic);
                }
                else
                {
                    context
                        .RegisterCodeFix(
                            new MyCodeAction(
                                MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterFix,
                                async ctx => await FixCodeTwoParameters(context.Document, root, node, ctx).ConfigureAwait(false),
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
            var typedNodeToFix = (InvocationExpressionSyntax)nodeToFix;
            var generator = SyntaxGenerator.GetGenerator(document);

            var fixComponents = GetFixComponents(generator, typedNodeToFix);

            SyntaxNode stringArgument = generator.IdentifierName(fixComponents.StringArgumentName);
            SyntaxNode lengthNode = generator
                .MemberAccessExpression(
                    stringArgument, 
                    generator.IdentifierName(nameof(string.Length)));

            var lengthArgument = generator
                .SubtractExpression(
                    lengthNode, 
                    fixComponents.OriginalInnerArguments[0].Expression);

            var newNode = generator.InvocationExpression(
                fixComponents.TargetMethod, 
                stringArgument,
                fixComponents.OriginalInnerArguments[0], 
                lengthArgument);
            var newRoot = root
                .ReplaceNode(
                    typedNodeToFix, 
                    newNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private struct FixComponents
        {
            public FixComponents(
                string stringArgumentName, 
                SyntaxNode targetMethod, 
                SeparatedSyntaxList<ArgumentSyntax> originalInnerArguments)
            {
                StringArgumentName = stringArgumentName;
                TargetMethod = targetMethod;
                OriginalInnerArguments = originalInnerArguments;
            }

            public string StringArgumentName { get; }
            public SyntaxNode TargetMethod { get; }
            public SeparatedSyntaxList<ArgumentSyntax> OriginalInnerArguments { get; }
        }

        private static async Task<Document> FixCodeTwoParameters(
            Document document,
            SyntaxNode root,
            SyntaxNode nodeToFix,
            CancellationToken cancellationToken)
        {
            var typedNodeToFix = (InvocationExpressionSyntax) nodeToFix;
            var generator = SyntaxGenerator.GetGenerator(document);

            FixComponents fixComponents = GetFixComponents(generator, typedNodeToFix);
            
            SyntaxNode stringArgument = generator.IdentifierName(fixComponents.StringArgumentName);

            var newNode = generator.InvocationExpression(
                fixComponents.TargetMethod,
                stringArgument,
                fixComponents.OriginalInnerArguments[0],
                fixComponents.OriginalInnerArguments[1]);
            var newRoot = root.ReplaceNode(typedNodeToFix, newNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private static FixComponents GetFixComponents(
            SyntaxGenerator generator, 
            InvocationExpressionSyntax typedNodeToFix)
        {
            if (typedNodeToFix.Expression is MemberAccessExpressionSyntax memberAccessExpression
                && memberAccessExpression.Expression is IdentifierNameSyntax identifierName)
            {
                string stringBuilderName = identifierName.Identifier.Text;

                SyntaxNode targetMethod = generator
                    .MemberAccessExpression(
                        generator.IdentifierName(stringBuilderName),
                        generator.IdentifierName(nameof(StringBuilder.Append)));

                ArgumentSyntax originalAppendArg = typedNodeToFix
                    .ArgumentList
                    .Arguments[0];

                if (originalAppendArg.Expression is InvocationExpressionSyntax originalSubstringExpression
                    && originalSubstringExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression2
                    && memberAccessExpression2.Expression is IdentifierNameSyntax identifierName2)
                {
                    return new FixComponents(
                        identifierName2.Identifier.ValueText,
                        targetMethod,
                        originalSubstringExpression
                            .ArgumentList
                            .Arguments);
                }
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
