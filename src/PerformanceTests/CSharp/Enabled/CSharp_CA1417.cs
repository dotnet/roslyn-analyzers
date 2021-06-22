// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.InteropServices;
using PerformanceTests.Utilities;
using PerfUtilities;

namespace CSharpPerformanceTests.Enabled
{
    public class CSharp_CA1417
    {
        [IterationSetup]
        public static void CreateEnvironmentCA1417()
        {
            var sources = new List<(string name, string content)>();
            for (var i = 0; i < Constants.Number_Of_Code_Files; i++)
            {
                var name = "TypeName" + i;
                sources.Add((name, @$"
using System;
using System.Runtime.InteropServices;

class {name}
{{
    [DllImport(""user32.dll"", CharSet=CharSet.Unicode)]
    private static extern void Method1([Out] string s); // Should not have [Out] string

    [DllImport(""user32.dll"", CharSet=CharSet.Unicode)]
    private static extern void Method2(string s1, [Out] string s2, [Out] string s3); // Should not have [Out] string
}}
"));
            }

            var compilation = CSharpCompilationHelper.CreateAsync(sources.ToArray()).GetAwaiter().GetResult();
            BaselineCompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EmptyAnalyzer()));
            CompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new DoNotUseOutAttributeStringPInvokeParametersAnalyzer()));
        }

        private static CompilationWithAnalyzers BaselineCompilationWithAnalyzers;
        private static CompilationWithAnalyzers CompilationWithAnalyzers;

        [Benchmark]
        public void CA1417_DiagnosticsProduced()
        {
            var analysisResult = CompilationWithAnalyzers.GetAnalysisResultAsync(default).GetAwaiter().GetResult();
            var diagnostics = analysisResult.GetAllDiagnostics(analysisResult.Analyzers.Single());
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
                throw new InvalidOperationException($"Expected '3,000' analyzer diagnostics but found '{diagnostics.Length}'");
            }
        }

        [Benchmark(Baseline = true)]
        public void CA1417_Baseline()
        {
            var analysisResult = BaselineCompilationWithAnalyzers.GetAnalysisResultAsync(default).GetAwaiter().GetResult();
            var diagnostics = analysisResult.GetAllDiagnostics(analysisResult.Analyzers.Single());
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
