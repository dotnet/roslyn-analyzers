using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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

            // TODO is it guaranteed that there's only one at once?
            Diagnostic diagnostic = context.Diagnostics.Single();
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

        private static async Task<Document> FixCodeOneParameter(
            Document document,
            SyntaxNode root,
            SyntaxNode nodeToFix,
            CancellationToken cancellationToken)
        {
            var typedNodeToFix = (InvocationExpressionSyntax)nodeToFix;
            var generator = SyntaxGenerator.GetGenerator(document);

            (string stringArgumentName, SyntaxNode targetMethod, SeparatedSyntaxList<ArgumentSyntax> originalInnerArguments) = GetFixComponents(generator, typedNodeToFix);

            SyntaxNode stringArgument = generator.IdentifierName(stringArgumentName);
            SyntaxNode lengthNode = generator
                .MemberAccessExpression(
                    stringArgument, 
                    generator.IdentifierName("Length"));

            var lengthArgument = generator.SubtractExpression(lengthNode, originalInnerArguments[0].Expression);

            var newNode = generator.InvocationExpression(
                targetMethod, 
                stringArgument,
                originalInnerArguments[0], 
                lengthArgument);
            var newRoot = root
                .ReplaceNode(
                    typedNodeToFix, 
                    newNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> FixCodeTwoParameters(
            Document document,
            SyntaxNode root,
            SyntaxNode nodeToFix,
            CancellationToken cancellationToken)
        {
            var typedNodeToFix = (InvocationExpressionSyntax) nodeToFix;
            var generator = SyntaxGenerator.GetGenerator(document);

            (string stringArgumentName, SyntaxNode targetMethod, SeparatedSyntaxList<ArgumentSyntax> originalInnerArguments) = GetFixComponents(generator, typedNodeToFix);

            SyntaxNode stringArgument = generator.IdentifierName(stringArgumentName);

            var newNode = generator.InvocationExpression(
                targetMethod,
                stringArgument,
                originalInnerArguments[0],
                originalInnerArguments[1]);
            var newRoot = root.ReplaceNode(typedNodeToFix, newNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private static (string stringArgument, SyntaxNode targetMethod, SeparatedSyntaxList<ArgumentSyntax> originalInnerArguments) GetFixComponents(
            SyntaxGenerator generator, 
            InvocationExpressionSyntax typedNodeToFix)
        {
            string stringBuilderName = ((typedNodeToFix.Expression as MemberAccessExpressionSyntax)
                .Expression as IdentifierNameSyntax).Identifier.Text;

            SyntaxNode targetMethod = generator
                .MemberAccessExpression(
                    generator.IdentifierName(stringBuilderName),
                    generator.IdentifierName("Append"));

            ArgumentSyntax originalAppendArg = typedNodeToFix
                .ArgumentList
                .Arguments[0];
            InvocationExpressionSyntax originalSubstringExpression = originalAppendArg.Expression as InvocationExpressionSyntax;
            

            var x1 = originalAppendArg.Expression as InvocationExpressionSyntax;
            var x2 = x1.Expression as MemberAccessExpressionSyntax;
            var x3 = x2.Expression as IdentifierNameSyntax;
            string stringArgumentName = x3.Identifier.ValueText;

            return (
                stringArgumentName, 
                targetMethod,
                originalSubstringExpression
                    .ArgumentList
                    .Arguments);
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
