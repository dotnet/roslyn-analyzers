// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Tasks;
using PerformanceTests.Utilities;
using PerfUtilities;

namespace CSharpPerformanceTests.Enabled
{
    public class CSharp_CA2247
    {
        [IterationSetup]
        public static void CreateEnvironmentCA2247()
        {
            var sources = new List<(string name, string content)>();
            for (var i = 0; i < Constants.Number_Of_Code_Files; i++)
            {
                var name = "TypeName" + i;
                sources.Add((name, @$"
using System.Threading.Tasks;

class {name}
{{
    void M()
    {{
        // Use TCS correctly without options
        new TaskCompletionSource<int>(null);
        new TaskCompletionSource<int>(""hello"");
        new TaskCompletionSource<int>(new object());
        new TaskCompletionSource<int>(42);

        // Uses TaskCreationOptions correctly
        var validEnum = TaskCreationOptions.RunContinuationsAsynchronously;
        new TaskCompletionSource<int>(validEnum);
        new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        new TaskCompletionSource<int>(this.MyProperty);
        new TaskCompletionSource<int>(new object(), validEnum);
        new TaskCompletionSource<int>(new object(), TaskCreationOptions.RunContinuationsAsynchronously);
        new TaskCompletionSource<int>(new object(), this.MyProperty);
        new TaskCompletionSource<int>(null, validEnum);
        new TaskCompletionSource<int>(null, TaskCreationOptions.RunContinuationsAsynchronously);
        new TaskCompletionSource<int>(null, this.MyProperty);

        // We only pay attention to things of type TaskContinuationOptions
        new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously.ToString());
        new TaskCompletionSource<int>(TaskContinuationOptions.RunContinuationsAsynchronously.ToString());
        new TaskCompletionSource<int>((int)TaskCreationOptions.RunContinuationsAsynchronously);
        new TaskCompletionSource<int>((int)TaskContinuationOptions.RunContinuationsAsynchronously);

        // Explicit choice to store into an object; ignored
        object validObject = TaskCreationOptions.RunContinuationsAsynchronously;
        new TaskCompletionSource<int>(validObject);
        object invalidObject = TaskContinuationOptions.RunContinuationsAsynchronously;
        new TaskCompletionSource<int>(invalidObject);

        new TaskCompletionSource<int>(TaskContinuationOptions.None);
        new TaskCompletionSource<int>(TaskContinuationOptions.RunContinuationsAsynchronously);
        var tcs = new TaskCompletionSource<int>(TaskContinuationOptions.AttachedToParent);
    }}
    TaskCreationOptions MyProperty {{ get; set; }}
}}
"));
            }

            var compilation = CSharpCompilationHelper.CreateAsync(sources.ToArray()).GetAwaiter().GetResult();
            BaselineCompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EmptyAnalyzer()));
            CompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DoNotCreateTaskCompletionSourceWithWrongArguments()));
        }

        private static CompilationWithAnalyzers BaselineCompilationWithAnalyzers;
        private static CompilationWithAnalyzers CompilationWithAnalyzers;

        [Benchmark]
        public async Task CA2247_DiagnosticsProduced()
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

            if (diagnostics.Length != 3 * Constants.Number_Of_Code_Files)
            {
                throw new InvalidOperationException($"Expected '{3 * Constants.Number_Of_Code_Files:N0}' analyzer diagnostics but found '{diagnostics.Length}'");
            }
        }

        [Benchmark(Baseline = true)]
        public async Task CA2247_Baseline()
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
