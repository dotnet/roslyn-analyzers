// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Xunit;

namespace Test.Utilities
{
    public abstract class CodeFixTestBase : DiagnosticAnalyzerTestBase
    {
        protected abstract CodeFixProvider GetCSharpCodeFixProvider();

        protected abstract CodeFixProvider GetBasicCodeFixProvider();

        protected void VerifyCSharpUnsafeCodeFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false)
        {
            var runner = new CodeFixRunner(GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), DefaultTestValidationMode);
            VerifyFix(LanguageNames.CSharp, runner, new[] { oldSource }, new[] { newSource }, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, allowUnsafeCode: true);
        }

        protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            var runner = new CodeFixRunner(GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), validationMode);
            VerifyFix(LanguageNames.CSharp, runner, new[] { oldSource }, new[] { newSource }, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, allowUnsafeCode: false);
        }

        protected void VerifyCSharpFix(string[] oldSources, string[] newSources, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            var runner = new CodeFixRunner(GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(),  validationMode);
            VerifyFix(LanguageNames.CSharp, runner, oldSources, newSources, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, allowUnsafeCode: false);
        }

        protected void VerifyCSharpFixAll(string oldSource, string newSource, bool allowNewCompilerDiagnostics = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            var runner = new CodeFixRunner(GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), validationMode: validationMode);
            VerifyFixAll(LanguageNames.CSharp, runner, new[] { oldSource }, new[] { newSource }, allowNewCompilerDiagnostics, allowUnsafeCode: false);
        }

        protected void VerifyBasicFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            var runner = new CodeFixRunner(GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), validationMode: validationMode);
            VerifyFix(LanguageNames.VisualBasic, runner, new[] { oldSource }, new[] { newSource }, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, allowUnsafeCode: false);
        }

        protected void VerifyBasicFix(string[] oldSources, string[] newSources, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            var runner = new CodeFixRunner(GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), validationMode: validationMode);
            VerifyFix(LanguageNames.VisualBasic, runner, oldSources, newSources, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, allowUnsafeCode: false);
        }

        protected void VerifyBasicFixAll(string oldSource, string newSource, bool allowNewCompilerDiagnostics = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            var runner = new CodeFixRunner(GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), validationMode: validationMode);
            VerifyFixAll(LanguageNames.VisualBasic, runner, new[] { oldSource }, new[] { newSource }, allowNewCompilerDiagnostics, allowUnsafeCode: false);
        }

        private void VerifyFix(string language, CodeFixRunner runner, string[] oldSources, string[] newSources, int? codeFixIndex, bool allowNewCompilerDiagnostics, bool onlyFixFirstFixableDiagnostic, bool allowUnsafeCode)
        {
            Assert.True(oldSources.Length == newSources.Length, "Length of expected and actual sources arrays must match.");
            Document[] documents = CreateDocuments(oldSources, language, allowUnsafeCode: allowUnsafeCode);

            var project = documents.First().Project;
            Solution newSolution;
            if (onlyFixFirstFixableDiagnostic || codeFixIndex.HasValue)
            {
                newSolution = runner.ApplySingleFix(project, ImmutableArray<TestAdditionalDocument>.Empty, codeFixIndex.HasValue ? codeFixIndex.Value : 0, fixableDiagnosticIndex: 0);
            }
            else
            {
                newSolution = runner.ApplyFixesOneByOne(project.Solution, ImmutableArray<TestAdditionalDocument>.Empty, allowNewCompilerDiagnostics);
            }

            VerifyDocuments(newSolution, documents, newSources);
        }

        private void VerifyFixAll(string language, CodeFixRunner runner, string[] oldSources, string[] newSources, bool allowNewCompilerDiagnostics, bool allowUnsafeCode)
        {
            Assert.True(oldSources.Length == newSources.Length, "Length of expected and actual sources arrays must match.");
            Document[] documents = CreateDocuments(oldSources, language, allowUnsafeCode: allowUnsafeCode);
            var solution = documents.First().Project.Solution;
            solution = runner.ApplyFixAll(solution, ImmutableArray<TestAdditionalDocument>.Empty, allowNewCompilerDiagnostics);
            VerifyDocuments(solution, documents, newSources);
        }

        protected void VerifyAdditionalFileFix(string language, DiagnosticAnalyzer analyzerOpt, CodeFixProvider codeFixProvider, string source,
            IEnumerable<TestAdditionalDocument> additionalFiles, TestAdditionalDocument newAdditionalFileToVerify, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false)
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

            Solution newSolution;
            var runner = new CodeFixRunner(analyzerOpt, codeFixProvider, DefaultTestValidationMode);
            if (onlyFixFirstFixableDiagnostic || codeFixIndex.HasValue)
            {
                newSolution = runner.ApplySingleFix(document.Project, additionalFiles, codeFixIndex.HasValue ? codeFixIndex.Value : 0, fixableDiagnosticIndex: 0);
            }
            else
            {
                newSolution = runner.ApplyFixesOneByOne(document.Project.Solution, additionalFiles, allowNewCompilerDiagnostics);
            }

            VerifyDocuments(newSolution, new[] { document }, new[] { additionalFileText });
        }

        private void VerifyDocuments(Solution solution, Document[] documents, string[] newSources)
        {
            string[] newSourceFileNames = documents.Select(d => d.Name).ToArray();
            DocumentId[] documentIds = documents.Select(d => d.Id).ToArray();

            for (int i = 0; i < documentIds.Length; i++)
            {
                var document = solution.GetDocument(documentIds[i]);
                var actualText = GetActualTextForNewDocument(document, newSourceFileNames[i]);
                Assert.Equal(newSources[i], actualText.ToString());
            }
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
                    throw new Exception($"Unable to find document with name {newSourceFileName} in new workspace after applying fix.");
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
    }
}
