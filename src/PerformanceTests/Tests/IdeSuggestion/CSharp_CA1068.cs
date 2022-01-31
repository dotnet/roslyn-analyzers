// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines;
using PerformanceTests.Utilities;
using PerfUtilities;

namespace CSharpPerformanceTests.IdeSuggestion
{
    public class CSharp_CA1068
    {
        [IterationSetup]
        public static void CreateEnvironmentCA1068()
        {
            var sources = new List<(string name, string content)>();
            for (var i = 0; i < Constants.Number_Of_Code_Files * 10; i++)
            {
                var name = "TypeName" + i;
                sources.Add((name, @$"
using System.Threading;
class {name}
{{
    void M(CancellationToken t, int i)
    {{
    }}

    void M1(int 1)
    {{
    }}

    void M2(int 1)
    {{
    }}

    void M3(int 1)
    {{
    }}

    void M4(int 1)
    {{
    }}

    void M5(int 1)
    {{
    }}

    void M6(int 1)
    {{
    }}

    void M7(int 1)
    {{
    }}

    void M8(int 1)
    {{
    }}

    void M9(int 1)
    {{
    }}

    void M10(int 1)
    {{
    }}
}}"));
            }

            var properties = new[]
            {
                ("dotnet_diagnostic.CA1068.severity", "warning"),
            };
            var (compilation, options) = CSharpCompilationHelper.CreateWithOptionsAsync(sources.ToArray(), properties).GetAwaiter().GetResult();
            BaselineCompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EmptyAnalyzer()), options);
            CompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new CancellationTokenParametersMustComeLastAnalyzer()), options);
        }

        private static CompilationWithAnalyzers BaselineCompilationWithAnalyzers;
        private static CompilationWithAnalyzers CompilationWithAnalyzers;

        [Benchmark]
        public async Task CA1068_DiagnosticsProduced()
        {
            var analysisResult = await CompilationWithAnalyzers.GetAnalysisResultAsync(CancellationToken.None);
            var diagnostics = analysisResult.GetAllDiagnostics(analysisResult.Analyzers.First());
            if (analysisResult.Analyzers.Length != 1)
            {
                throw new InvalidOperationException($"Expected a single analyzer but found '{analysisResult.Analyzers.Length}'");
            }

            if (analysisResult.CompilationDiagnostics.Count != 0)
            {
                throw new InvalidOperationException($"Expected no compilation diagnostics but found '{analysisResult.CompilationDiagnostics.Count}'");
            }

            if (diagnostics.Length != 1 * Constants.Number_Of_Code_Files * 10)
            {
                throw new InvalidOperationException($"Expected '{1 * Constants.Number_Of_Code_Files:N0}' analyzer diagnostics but found '{diagnostics.Length}'");
            }
        }

        [Benchmark(Baseline = true)]
        public async Task CA1068_Baseline()
        {
            var analysisResult = await BaselineCompilationWithAnalyzers.GetAnalysisResultAsync(CancellationToken.None);
            var diagnostics = analysisResult.GetAllDiagnostics(analysisResult.Analyzers.First());
            if (analysisResult.Analyzers.Length != 1)
            {
                throw new InvalidOperationException($"Expected a single analyzer but found '{analysisResult.Analyzers.Length}'");
            }

            if (analysisResult.CompilationDiagnostics.Count != 0)
            {
                throw new InvalidOperationException($"Expected no compilation diagnostics but found '{analysisResult.CompilationDiagnostics.Count}'");
            }

            if (diagnostics.Length != 0)
            {
                throw new InvalidOperationException($"Expected no analyzer diagnostics but found '{diagnostics.Length}'");
            }
        }
    }
}
