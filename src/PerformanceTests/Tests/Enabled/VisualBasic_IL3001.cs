﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Publish;
using PerformanceTests.Utilities;
using PerfUtilities;

namespace VisualBasicPerformanceTests.Enabled
{
    public class VisualBasic_IL3001
    {
        [IterationSetup]
        public static void CreateEnvironmentIL3001()
        {
            var sources = new List<(string name, string content)>();
            for (var i = 0; i < Constants.Number_Of_Code_Files; i++)
            {
                var name = "TypeName" + i;
                sources.Add((name, $@"Imports System
Imports System.Reflection

Class {name}
    Sub M()
        Dim a = Assembly.LoadFrom(""/some/path/not/in/bundle"")
        Dim b = a.GetFile(""/some/file/path"")
        Dim c = a.GetFiles()
    End Sub
End Class"));
            }

            var (compilation, options) = VisualBasicCompilationHelper.CreateWithOptionsAsync(sources.ToArray(), new[] { ("build_property.PublishSingleFile", "true") }).GetAwaiter().GetResult();
            BaselineCompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EmptyAnalyzer()), options);
            CompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new AvoidAssemblyLocationInSingleFile()), options);
        }

        private static CompilationWithAnalyzers BaselineCompilationWithAnalyzers;
        private static CompilationWithAnalyzers CompilationWithAnalyzers;

        [Benchmark]
        public async Task IL3001_DiagnosticsProduced()
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

            if (diagnostics.Length != 2 * Constants.Number_Of_Code_Files)
            {
                throw new InvalidOperationException($"Expected '{2 * Constants.Number_Of_Code_Files:N0}' analyzer diagnostics but found '{diagnostics.Length}'");
            }
        }

        [Benchmark(Baseline = true)]
        public async Task IL3001_Baseline()
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
