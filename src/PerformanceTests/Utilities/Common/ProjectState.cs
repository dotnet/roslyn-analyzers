// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PerformanceTests.Utilities
{

    public record ProjectState
    {
        public static ProjectState Create(string name, string language, string defaultPrefix, string defaultExtension)
        {
            return new ProjectState
            {
                Name = name,
                Language = language,
                DefaultPrefix = defaultPrefix,
                DefaultExtension = defaultExtension,
                Sources = new SourceFileList(defaultPrefix, defaultExtension),
            };
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; init; }
        public string Language { get; init; }
        public SourceFileList Sources { get; init; }
        private protected string DefaultPrefix { get; init; }
        private protected string DefaultExtension { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string AssemblyName => Name;
        public ReferenceAssemblies? ReferenceAssemblies { get; set; }
        public OutputKind? OutputKind { get; set; }
        public DocumentationMode? DocumentationMode { get; set; }
        public SourceFileCollection GeneratedSources { get; } = new SourceFileCollection();
        public SourceFileCollection AdditionalFiles { get; } = new SourceFileCollection();
        public SourceFileCollection AnalyzerConfigFiles { get; } = new SourceFileCollection();
        public List<Func<IEnumerable<(string filename, SourceText content)>>> AdditionalFilesFactories { get; } = new List<Func<IEnumerable<(string filename, SourceText content)>>>();
        public List<string> AdditionalProjectReferences { get; } = new List<string>();
    }
}
