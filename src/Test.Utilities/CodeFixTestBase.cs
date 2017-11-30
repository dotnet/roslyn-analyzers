// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
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

        protected void VerifyCSharpUnsafeCodeFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, bool testFixAll = true)
        {
            VerifyFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, validationMode: DefaultTestValidationMode, testFixAll: testFixAll, allowUnsafeCode: true);
        }

        protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, TestValidationMode validationMode = DefaultTestValidationMode, bool testFixAll = true)
        {
            VerifyFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, validationMode, testFixAll, allowUnsafeCode: false);
        }

        protected void VerifyCSharpFixAll(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            VerifyFixAll(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics, validationMode, allowUnsafeCode: false);
        }

        protected void VerifyBasicFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, bool testFixAll = true, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            VerifyFix(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, validationMode, testFixAll, allowUnsafeCode: false);
        }

        protected void VerifyBasicFixAll(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false, TestValidationMode validationMode = DefaultTestValidationMode)
        {
            VerifyFixAll(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics, validationMode, allowUnsafeCode: false);
        }

        private void VerifyFix(string language, DiagnosticAnalyzer analyzerOpt, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics, bool onlyFixFirstFixableDiagnostic, TestValidationMode validationMode, bool testFixAll, bool allowUnsafeCode)
        {
            Document document = CreateDocument(oldSource, language, allowUnsafeCode: allowUnsafeCode);
            var newSourceFileName = document.Name;

            VerifyFix(document, analyzerOpt, codeFixProvider, newSource, newSourceFileName, ImmutableArray<TestAdditionalDocument>.Empty, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, validationMode, testFixAll);
        }

        private void VerifyFixAll(string language, DiagnosticAnalyzer analyzerOpt, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics, TestValidationMode validationMode, bool allowUnsafeCode)
        {
            Document document = CreateDocument(oldSource, language, allowUnsafeCode: allowUnsafeCode);
            var newSourceFileName = document.Name;
            var additionalFiles = ImmutableArray<TestAdditionalDocument>.Empty;
            
            VerifyFixOrFixAllCore(document, analyzerOpt, codeFixProvider, newSource, newSourceFileName, additionalFiles,
                codeFixIndex, allowNewCompilerDiagnostics, validationMode, testFixAll: true, applySingleFixOrFixAll: true);
        }

        protected void VerifyAdditionalFileFix(string language, DiagnosticAnalyzer analyzerOpt, CodeFixProvider codeFixProvider, string source,
            IEnumerable<TestAdditionalDocument> additionalFiles, TestAdditionalDocument newAdditionalFileToVerify, int? codeFixIndex = null,
            bool allowNewCompilerDiagnostics = false, bool onlyFixFirstFixableDiagnostic = false, bool testFixAll = true)
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

            VerifyFix(document, analyzerOpt, codeFixProvider, additionalFileText, additionalFileName, additionalFiles, codeFixIndex, allowNewCompilerDiagnostics, onlyFixFirstFixableDiagnostic, DefaultTestValidationMode, testFixAll);
        }

        private void VerifyFix(
            Document document,
            DiagnosticAnalyzer analyzerOpt,
            CodeFixProvider codeFixProvider,
            string newSource,
            string newSourceFileName,
            IEnumerable<TestAdditionalDocument> additionalFiles,
            int? codeFixIndex,
            bool allowNewCompilerDiagnostics,
            bool onlyFixFirstFixableDiagnostic,
            TestValidationMode validationMode,
            bool testFixAll)
        {
            // Verify code fix.
            VerifyFixOrFixAllCore(document, analyzerOpt, codeFixProvider, newSource, newSourceFileName, additionalFiles,
                codeFixIndex, allowNewCompilerDiagnostics, validationMode, testFixAll: false, applySingleFixOrFixAll: onlyFixFirstFixableDiagnostic);

            // Also verify FixAll.
            if (testFixAll && codeFixProvider.GetFixAllProvider() != null)
            {
                VerifyFixOrFixAllCore(document, analyzerOpt, codeFixProvider, newSource, newSourceFileName, additionalFiles,
                    codeFixIndex, allowNewCompilerDiagnostics, validationMode, testFixAll: true, applySingleFixOrFixAll: true);
            }
        }

        private void VerifyFixOrFixAllCore(
            Document document,
            DiagnosticAnalyzer analyzerOpt,
            CodeFixProvider codeFixProvider,
            string newSource,
            string newSourceFileName,
            IEnumerable<TestAdditionalDocument> additionalFiles,
            int? codeFixIndex,
            bool allowNewCompilerDiagnostics,
            TestValidationMode validationMode,
            bool testFixAll,
            bool applySingleFixOrFixAll)
        {
            var fixableDiagnosticIds = codeFixProvider.FixableDiagnosticIds.ToSet();
            Func<IEnumerable<Diagnostic>, ImmutableArray<Diagnostic>> getFixableDiagnostics = diags =>
                diags.Where(d => fixableDiagnosticIds.Contains(d.Id)).ToImmutableArrayOrEmpty();

            var analyzerDiagnostics = GetSortedDiagnostics(analyzerOpt, new[] { document }, additionalFiles: additionalFiles, validationMode: validationMode);
            var compilerDiagnostics = document.GetSemanticModelAsync().Result.GetDiagnostics();
            var fixableDiagnostics = getFixableDiagnostics(analyzerDiagnostics.Concat(compilerDiagnostics));

            var diagnosticIndexToFix = 0;
            while (diagnosticIndexToFix < fixableDiagnostics.Length)
            {
                var actions = new List<CodeAction>();
                Diagnostic triggerDiagnostic = fixableDiagnostics[diagnosticIndexToFix];

                var context = new CodeFixContext(document, triggerDiagnostic, (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();
                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex == null)
                {
                    codeFixIndex = 0;
                }

                if (codeFixIndex >= actions.Count)
                {
                    throw new Exception($"Unable to invoke code fix at index '{codeFixIndex.Value}', only '{actions.Count}' code fixes were registered.");
                }

                var codeAction = actions.ElementAt(codeFixIndex.Value);

                if (!testFixAll)
                {
                    document = document.Apply(codeAction);
                }
                else
                {
                    string diagnosticIdToFix = triggerDiagnostic.Id;
                    var diagnosticsToFix = fixableDiagnostics.Where(d => d.Id == diagnosticIdToFix);
                    document = ApplyFixAll(document, codeAction, codeFixProvider, codeFixProvider.GetFixAllProvider(), diagnosticIdToFix, diagnosticsToFix);
                }

                if (applySingleFixOrFixAll)
                {
                    break;
                }

                additionalFiles = document.Project.AdditionalDocuments.Select(a => new TestAdditionalDocument(a));
                analyzerDiagnostics = GetSortedDiagnostics(analyzerOpt, new[] { document }, additionalFiles: additionalFiles, validationMode: validationMode);

                var updatedCompilerDiagnostics = document.GetSemanticModelAsync().Result.GetDiagnostics();
                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, updatedCompilerDiagnostics);
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

                var newFixableDiagnostics = getFixableDiagnostics(analyzerDiagnostics.Concat(updatedCompilerDiagnostics));
                if (fixableDiagnostics.SetEquals(newFixableDiagnostics, DiagnosticComparer.Instance))
                {
                    diagnosticIndexToFix++;
                }
                else
                {
                    fixableDiagnostics = newFixableDiagnostics;
                }
            }

            var actualText = GetActualTextForNewDocument(document, newSourceFileName);
            Assert.Equal(newSource, actualText.ToString());
        }

        private Document ApplyFixAll(
            Document document,
            CodeAction codeAction,
            CodeFixProvider codeFixProvider,
            FixAllProvider fixAllProvider,
            string diagnosticIdToFix,
            IEnumerable<Diagnostic> fixableDiagnostics)
        {
            var diagnosticProvider = new FixAllDiagnosticProvider(fixableDiagnostics);
            IEnumerable<string> fixableDiagnosticIds = new string[] { diagnosticIdToFix };
            var fixAllContext = new FixAllContext(document, codeFixProvider, FixAllScope.Document, codeAction.EquivalenceKey, fixableDiagnosticIds, diagnosticProvider, CancellationToken.None);
            var fixAllCodeAction = fixAllProvider.GetFixAsync(fixAllContext).Result;
            return fixAllCodeAction != null ? document.Apply(fixAllCodeAction) : document;
        }

        private sealed class DiagnosticComparer : IEqualityComparer<Diagnostic>
        {
            internal static readonly DiagnosticComparer Instance = new DiagnosticComparer();

            public bool Equals(Diagnostic x, Diagnostic y)
            {
                return x.Id == y.Id &&
                    x.GetMessage() == y.GetMessage() &&
                    x.Location.IsInSource == y.Location.IsInSource &&
                    x.Location.SourceSpan == y.Location.SourceSpan &&
                    (x.Location.SourceTree?.IsEquivalentTo(y.Location.SourceTree)).GetValueOrDefault();
            }

            public int GetHashCode(Diagnostic obj)
            {
                return Hash.CombineValues(new[] {
                    obj.Id.GetHashCode(),
                    obj.GetMessage().GetHashCode(),
                    obj.Location.IsInSource ? 1 : 0,
                    obj.Location.SourceSpan.GetHashCode(),
                    obj.Location.SourceTree?.ToString().GetHashCode() });
            }
        }

        private class FixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
        {
            private IEnumerable<Diagnostic> _fixableDiagnostics;

            public FixAllDiagnosticProvider(IEnumerable<Diagnostic> fixableDiagnostics)
            {
                _fixableDiagnostics = fixableDiagnostics;
            }

            public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
                => Task.FromResult(_fixableDiagnostics);

            public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
                => Task.FromResult(_fixableDiagnostics);

            public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
                => Task.FromResult(_fixableDiagnostics);
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
    }
}
