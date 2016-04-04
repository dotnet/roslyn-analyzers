// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;
using TestResources.NetFX;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public abstract class DiagnosticAnalyzerTestBase
    {
        private static readonly MetadataReference s_corlibReference = MetadataReference.CreateFromAssemblyInternal(typeof(object).Assembly);
        private static readonly MetadataReference s_systemCoreReference = MetadataReference.CreateFromAssemblyInternal(typeof(Enumerable).Assembly);
        private static readonly MetadataReference s_systemXmlReference = MetadataReference.CreateFromAssemblyInternal(typeof(System.Xml.XmlDocument).Assembly);
        private static readonly MetadataReference s_systemXmlDataReference = MetadataReference.CreateFromAssemblyInternal(typeof(System.Data.Rule).Assembly);
        private static readonly MetadataReference s_CSharpSymbolsReference = MetadataReference.CreateFromAssemblyInternal(typeof(CSharpCompilation).Assembly);
        private static readonly MetadataReference s_visualBasicSymbolsReference = MetadataReference.CreateFromAssemblyInternal(typeof(VisualBasicCompilation).Assembly);
        private static readonly MetadataReference s_codeAnalysisReference = MetadataReference.CreateFromAssemblyInternal(typeof(Compilation).Assembly);
        private static readonly MetadataReference s_workspacesReference = MetadataReference.CreateFromAssemblyInternal(typeof(Workspace).Assembly);
        private static readonly MetadataReference s_immutableCollectionsReference = MetadataReference.CreateFromAssemblyInternal(typeof(ImmutableArray<int>).Assembly);
        private static readonly MetadataReference s_systemDiagnosticsDebugReference = MetadataReference.CreateFromAssemblyInternal(typeof(System.Diagnostics.Debug).Assembly);
        private static readonly MetadataReference s_systemDataReference = MetadataReference.CreateFromAssemblyInternal(typeof(System.Data.DataSet).Assembly);
        private static readonly CompilationOptions s_CSharpDefaultOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        private static readonly CompilationOptions s_visualBasicDefaultOptions = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        internal static readonly string DefaultFilePathPrefix = "Test";
        internal static readonly string CSharpDefaultFileExt = "cs";
        internal static readonly string VisualBasicDefaultExt = "vb";
        internal static readonly string CSharpDefaultFilePath = DefaultFilePathPrefix + 0 + "." + CSharpDefaultFileExt;
        internal static readonly string VisualBasicDefaultFilePath = DefaultFilePathPrefix + 0 + "." + VisualBasicDefaultExt;

        private const string _testProjectName = "TestProject";

        protected abstract DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer();
        protected abstract DiagnosticAnalyzer GetBasicDiagnosticAnalyzer();

        private static MetadataReference s_systemRuntimeFacadeRef;
        public static MetadataReference SystemRuntimeFacadeRef
        {
            get
            {
                if (s_systemRuntimeFacadeRef == null)
                {
                    s_systemRuntimeFacadeRef = AssemblyMetadata.CreateFromImage(ReferenceAssemblies_V45_Facades.System_Runtime).GetReference(display: "System.Runtime.dll");
                }

                return s_systemRuntimeFacadeRef;
            }
        }

        private static MetadataReference s_systemThreadingFacadeRef;
        public static MetadataReference SystemThreadingFacadeRef
        {
            get
            {
                if (s_systemThreadingFacadeRef == null)
                {
                    s_systemThreadingFacadeRef = AssemblyMetadata.CreateFromImage(ReferenceAssemblies_V45_Facades.System_Threading).GetReference(display: "System.Threading.dll");
                }

                return s_systemThreadingFacadeRef;
            }
        }

        private static MetadataReference s_systemThreadingTasksFacadeRef;
        public static MetadataReference SystemThreadingTaskFacadeRef
        {
            get
            {
                if (s_systemThreadingTasksFacadeRef == null)
                {
                    s_systemThreadingTasksFacadeRef = AssemblyMetadata.CreateFromImage(ReferenceAssemblies_V45_Facades.System_Threading_Tasks).GetReference(display: "System.Threading.Tasks.dll");
                }

                return s_systemThreadingTasksFacadeRef;
            }
        }

        protected bool PrintActualDiagnosticsOnFailure { private get; set; }

        // It is assumed to be of the format, Get<RuleId>CSharpResultAt(line: {0}, column: {1}, message: {2})
        public string ExpectedDiagnosticsAssertionTemplate { private get; set; }

        protected static DiagnosticResult GetGlobalResult(string id, string message)
        {
            return new DiagnosticResult
            {
                Id = id,
                Severity = DiagnosticSeverity.Warning,
                Message = message
            };
        }

        protected static DiagnosticResult GetGlobalResult(DiagnosticDescriptor rule, params string[] messageArguments)
        {
            return new DiagnosticResult
            {
                Id = rule.Id,
                Severity = rule.DefaultSeverity,
                Message = String.Format(rule.MessageFormat.ToString(), messageArguments)
            };
        }

        protected static DiagnosticResult GetBasicResultAt(int line, int column, string id, string message)
        {
            return GetResultAt(VisualBasicDefaultFilePath, line, column, id, message);
        }

        protected static DiagnosticResult GetBasicResultAt(string id, string message, params string[] locationStrings)
        {
            return GetResultAt(VisualBasicDefaultFilePath, id, message, locationStrings);
        }

        protected static DiagnosticResult GetBasicResultAt(int line, int column, DiagnosticDescriptor rule, params object[] messageArguments)
        {
            return GetResultAt(VisualBasicDefaultFilePath, line, column, rule, messageArguments);
        }

        protected static DiagnosticResult GetCSharpResultAt(int line, int column, string id, string message)
        {
            return GetResultAt(CSharpDefaultFilePath, line, column, id, message);
        }

        protected static DiagnosticResult GetCSharpResultAt(string id, string message, params string[] locationStrings)
        {
            return GetResultAt(CSharpDefaultFilePath, id, message, locationStrings);
        }

        protected static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, params object[] messageArguments)
        {
            return GetResultAt(CSharpDefaultFilePath, line, column, rule, messageArguments);
        }

        protected static DiagnosticResult GetResultAt(string path, int line, int column, string id, string message)
        {
            var location = new DiagnosticResultLocation(path, line, column);

            return new DiagnosticResult
            {
                Locations = new[] { location },
                Id = id,
                Severity = DiagnosticSeverity.Warning,
                Message = message
            };
        }

        protected static DiagnosticResult GetResultAt(string defaultPath, string id, string message, params string[] locationStrings)
        {
            return new DiagnosticResult
            {
                Locations = ParseResultLocations(defaultPath, locationStrings),
                Id = id,
                Severity = DiagnosticSeverity.Warning,
                Message = message
            };
        }

        protected static DiagnosticResult GetResultAt(string path, int line, int column, DiagnosticDescriptor rule, params object[] messageArguments)
        {
            var location = new DiagnosticResultLocation(path, line, column);

            return new DiagnosticResult
            {
                Locations = new[] { location },
                Id = rule.Id,
                Severity = rule.DefaultSeverity,
                Message = string.Format(rule.MessageFormat.ToString(), messageArguments)
            };
        }

        protected static DiagnosticResultLocation[] ParseResultLocations(string defaultPath, string[] locationStrings)
        {
            var builder = new List<DiagnosticResultLocation>();

            foreach (string str in locationStrings)
            {
                string[] tokens = str.Split('(', ',', ')');
                Assert.True(tokens.Length == 4, "Location string must be of the format 'FileName.cs(line,column)' or just 'line,column' to use " + defaultPath + " as the file name.");

                string path = tokens[0] == "" ? defaultPath : tokens[0];

                int line;
                Assert.True(int.TryParse(tokens[1], out line) && line >= -1, "Line must be >= -1 in location string: " + str);

                int column;
                Assert.True(int.TryParse(tokens[2], out column) && line >= -1, "Column must be >= -1 in location string: " + str);

                builder.Add(new DiagnosticResultLocation(path, line, column));
            }

            return builder.ToArray();
        }

        protected void VerifyCSharp(string source, params DiagnosticResult[] expected)
        {
            Verify(source, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected);
        }

        protected void VerifyCSharp(string source, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            Verify(source, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), addLanguageSpecificCodeAnalysisReference, expected);
        }

        protected void VerifyBasic(string source, params DiagnosticResult[] expected)
        {
            Verify(source, LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), expected);
        }

        protected void VerifyBasic(string source, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            Verify(source, LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), addLanguageSpecificCodeAnalysisReference, expected);
        }

        protected void Verify(string source, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
        {
            Verify(new[] { source }, language, analyzer, expected);
        }

        protected void Verify(string source, string language, DiagnosticAnalyzer analyzer, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            Verify(new[] { source }, language, analyzer, addLanguageSpecificCodeAnalysisReference, expected);
        }

        protected void VerifyBasic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyBasic(sources.ToFileAndSource(), expected);
        }

        protected void VerifyBasic(FileAndSource[] sources, params DiagnosticResult[] expected)
        {
            Verify(sources, LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), expected);
        }

        protected void VerifyBasic(string[] sources, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            Verify(sources, LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), addLanguageSpecificCodeAnalysisReference, expected);
        }

        protected void VerifyCSharp(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyCSharp(sources.ToFileAndSource(), expected);
        }

        protected void VerifyCSharp(FileAndSource[] sources, params DiagnosticResult[] expected)
        {
            Verify(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected);
        }

        protected void VerifyCSharp(string[] sources, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            Verify(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), addLanguageSpecificCodeAnalysisReference, expected);
        }

        protected void Verify(string[] sources, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
        {
            Verify(sources.ToFileAndSource(), language, analyzer, expected);
        }

        protected void Verify(FileAndSource[] sources, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
        {
            GetSortedDiagnostics(sources, language, analyzer).Verify(analyzer, PrintActualDiagnosticsOnFailure, ExpectedDiagnosticsAssertionTemplate, expected);
        }

        protected void Verify(string[] sources, string language, DiagnosticAnalyzer analyzer, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            Verify(sources.ToFileAndSource(), language, analyzer, addLanguageSpecificCodeAnalysisReference, expected);
        }

        protected void Verify(FileAndSource[] sources, string language, DiagnosticAnalyzer analyzer, bool addLanguageSpecificCodeAnalysisReference, params DiagnosticResult[] expected)
        {
            GetSortedDiagnostics(sources, language, analyzer, addLanguageSpecificCodeAnalysisReference).Verify(analyzer, PrintActualDiagnosticsOnFailure, ExpectedDiagnosticsAssertionTemplate, expected);
        }

        protected static Diagnostic[] GetSortedDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer, bool addLanguageSpecificCodeAnalysisReference = true)
        {
            return GetSortedDiagnostics(sources.ToFileAndSource(), language, analyzer, addLanguageSpecificCodeAnalysisReference);
        }

        protected static Diagnostic[] GetSortedDiagnostics(FileAndSource[] sources, string language, DiagnosticAnalyzer analyzer, bool addLanguageSpecificCodeAnalysisReference = true, string projectName = _testProjectName)
        {
            Tuple<Document[], bool, TextSpan?[]> documentsAndUseSpan = GetDocumentsAndSpans(sources, language, addLanguageSpecificCodeAnalysisReference, projectName);
            Document[] documents = documentsAndUseSpan.Item1;
            bool useSpans = documentsAndUseSpan.Item2;
            TextSpan?[] spans = documentsAndUseSpan.Item3;
            return GetSortedDiagnostics(analyzer, documents, useSpans ? spans : null);
        }

        protected static Tuple<Document[], bool, TextSpan?[]> GetDocumentsAndSpans(string[] sources, string language, bool addLanguageSpecificCodeAnalysisReference = true)
        {
            return GetDocumentsAndSpans(sources.ToFileAndSource(), language, addLanguageSpecificCodeAnalysisReference);
        }

        protected static Tuple<Document[], bool, TextSpan?[]> GetDocumentsAndSpans(FileAndSource[] sources, string language, bool addLanguageSpecificCodeAnalysisReference = true, string projectName = _testProjectName)
        {
            Assert.True(language == LanguageNames.CSharp || language == LanguageNames.VisualBasic, "Unsupported language");

            var spans = new TextSpan?[sources.Length];
            bool useSpans = false;

            for (int i = 0; i < sources.Length; i++)
            {
                string fileName = language == LanguageNames.CSharp ? "Test" + i + ".cs" : "Test" + i + ".vb";

                string source;
                int? pos;
                TextSpan? span;
                MarkupTestFile.GetPositionAndSpan(sources[i].Source, out source, out pos, out span);

                sources[i].Source = source;
                spans[i] = span;

                if (span != null)
                {
                    useSpans = true;
                }
            }

            Project project = CreateProject(sources, language, addLanguageSpecificCodeAnalysisReference, null, projectName);
            Document[] documents = project.Documents.ToArray();
            Assert.Equal(sources.Length, documents.Length);

            return Tuple.Create(documents, useSpans, spans);
        }

        protected static Document CreateDocument(string source, string language = LanguageNames.CSharp, bool addLanguageSpecificCodeAnalysisReference = true)
        {
            return CreateProject(new[] { source }, language, addLanguageSpecificCodeAnalysisReference).Documents.First();
        }

        protected static Project CreateProject(string[] sources, string language = LanguageNames.CSharp, bool addLanguageSpecificCodeAnalysisReference = true, Solution addToSolution = null)
        {
            return CreateProject(sources.ToFileAndSource(), language, addLanguageSpecificCodeAnalysisReference, addToSolution);
        }

        protected static Project CreateProject(
            FileAndSource[] sources,
            string language = LanguageNames.CSharp,
            bool addLanguageSpecificCodeAnalysisReference = true,
            Solution addToSolution = null,
            string projectName = _testProjectName)
        {
            string fileNamePrefix = DefaultFilePathPrefix;
            string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;
            CompilationOptions options = language == LanguageNames.CSharp ? s_CSharpDefaultOptions : s_visualBasicDefaultOptions;

            ProjectId projectId = ProjectId.CreateNewId(debugName: projectName);

            Project project = (addToSolution ?? new AdhocWorkspace().CurrentSolution)
                .AddProject(projectId, projectName, projectName, language)
                .AddMetadataReference(projectId, s_corlibReference)
                .AddMetadataReference(projectId, s_systemCoreReference)
                .AddMetadataReference(projectId, s_systemXmlReference)
                .AddMetadataReference(projectId, s_systemXmlDataReference)
                .AddMetadataReference(projectId, s_codeAnalysisReference)
                .AddMetadataReference(projectId, SystemRuntimeFacadeRef)
                .AddMetadataReference(projectId, SystemThreadingFacadeRef)
                .AddMetadataReference(projectId, SystemThreadingTaskFacadeRef)
                //.AddMetadataReference(projectId, TestBase.SystemRef)
                //.AddMetadataReference(projectId, TestBase.SystemRuntimeFacadeRef)
                //.AddMetadataReference(projectId, TestBase.SystemThreadingFacadeRef)
                //.AddMetadataReference(projectId, TestBase.SystemThreadingTaskFacadeRef)
                .AddMetadataReference(projectId, s_immutableCollectionsReference)
                .AddMetadataReference(projectId, s_workspacesReference)
                .AddMetadataReference(projectId, s_systemDiagnosticsDebugReference)
                .AddMetadataReference(projectId, s_systemDataReference)
                .WithProjectCompilationOptions(projectId, options)
                .GetProject(projectId);

            // Enable IOperation Feature on the project
            var parseOptions = project.ParseOptions.WithFeatures(project.ParseOptions.Features.Concat(SpecializedCollections.SingletonEnumerable(KeyValuePair.Create("IOperation", "true"))));
            project = project.WithParseOptions(parseOptions);

            if (addLanguageSpecificCodeAnalysisReference)
            {
                MetadataReference symbolsReference = language == LanguageNames.CSharp ? s_CSharpSymbolsReference : s_visualBasicSymbolsReference;
                project = project.AddMetadataReference(symbolsReference);
            }

            int count = 0;
            foreach (FileAndSource source in sources)
            {
                string newFileName = source.FilePath ?? fileNamePrefix + count++ + "." + fileExt;
                DocumentId documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                project = project.AddDocument(newFileName, SourceText.From(source.Source)).Project;
            }

            return project;
        }

        protected static Diagnostic[] GetSortedDiagnostics(DiagnosticAnalyzer analyzer, Document document, TextSpan?[] spans = null)
        {
            return GetSortedDiagnostics(analyzer, new[] { document }, spans);
        }

        protected static Diagnostic[] GetSortedDiagnostics(DiagnosticAnalyzer analyzer, Document[] documents, TextSpan?[] spans = null)
        {
            var projects = new HashSet<Project>();
            foreach (Document document in documents)
            {
                projects.Add(document.Project);
            }

            DiagnosticBag diagnostics = DiagnosticBag.GetInstance();
            foreach (Project project in projects)
            {
                Compilation compilation = project.GetCompilationAsync().Result;
                compilation = EnableAnalyzer(analyzer, compilation);

                ImmutableArray<Diagnostic> diags = compilation.GetAnalyzerDiagnostics(new[] { analyzer });
                if (spans == null)
                {
                    diagnostics.AddRange(diags);
                }
                else
                {
                    Debug.Assert(spans.Length == documents.Length);
                    foreach (Diagnostic diag in diags)
                    {
                        if (diag.Location == Location.None || diag.Location.IsInMetadata)
                        {
                            diagnostics.Add(diag);
                        }
                        else
                        {
                            for (int i = 0; i < documents.Length; i++)
                            {
                                Document document = documents[i];
                                SyntaxTree tree = document.GetSyntaxTreeAsync().Result;
                                if (tree == diag.Location.SourceTree)
                                {
                                    TextSpan? span = spans[i];
                                    if (span == null || span.Value.Contains(diag.Location.SourceSpan))
                                    {
                                        diagnostics.Add(diag);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Diagnostic[] results = GetSortedDiagnostics(diagnostics.AsEnumerable());
            diagnostics.Free();
            return results;
        }

        private static Compilation EnableAnalyzer(DiagnosticAnalyzer analyzer, Compilation compilation)
        {
            return compilation
                .WithOptions(
                    compilation
                        .Options
                        .WithSpecificDiagnosticOptions(
                            analyzer
                                .SupportedDiagnostics
                                .Select(x =>
                                    KeyValuePair.Create(x.Id, ReportDiagnostic.Default))
                                    .ToImmutableDictionaryOrEmpty()));
        }

        protected static void AnalyzeDocumentCore(DiagnosticAnalyzer analyzer, Document document, Action<Diagnostic> addDiagnostic, TextSpan? span = null, Action<Exception, DiagnosticAnalyzer, Diagnostic> onAnalyzerException = null, bool logAnalyzerExceptionAsDiagnostics = true)
        {
            SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
            Compilation compilation = semanticModel.Compilation;
            compilation = EnableAnalyzer(analyzer, compilation);

            ImmutableArray<Diagnostic> diagnostics = compilation.GetAnalyzerDiagnostics(new[] { analyzer }, onAnalyzerException: onAnalyzerException, logAnalyzerExceptionAsDiagnostics: logAnalyzerExceptionAsDiagnostics);
            foreach (Diagnostic diagnostic in diagnostics)
            {
                if (!span.HasValue ||
                    diagnostic.Location == Location.None ||
                    diagnostic.Location.IsInMetadata ||
                    (diagnostic.Location.SourceTree == semanticModel.SyntaxTree &&
                    span.Value.Contains(diagnostic.Location.SourceSpan)))
                {
                    addDiagnostic(diagnostic);
                }
            }
        }

        protected static Diagnostic[] GetSortedDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        public struct FileAndSource
        {
            public string FilePath { get; set; }
            public string Source { get; set; }
        }
    }
}
