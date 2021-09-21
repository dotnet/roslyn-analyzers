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
using Microsoft.NetCore.Analyzers.InteropServices;
using PerformanceTests.Utilities;
using PerfUtilities;

namespace CSharpPerformanceTests.Enabled
{
    public class CSharp_CA1416
    {
        [IterationSetup]
        public static void CreateEnvironmentCA1416()
        {
            var sources = new List<(string name, string content)>();
            for (var i = 0; i < Constants.Number_Of_Code_Files; i++)
            {
                var name = "TypeName" + i;
                sources.Add((name, @$"
using System;
using PlatformCompatDemo.SupportedUnupported;

class {name}
{{
    private B field = new B();
    public void M1()
    {{
        field.M2();
    }}
}}
"));
                name += "Unsupported";
                sources.Add((name + "Unsupport", @$"
using System;
using PlatformCompatDemo.SupportedUnupported;

class {name}
{{
    private B field = new B();
    public void M1()
    {{
        field.M3();
    }}
}}
"));

                name += "Flow";
                sources.Add((name, @$"
using System;
using PlatformCompatDemo.SupportedUnupported;

class {name}
{{
    private B field = new B();
    public void M1()
    {{
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 2))
        {{
            field.M2();
        }}
        else
        {{
            field.M2();
        }}
    }}
}}
"));
            }

            var targetTypesForTest = @"
using System.Runtime.Versioning;
namespace PlatformCompatDemo.SupportedUnupported
{
    public class B
    {
        [SupportedOSPlatform(""Windows10.1.1.1"")]
        public void M2() { }
        [UnsupportedOSPlatform(""macOS11.0.1"")]
        public void M3() {}
    }
}";
            sources.Add((nameof(targetTypesForTest), targetTypesForTest));
            var properties = new[]
            {
                ("build_property.TargetFramework", "net6"),
                ("build_property._SupportedPlatformList", "Linux,macOS"),
            };
            var (compilation, options) = CSharpCompilationHelper.CreateWithOptionsAsync(sources.ToArray(), properties).GetAwaiter().GetResult();
            BaselineCompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EmptyAnalyzer()), options);
            CompilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new PlatformCompatibilityAnalyzer()), options);
        }

        private static CompilationWithAnalyzers BaselineCompilationWithAnalyzers;
        private static CompilationWithAnalyzers CompilationWithAnalyzers;

        [Benchmark]
        public async Task CA1416_DiagnosticsProduced()
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
                throw new InvalidOperationException($"Expected '{1 * Constants.Number_Of_Code_Files:N0}' analyzer diagnostics but found '{diagnostics.Length}'");
            }
        }

        [Benchmark(Baseline = true)]
        public async Task CA1416_Baseline()
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
