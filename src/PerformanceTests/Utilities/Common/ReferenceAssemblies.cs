// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace PerformanceTests.Utilities
{
    public sealed partial class ReferenceAssemblies
    {
        private readonly ImmutableArray<PortableExecutableReference> _assemblies;

        public ReferenceAssemblies(string targetFramework, ImmutableArray<PortableExecutableReference> assemblies)
        {
            _assemblies = assemblies;
            TargetFramework = targetFramework ?? throw new ArgumentNullException(nameof(targetFramework));
            AssemblyIdentityComparer = AssemblyIdentityComparer.Default;
            ReferenceAssemblyPath = null;
            Assemblies = ImmutableArray<string>.Empty;
            FacadeAssemblies = ImmutableArray<string>.Empty;
            LanguageSpecificAssemblies = ImmutableDictionary<string, ImmutableArray<string>>.Empty;
        }

        public static ReferenceAssemblies Default => Net.Net60;

        public string TargetFramework { get; }

        public AssemblyIdentityComparer AssemblyIdentityComparer { get; }

        public string? ReferenceAssemblyPath { get; }

        public ImmutableArray<string> Assemblies { get; }

        public ImmutableArray<string> FacadeAssemblies { get; }

        public ImmutableDictionary<string, ImmutableArray<string>> LanguageSpecificAssemblies { get; }

        public string? NuGetConfigFilePath { get; }

        public Task<ImmutableArray<MetadataReference>> ResolveAsync(string? language)
        {
            var references = ImmutableArray.CreateBuilder<MetadataReference>();
            if (language == LanguageNames.CSharp)
            {
                foreach (var assembly in _assemblies)
                {
                    if (assembly.FilePath != null && !assembly.FilePath.Contains("VisualBasic"))
                    {
                        references.Add(assembly);
                    }
                }

                return Task.FromResult(references.ToImmutable());
            }

            return Task.FromResult(_assemblies.CastArray<MetadataReference>());
        }

        public static class Net
        {
            private static readonly Lazy<ReferenceAssemblies> _lazyNet60 =
                new(() => new ReferenceAssemblies("net6.0", Reference.Assemblies.Net60.All.ToImmutableArray()));

            public static ReferenceAssemblies Net60 => _lazyNet60.Value;
        }
    }
}
