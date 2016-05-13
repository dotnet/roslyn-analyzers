// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public abstract class CodeFixTestBase : DiagnosticAnalyzerTestBase
    {
        protected abstract CodeFixProvider GetCSharpCodeFixProvider();

        protected abstract CodeFixProvider GetBasicCodeFixProvider();

        protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
        }

        protected void VerifyBasicFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyFix(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
        }

        protected void VerifyCSharpAdditionalFileFix(string source, IEnumerable<TestAdditionalDocument> additionalFiles, TestAdditionalDocument newAdditionalFileToVerify, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyAdditionalFileFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), source, additionalFiles, newAdditionalFileToVerify, codeFixIndex, allowNewCompilerDiagnostics);
        }

        protected void VerifyBasicAdditionalFileFix(string source, IEnumerable<TestAdditionalDocument> additionalFiles, TestAdditionalDocument newAdditionalFileToVerify, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyAdditionalFileFix(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), source, additionalFiles, newAdditionalFileToVerify, codeFixIndex, allowNewCompilerDiagnostics);
        }

        protected void VerifyFix(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics)
        {
            Document document = CreateDocument(oldSource, language);
            var newSourceFileName = document.Name;

            VerifyFix(document, analyzer, codeFixProvider, newSource, newSourceFileName, ImmutableArray<TestAdditionalDocument>.Empty, codeFixIndex, allowNewCompilerDiagnostics);
        }

        protected void VerifyAdditionalFileFix(
            string language,
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            string source,
            IEnumerable<TestAdditionalDocument> additionalFiles,
            TestAdditionalDocument newAdditionalFileToVerify,
            int? codeFixIndex = null,
            bool allowNewCompilerDiagnostics = false)
        {
            Document document = CreateDocument(source, language);
            if (additionalFiles != null)
            {
                var project = document.Project;
                foreach (var additionalFile in additionalFiles)
                {
                    project = project.AddAdditionalDocument(additionalFile.Name, additionalFile.GetText(), filePath: additionalFile.Path).Project;
                }

                document = project.GetDocument(document.Id);
            }

            var additionalFileName = newAdditionalFileToVerify.Name;
            var additionalFileText = newAdditionalFileToVerify.GetText().ToString();

            VerifyFix(document, analyzer, codeFixProvider, additionalFileText, additionalFileName, additionalFiles, codeFixIndex, allowNewCompilerDiagnostics);
        }

        private void VerifyFix(
            Document document,
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            string newSource,
            string newSourceFileName,
            IEnumerable<TestAdditionalDocument> additionalFiles,
            int? codeFixIndex,
            bool allowNewCompilerDiagnostics)
        {
            var fixableDiagnosticIds = codeFixProvider.FixableDiagnosticIds.ToSet();
            Func<IEnumerable<Diagnostic>, ImmutableArray<Diagnostic>> filterDiagnostics = diags =>
                diags.Where(d => fixableDiagnosticIds.Contains(d.Id)).ToImmutableArrayOrEmpty();

            var analyzerDiagnostics = filterDiagnostics(GetSortedDiagnostics(analyzer, document, additionalFiles: additionalFiles));
            var compilerDiagnostics = document.GetSemanticModelAsync().Result.GetDiagnostics();

            int attempts = analyzerDiagnostics.Length;

            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();
                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    document = document.Apply(actions.ElementAt((int)codeFixIndex));
                    break;
                }

                document = document.Apply(actions.ElementAt(0));
                additionalFiles = document.Project.AdditionalDocuments.Select(a => new TestAdditionalDocument(a));

                analyzerDiagnostics = filterDiagnostics(GetSortedDiagnostics(analyzer, document, additionalFiles: additionalFiles));
                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, document.GetSemanticModelAsync().Result.GetDiagnostics());
                if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                {
                    // Format and get the compiler diagnostics again so that the locations make sense in the output
                    document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, document.GetSemanticModelAsync().Result.GetDiagnostics());

                    Assert.True(false,
                        string.Format("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                            newCompilerDiagnostics.Select(d => d.ToString()).Join("\r\n"),
                            document.GetSyntaxRootAsync().Result.ToFullString()));
                }

                if (analyzerDiagnostics.IsEmpty)
                {
                    break;
                }
            }

            var actualText = GetActualTextForNewDocument(document, newSourceFileName);
            Assert.Equal(newSource, actualText.ToString());
        }

        private static SourceText GetActualTextForNewDocument(Document documentInNewWorkspace, string newSourceFileName)
        {
            TextDocument newDocument = documentInNewWorkspace;
            if (documentInNewWorkspace.Name != newSourceFileName)
            {
                newDocument = documentInNewWorkspace.Project.Documents.FirstOrDefault(d => d.Name == newSourceFileName) ??
                    documentInNewWorkspace.Project.AdditionalDocuments.FirstOrDefault(d => d.Name == newSourceFileName);

                if (newDocument == null)
                {
                    throw new System.Exception($"Unable to find document with name {newSourceFileName} in new workspace after applying fix.");
                }
            }

            if (((newDocument as Document)?.SupportsSyntaxTree).GetValueOrDefault())
            {
                var newSourceDocument = (Document)newDocument;
                newSourceDocument = Simplifier.ReduceAsync(newSourceDocument, Simplifier.Annotation).Result;
                SyntaxNode root = newSourceDocument.GetSyntaxRootAsync().Result;
                root = Formatter.Format(root, Formatter.Annotation, newSourceDocument.Project.Solution.Workspace);
                return root.GetText();
            }
            else
            {
                return newDocument.GetTextAsync(CancellationToken.None).Result;
            }
        }

        private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
        {
            Diagnostic[] oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
            Diagnostic[] newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

            int oldIndex = 0;
            int newIndex = 0;

            while (newIndex < newArray.Length)
            {
                if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
                {
                    ++oldIndex;
                    ++newIndex;
                }
                else
                {
                    yield return newArray[newIndex++];
                }
            }
        }

        private static Diagnostic[] GetSortedDiagnostics(DiagnosticAnalyzer analyzer, Document document, TextSpan? span = null)
        {
            TextSpan spanToTest = span.HasValue ? span.Value : document.GetSyntaxRootAsync().Result.FullSpan;
            IEnumerable<Diagnostic> diagnostics = GetDiagnosticsUsingAnalyzerDriver(analyzer, document, span);
            return GetSortedDiagnostics(diagnostics);
        }

        private static IEnumerable<Diagnostic> GetDiagnosticsUsingAnalyzerDriver(DiagnosticAnalyzer analyzer, Document document, TextSpan? span)
        {
            SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
            Compilation compilation = semanticModel.Compilation;
            var diagnostics = new List<Diagnostic>();
            AnalyzeDocumentCore(analyzer, document, diagnostics.Add, span);
            return diagnostics;
        }
    }
}
