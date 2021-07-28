// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;

namespace PerformanceTests.Utilities
{
    public class VisualBasicCompilationHelper
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
                LanguageNames.VisualBasic,
                "/0/Test",
                "vb",
                new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                new VisualBasicParseOptions(LanguageVersion.Default));
    }
}
