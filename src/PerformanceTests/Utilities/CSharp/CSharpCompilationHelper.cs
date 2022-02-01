// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PerformanceTests.Utilities
{
    public static class CSharpCompilationHelper
    {
        public static async Task<Compilation?> CreateAsync((string, string)[] sourceFiles)
        {
            var (project, _) = await CreateProjectAsync(sourceFiles, null);
            return await project.GetCompilationAsync().ConfigureAwait(false);
        }

        public static async Task<(Compilation?, AnalyzerOptions)> CreateWithOptionsAsync((string, string)[] sourceFiles, (string, string)[] globalOptions)
        {
            var (project, options) = await CreateProjectAsync(sourceFiles, globalOptions);
            return (await project.GetCompilationAsync().ConfigureAwait(false), options);
        }

        private static Task<(Project, AnalyzerOptions)> CreateProjectAsync((string, string)[] sourceFiles, (string, string)[]? globalOptions = null)
            => CompilationHelper.CreateProjectAsync(
                sourceFiles,
                globalOptions,
                "TestProject",
                LanguageNames.CSharp,
                "/0/Test",
                "cs",
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                new CSharpParseOptions(LanguageVersion.Default));
    }
}
