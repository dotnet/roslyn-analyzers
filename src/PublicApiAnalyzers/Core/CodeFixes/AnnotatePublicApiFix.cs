﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using DiagnosticIds = Roslyn.Diagnostics.Analyzers.RoslynDiagnosticIds;

#nullable enable

namespace Microsoft.CodeAnalysis.PublicApiAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "AnnotatePublicApiFix"), Shared]
    public sealed class AnnotatePublicApiFix : CodeFixProvider
    {
        private const char ObliviousMarker = '~';

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DiagnosticIds.AnnotatePublicApiRuleId);

        public sealed override FixAllProvider GetFixAllProvider()
            => new PublicSurfaceAreaFixAllProvider();

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Project project = context.Document.Project;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                string minimalSymbolName = diagnostic.Properties[DeclarePublicApiAnalyzer.MinimalNamePropertyBagKey];
                string publicSymbolName = diagnostic.Properties[DeclarePublicApiAnalyzer.PublicApiNamePropertyBagKey];
                string publicSymbolNameWithNullability = diagnostic.Properties[DeclarePublicApiAnalyzer.PublicApiNameWithNullabilityPropertyBagKey];
                string fileName = diagnostic.Properties[DeclarePublicApiAnalyzer.FileName];

                TextDocument? document = project.GetPublicApiDocument(fileName);

                if (document != null)
                {
                    context.RegisterCodeFix(
                            new DeclarePublicApiFix.AdditionalDocumentChangeAction(
                                $"Annotate {minimalSymbolName} in public API",
                                document.Id,
                                c => GetFix(document, publicSymbolName, publicSymbolNameWithNullability, c)),
                            diagnostic);
                }
            }

            return Task.CompletedTask;

            static async Task<Solution> GetFix(TextDocument publicSurfaceAreaDocument, string oldSymbolName, string newSymbolName, CancellationToken cancellationToken)
            {
                SourceText sourceText = await publicSurfaceAreaDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
                SourceText newSourceText = AnnotateSymbolNamesInSourceText(sourceText, new Dictionary<string, string> { [oldSymbolName] = newSymbolName });

                return publicSurfaceAreaDocument.Project.Solution.WithAdditionalDocumentText(publicSurfaceAreaDocument.Id, newSourceText);
            }
        }

        private static SourceText AnnotateSymbolNamesInSourceText(SourceText sourceText, Dictionary<string, string> changes)
        {
            if (changes.Count == 0)
            {
                return sourceText;
            }

            List<string> lines = DeclarePublicApiFix.GetLinesFromSourceText(sourceText);

            for (int i = 0; i < lines.Count; i++)
            {
                if (changes.TryGetValue(lines[i].Trim(ObliviousMarker), out string newLine))
                {
                    lines.Insert(i, newLine);
                    lines.RemoveAt(i + 1);
                }
            }

            var endOfLine = sourceText.GetEndOfLine();
            SourceText newSourceText = sourceText.Replace(new TextSpan(0, sourceText.Length), string.Join(endOfLine, lines) + sourceText.GetEndOfFileText(endOfLine));
            return newSourceText;
        }

        private class FixAllAdditionalDocumentChangeAction : CodeAction
        {
            private readonly List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> _diagnosticsToFix;
            private readonly Solution _solution;

            public FixAllAdditionalDocumentChangeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                this.Title = title;
                _solution = solution;
                _diagnosticsToFix = diagnosticsToFix;
            }

            public override string Title { get; }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var updatedPublicSurfaceAreaText = new List<(DocumentId, SourceText)>();

                foreach (var (project, diagnostics) in _diagnosticsToFix)
                {
                    IEnumerable<IGrouping<SyntaxTree, Diagnostic>> groupedDiagnostics =
                        diagnostics
                            .Where(d => d.Location.IsInSource)
                            .GroupBy(d => d.Location.SourceTree);

                    var allChanges = new Dictionary<string, Dictionary<string, string>>();

                    foreach (IGrouping<SyntaxTree, Diagnostic> grouping in groupedDiagnostics)
                    {
                        Document document = project.GetDocument(grouping.Key);

                        if (document is null)
                        {
                            continue;
                        }

                        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                        SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                        foreach (Diagnostic diagnostic in grouping)
                        {
                            if (diagnostic.Id != DeclarePublicApiAnalyzer.AnnotateApiRule.Id)
                            {
                                continue;
                            }

                            string oldName = diagnostic.Properties[DeclarePublicApiAnalyzer.PublicApiNamePropertyBagKey];
                            string newName = diagnostic.Properties[DeclarePublicApiAnalyzer.PublicApiNameWithNullabilityPropertyBagKey];
                            bool isShipped = diagnostic.Properties[DeclarePublicApiAnalyzer.PublicApiIsShippedPropertyBagKey] == "true";
                            string fileName = diagnostic.Properties[DeclarePublicApiAnalyzer.FileName];

                            if (!allChanges.TryGetValue(fileName, out var mapToUpdate))
                            {
                                mapToUpdate = new();
                                allChanges.Add(fileName, mapToUpdate);
                            }

                            mapToUpdate[oldName] = newName;
                        }
                    }

                    foreach (var (path, changes) in allChanges)
                    {
                        var doc = project.GetPublicApiDocument(path);

                        if (doc is not null)
                        {
                            var text = await doc.GetTextAsync(cancellationToken).ConfigureAwait(false);
                            SourceText newShippedSourceText = AnnotateSymbolNamesInSourceText(text, changes);
                            updatedPublicSurfaceAreaText.Add((doc.Id, newShippedSourceText));
                        }
                    }
                }

                Solution newSolution = _solution;

                foreach (var (docId, text) in updatedPublicSurfaceAreaText)
                {
                    newSolution = newSolution.WithAdditionalDocumentText(docId, text);
                }

                return newSolution;
            }
        }

        private class PublicSurfaceAreaFixAllProvider : FixAllProvider
        {
            public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
            {
                var diagnosticsToFix = new List<KeyValuePair<Project, ImmutableArray<Diagnostic>>>();
                string? title;
                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        {
                            ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                            title = string.Format(CultureInfo.InvariantCulture, PublicApiAnalyzerResources.AddAllItemsInDocumentToThePublicApiTitle, fixAllContext.Document.Name);
                            break;
                        }

                    case FixAllScope.Project:
                        {
                            Project project = fixAllContext.Project;
                            ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                            title = string.Format(CultureInfo.InvariantCulture, PublicApiAnalyzerResources.AddAllItemsInProjectToThePublicApiTitle, fixAllContext.Project.Name);
                            break;
                        }

                    case FixAllScope.Solution:
                        {
                            foreach (Project project in fixAllContext.Solution.Projects)
                            {
                                ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                                diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));
                            }

                            title = PublicApiAnalyzerResources.AddAllItemsInTheSolutionToThePublicApiTitle;
                            break;
                        }

                    case FixAllScope.Custom:
                        return null;

                    default:
                        Debug.Fail($"Unknown FixAllScope '{fixAllContext.Scope}'");
                        return null;
                }

                return new FixAllAdditionalDocumentChangeAction(title, fixAllContext.Solution, diagnosticsToFix);
            }
        }
    }
}
