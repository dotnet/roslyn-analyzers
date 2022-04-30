// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PerformanceTests.Utilities
{
    public record EvaluatedProjectState
    {
        public static EvaluatedProjectState Create(ProjectState state, ReferenceAssemblies defaultReferenceAssemblies)
            => new()
            {
                Name = state.Name,
                AssemblyName = state.AssemblyName,
                Language = state.Language,
                ReferenceAssemblies = state.ReferenceAssemblies ?? defaultReferenceAssemblies,
                OutputKind = state.OutputKind ?? OutputKind.DynamicallyLinkedLibrary,
                DocumentationMode = state.DocumentationMode ?? DocumentationMode.Diagnose,
                Sources = state.Sources.ToImmutableArray(),
                GeneratedSources = state.GeneratedSources.ToImmutableArray(),
                AdditionalFiles = state.AdditionalFiles.ToImmutableArray(),
                AnalyzerConfigFiles = state.AnalyzerConfigFiles.ToImmutableArray(),
                AdditionalProjectReferences = state.AdditionalProjectReferences.ToImmutableArray(),
                AdditionalDiagnostics = ImmutableArray<Diagnostic>.Empty
            };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; init; }
        public string AssemblyName { get; init; }
        public string Language { get; init; }
        public ReferenceAssemblies ReferenceAssemblies { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public OutputKind OutputKind { get; init; }
        public DocumentationMode DocumentationMode { get; init; }
        public ImmutableArray<(string filename, SourceText content)> Sources { get; init; }
        public ImmutableArray<(string filename, SourceText content)> GeneratedSources { get; init; }
        public ImmutableArray<(string filename, SourceText content)> AdditionalFiles { get; init; }
        public ImmutableArray<(string filename, SourceText content)> AnalyzerConfigFiles { get; init; }
        public ImmutableArray<string> AdditionalProjectReferences { get; init; }
        public ImmutableArray<Diagnostic> AdditionalDiagnostics { get; init; }
    }
}
