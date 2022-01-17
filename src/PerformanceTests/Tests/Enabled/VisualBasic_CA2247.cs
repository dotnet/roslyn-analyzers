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

namespace VisualBasicPerformanceTests.Enabled
{
    public class VisualBasic_CA2247
    {
        [IterationSetup]
        public static void CreateEnvironmentCA2247()
        {
            var sources = new List<(string name, string content)>();
            for (var i = 0; i < Constants.Number_Of_Code_Files; i++)
            {
                var name = "TypeName" + i;
                sources.Add((name, @$"
Imports System.Threading.Tasks

Class {name}
    Private Sub M()
        ' Use TCS correctly without options
        Dim a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(Nothing)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(""hello"")
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(New Object())
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(42)

        ' Uses TaskCreationOptions correctly
        Dim validEnum = System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(validEnum)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(MyProperty)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(New Object(), validEnum)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(New Object(), System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(New Object(), MyProperty)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(Nothing, validEnum)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(Nothing, System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously)
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(Nothing, MyProperty)

        ' We only pay attention to things of type TaskContinuationOptions
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously.ToString())
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(System.Threading.Tasks.TaskContinuationOptions.RunContinuationsAsynchronously.ToString())
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(CType(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously, Integer))
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(CType(System.Threading.Tasks.TaskContinuationOptions.RunContinuationsAsynchronously, Integer))

        ' Explicit choice to store into an object ignored
        Dim validObject As Object = System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(validObject)
        Dim invalidObject As Object = System.Threading.Tasks.TaskContinuationOptions.RunContinuationsAsynchronously
        a = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(invalidObject)


        Dim tcs = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(System.Threading.Tasks.TaskContinuationOptions.None)
        tcs = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(System.Threading.Tasks.TaskContinuationOptions.RunContinuationsAsynchronously)
        tcs = New System.Threading.Tasks.TaskCompletionSource(Of Integer)(System.Threading.Tasks.TaskContinuationOptions.AttachedToParent)
    End Sub

    Private MyProperty As System.Threading.Tasks.TaskCreationOptions
End Class
"));
            }

            var compilation = VisualBasicCompilationHelper.CreateAsync(sources.ToArray()).GetAwaiter().GetResult();
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
