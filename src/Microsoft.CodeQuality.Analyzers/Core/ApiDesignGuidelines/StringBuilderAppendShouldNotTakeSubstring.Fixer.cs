// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Analyzer.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
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
            return StringBuilderAppendShouldNotTakeSubstringFixAllProvider.Instance;
        }

        private class StringBuilderAppendShouldNotTakeSubstringFixAllProvider : FixAllProvider
        {
            public static StringBuilderAppendShouldNotTakeSubstringFixAllProvider Instance = new StringBuilderAppendShouldNotTakeSubstringFixAllProvider();

            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var diagnosticsToFix = new List<KeyValuePair<Project, ImmutableArray<Diagnostic>>>();
                string titleFormat = "Inline String.Substring to StringBuilder.Append() in {0} {1}"; // TODO: use localizable string here!
                string title = null;

                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                    {
                        var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document)
                            .ConfigureAwait(false);
                        diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                        title = string.Format(titleFormat, "document", fixAllContext.Document.Name);
                        break;
                    }
                    case FixAllScope.Project:
                    {
                        var project = fixAllContext.Project;
                        var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                        diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));
                        title = string.Format(titleFormat, "project", fixAllContext.Project.Name);
                        break;
                    }
                    case FixAllScope.Solution:
                    {
                        foreach (var project in fixAllContext.Solution.Projects)
                        {
                            var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));
                        }

                        title = "Fix all occurrences in the solution";
                        break;
                    }
                    case FixAllScope.Custom:
                        return null;
                    default:
                        break;
                }

                return new FixAllStringBuilderAppendShouldNotTakeSubstringDocumentChangeAction(title, fixAllContext.Solution, diagnosticsToFix);
            }
        }

        private class FixAllStringBuilderAppendShouldNotTakeSubstringDocumentChangeAction : CodeAction
        {
            private readonly List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> _diagnosticsToFix;
            private readonly Solution _solution;
            public FixAllStringBuilderAppendShouldNotTakeSubstringDocumentChangeAction(string title, Solution solution,
                List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                Title = title;
                _solution = solution;
                _diagnosticsToFix = diagnosticsToFix;
            }

            public override string Title { get; }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var solution = _solution;

                foreach (var pair in _diagnosticsToFix)
                {
                    var project = pair.Key;
                    var diagnostics = pair.Value;
                    var groupedDiagnostics = diagnostics.Where(d => d.Location.IsInSource)
                        .GroupBy(d => d.Location.SourceTree);

                    foreach (var grouping in groupedDiagnostics)
                    {
                        var document = project.GetDocument(grouping.Key);
                        if (document == null)
                        {
                            continue;
                        }

                        // going to fix bottom up to keep spans of diagnostics still to fix intact by not manipulating their location:
                        var diagnosticsInDocument = grouping.OrderByDescending(g => g.Location.SourceSpan.Start);

                        foreach (var diagnostic in diagnosticsInDocument)
                        {
                            if (diagnostic.Id == StringBuilderAppendShouldNotTakeSubstring.RuleIdOneParameterId)
                            {
                                document = await FixCodeOneParameter(document, diagnostic.Location.SourceSpan,
                                    cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                document = await FixCodeTwoParameters(document, diagnostic.Location.SourceSpan,
                                    cancellationToken).ConfigureAwait(false);
                            }
                        }

                        solution = solution.WithDocumentSyntaxRoot(document.Id,
                            await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false));
                    }
                }

                return solution;
            }
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
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

            return Task.CompletedTask;
        }

        private static async Task<Document> FixCodeOneParameter(
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

                if (fixComponents.Equals(default(FixComponents)))
                {
                    // something went horribly wrong.
                    return document;
                }

                // generate text.Length (or whatever "text" is instead in the actual code)
                var lengthNode = generator
                    .MemberAccessExpression(
                        fixComponents.StringArgument.Syntax, 
                        generator.IdentifierName(nameof(string.Length)));

                var startIndexNode = fixComponents.OriginalInnerArguments[0].Value.Syntax;

                // generate "text".Length - 2 (where 2 is the start index given in the original code
                var lengthArgument = generator.SubtractExpression(lengthNode, startIndexNode);

                // generate sb.Append(text, 2, text.Length - 2)
                var newNode = generator.InvocationExpression(
                    fixComponents.TargetMethod, 
                    fixComponents.StringArgument.Syntax,
                    startIndexNode, 
                    lengthArgument);

                // replace sb.Append(text.Substring(2)) by sb.Append(text, 2, text.Length - 2)
                var newRoot = root.ReplaceNode(nodeToFix, newNode);
                return document.WithSyntaxRoot(newRoot);
            }

            return document;
        }

        private struct FixComponents
        {
            public FixComponents(IOperation stringArgument, SyntaxNode targetMethod, ImmutableArray<IArgumentOperation> originalInnerArguments)
            {
                StringArgument = stringArgument;
                TargetMethod = targetMethod;
                OriginalInnerArguments = originalInnerArguments;
            }

            public IOperation StringArgument { get; }
            public SyntaxNode TargetMethod { get; }
            public ImmutableArray<IArgumentOperation> OriginalInnerArguments { get; }
        }

        private static async Task<Document> FixCodeTwoParameters(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var nodeToFix = root.FindNode(span, getInnermostNodeForTie: true);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (semanticModel.GetOperation(nodeToFix) is IInvocationOperation typedNodeToFix)
            {
                var generator = SyntaxGenerator.GetGenerator(document);

                var fixComponents = GetFixComponents(generator, typedNodeToFix);

                if (fixComponents.Equals(default(FixComponents)))
                {
                    // something went horribly wrong, 
                    return document;
                }

                // check if named parameters are used to change parameter order:
                var (startIndexArgument, lengthArgument) = fixComponents.OriginalInnerArguments[0].Parameter.Name == "length"
                    ? (fixComponents.OriginalInnerArguments[1], fixComponents.OriginalInnerArguments[0])
                    : (fixComponents.OriginalInnerArguments[0], fixComponents.OriginalInnerArguments[1]);
                
                // from sb.Append(text.Substring(2, 5)) generate sb.Append(text, 2, 5)
                var newNode = generator.InvocationExpression(
                    fixComponents.TargetMethod,
                    fixComponents.StringArgument.Syntax,
                    startIndexArgument.Value.Syntax,
                    lengthArgument.Value.Syntax);

                var newRoot = root.ReplaceNode(nodeToFix, newNode);

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
                : base(title, createChangedDocument, title)
            {
            }
        }
    }
}
