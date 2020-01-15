// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MetaCompilation.Analyzers.UnitTests
{
    /// <summary>
    /// Class for turning strings into documents and getting the diagnostics on them
    /// All methods are static
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        private static readonly MetadataReference s_corlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference s_systemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference s_csharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference s_codeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        private static readonly MetadataReference s_systemCollectionsImmutableReference = MetadataReference.CreateFromFile(typeof(ImmutableArray).Assembly.Location);
        private static readonly MetadataReference s_runtimeReference = MetadataReference.CreateFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll"));

        internal const string DefaultFilePathPrefix = "Test";
        internal const string CSharpDefaultFileExt = "cs";
        internal const string VisualBasicDefaultExt = "vb";
        internal static readonly string CSharpDefaultFilePath = DefaultFilePathPrefix + 0 + "." + CSharpDefaultFileExt;
        internal const string TestProjectName = "TestProject";
        internal static readonly string VisualBasicDefaultFilePath = DefaultFilePathPrefix + 0 + "." + VisualBasicDefaultExt;

        #region Set up compilation and documents
        /// <summary>
        /// Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Document created from the source string</returns>
        protected static Document CreateDocument(string source, string language = LanguageNames.CSharp)
        {
            return CreateProject(new[] { source }, language).Documents.First();
        }

        /// <summary>
        /// Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Project created out of the Documents created from the source strings</returns>
        private static Project CreateProject(string[] sources, string language = LanguageNames.CSharp)
        {
            string fileNamePrefix = DefaultFilePathPrefix;
            string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

            ProjectId projectId = ProjectId.CreateNewId(debugName: TestProjectName);

#pragma warning disable CA2000 // Dispose objects before losing scope - Current solution/project takes the dispose ownership of the created AdhocWorkspace
            Solution solution = new AdhocWorkspace()
#pragma warning restore CA2000 // Dispose objects before losing scope
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, language)
                .AddMetadataReference(projectId, s_corlibReference)
                .AddMetadataReference(projectId, s_systemCoreReference)
                .AddMetadataReference(projectId, s_csharpSymbolsReference)
                .AddMetadataReference(projectId, s_codeAnalysisReference)
                .AddMetadataReference(projectId, s_systemCollectionsImmutableReference)
                .AddMetadataReference(projectId, s_runtimeReference);

            int count = 0;
            foreach (string source in sources)
            {
                string newFileName = fileNamePrefix + count + "." + fileExt;
                DocumentId documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }
            return solution.GetProject(projectId);
        }
        #endregion
    }
}

