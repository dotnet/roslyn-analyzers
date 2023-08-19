// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Composition;

namespace PerformanceTests.Utilities
{
    public class CompilationHelper
    {
        public static async Task<(Project, AnalyzerOptions)> CreateProjectAsync(
            (string, string)[] sourceFiles,
            (string, string)[]? globalOptions,
            string name,
            string language,
            string defaultPrefix,
            string defaultExtension,
            CompilationOptions compilationOptions,
            ParseOptions parseOptions)
        {
            var projectState = ProjectState.Create(name, language, defaultPrefix, defaultExtension);
            foreach (var (filename, content) in sourceFiles)
            {
                projectState.Sources.Add((defaultPrefix + filename + "." + defaultExtension, content));
            }

            var evaluatedProj = EvaluatedProjectState.Create(projectState, ReferenceAssemblies.Default);

            var project = await CreateProjectAsync(evaluatedProj, compilationOptions, parseOptions);

            if (globalOptions is not null)
            {
                var optionsProvider = new OptionsProvider(globalOptions);
                var options = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty, optionsProvider);

                return (project, options);

            }

            return (project, project.AnalyzerOptions);
        }

        private static async Task<Project> CreateProjectAsync(
            EvaluatedProjectState primaryProject,
            CompilationOptions compilationOptions,
            ParseOptions parseOptions)
        {
            var projectId = ProjectId.CreateNewId(debugName: primaryProject.Name);
            var solution = await CreateSolutionAsync(projectId, primaryProject, compilationOptions, parseOptions);

            foreach (var (newFileName, source) in primaryProject.Sources)
            {
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, source, filePath: newFileName);
            }

            foreach (var (newFileName, source) in primaryProject.AdditionalFiles)
            {
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddAdditionalDocument(documentId, newFileName, source, filePath: newFileName);
            }

            foreach (var (newFileName, source) in primaryProject.AnalyzerConfigFiles)
            {
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddAnalyzerConfigDocument(documentId, newFileName, source, filePath: newFileName);
            }

            return solution.GetProject(projectId)!;
        }

        private static async Task<Solution> CreateSolutionAsync(
            ProjectId projectId,
            EvaluatedProjectState projectState,
            CompilationOptions compilationOptions,
            ParseOptions parseOptions)
        {
            var referenceAssemblies = projectState.ReferenceAssemblies ?? ReferenceAssemblies.Default;

            compilationOptions = compilationOptions
                .WithOutputKind(projectState.OutputKind)
                .WithAssemblyIdentityComparer(referenceAssemblies.AssemblyIdentityComparer);

            parseOptions = parseOptions
                .WithDocumentationMode(projectState.DocumentationMode);

            var exportProviderFactory = new Lazy<IExportProviderFactory>(
                () =>
                {
                    var discovery = new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true);
                    var parts = Task.Run(() => discovery.CreatePartsAsync(MefHostServices.DefaultAssemblies)).GetAwaiter().GetResult();
                    var catalog = ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);

                    var configuration = CompositionConfiguration.Create(catalog);
                    var runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
                    return runtimeComposition.CreateExportProviderFactory();
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
            var exportProvider = exportProviderFactory.Value.CreateExportProvider();
            var host = MefHostServices.Create(exportProvider.AsCompositionContext());
            var workspace = new AdhocWorkspace(host);

            var solution = workspace
                .CurrentSolution
                .AddProject(projectId, projectState.Name, projectState.Name, projectState.Language)
                .WithProjectCompilationOptions(projectId, compilationOptions)
                .WithProjectParseOptions(projectId, parseOptions);

            var metadataReferences = await referenceAssemblies.ResolveAsync(projectState.Language);
            solution = solution.AddMetadataReferences(projectId, metadataReferences);

            return solution;
        }

        /// <summary>
        /// This class just passes argument through to the projects options provider and it used to provider custom global options
        /// </summary>
        private sealed class OptionsProvider : AnalyzerConfigOptionsProvider
        {
            public OptionsProvider((string, string)[] globalOptions)
            {
                GlobalOptions = new ConfigOptions(globalOptions);
            }

            public override AnalyzerConfigOptions GlobalOptions { get; }

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
                => GlobalOptions;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
                => GlobalOptions;
        }

        /// <summary>
        /// Allows adding additional global options
        /// </summary>
        private sealed class ConfigOptions : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _globalOptions;

            public ConfigOptions((string, string)[] globalOptions)
                => _globalOptions = globalOptions.ToDictionary(t => t.Item1, t => t.Item2);

            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
                => _globalOptions.TryGetValue(key, out value);
        }
    }
}
