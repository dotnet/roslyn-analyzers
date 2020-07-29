// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Publish.AvoidAssemblyLocationInSingleFile,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using static Analyzer.Utilities.MSBuildPropertyOptionNames;
using System.Collections.Generic;

namespace Microsoft.NetCore.Analyzers.Publish.UnitTests
{
    public class AvoidAssemblyLocationInSingleFileTests
    {
        [Theory]
        [CombinatorialData]
        public Task GetExecutingAssemblyLocation(
            [CombinatorialValues(true, false, null)] bool? publish,
            [CombinatorialValues(true, false, null)] bool? includeContent)
        {
            const string source = @"
using System.Reflection;
class C
{
    public string M() => Assembly.GetExecutingAssembly().Location;
}";
            string analyzerConfig = "";
            if (publish is not null)
            {
                analyzerConfig += $"build_property.{PublishSingleFile} = {publish}" + Environment.NewLine;
            }
            if (includeContent is not null)
            {
                analyzerConfig += $"build_property.{IncludeAllContentForSelfExtract} = {includeContent}";
            }

            var test = new VerifyCS.Test
            {
                TestCode = source,
                AnalyzerConfigDocument = analyzerConfig
            };

            DiagnosticResult[] diagnostics;
            if (publish is true && includeContent is not true)
            {
                diagnostics = new[] {
// /0/Test0.cs(5,26): warning CA3000: Avoid `System.Reflection.Assembly.Location` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
VerifyCS.Diagnostic().WithSpan(5, 26, 5, 66).WithArguments("System.Reflection.Assembly.Location")
                };
            }
            else
            {
                diagnostics = Array.Empty<DiagnosticResult>();
            }

            test.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        [Fact]
        public Task AssemblyProperties()
        {
            var src = @"
using System.Reflection;
class C
{
    public void M()
    {
        var a = Assembly.GetExecutingAssembly();
        _ = a.Location;
        _ = a.CodeBase;
        _ = a.EscapedCodeBase;
    }
}";
            return VerifyDiagnosticsAsync(src,
                // /0/Test0.cs(8,13): warning CA3000: Avoid `System.Reflection.Assembly.Location` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(8, 13, 8, 23).WithArguments("System.Reflection.Assembly.Location"),
                // /0/Test0.cs(9,13): warning CA3000: Avoid `System.Reflection.Assembly.CodeBase` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(9, 13, 9, 23).WithArguments("System.Reflection.Assembly.CodeBase"),
                // /0/Test0.cs(10,13): warning CA3000: Avoid `System.Reflection.Assembly.EscapedCodeBase` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(10, 13, 10, 30).WithArguments("System.Reflection.Assembly.EscapedCodeBase")
            );
        }

        [Fact]
        public Task AssemblyMethods()
        {
            var src = @"
using System.Reflection;
class C
{
    public void M()
    {
        var a = Assembly.GetExecutingAssembly();
        _ = a.GetFile(""/some/file/path"");
        _ = a.GetFiles();
    }
}";
            return VerifyDiagnosticsAsync(src,
                // /0/Test0.cs(8,13): warning CA3000: Avoid `System.Reflection.Assembly.GetFile(string)` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(8, 13, 8, 41).WithArguments("System.Reflection.Assembly.GetFile(string)"),
                // /0/Test0.cs(9,13): warning CA3000: Avoid `System.Reflection.Assembly.GetFiles()` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(9, 13, 9, 25).WithArguments("System.Reflection.Assembly.GetFiles()")
                );
        }

        [Fact]
        public Task AssemblyNameAttributes()
        {
            var src = @"
using System.Reflection;
class C
{
    public void M()
    {
        var a = Assembly.GetExecutingAssembly().GetName();
        _ = a.CodeBase;
        _ = a.EscapedCodeBase;
    }
}";
            return VerifyDiagnosticsAsync(src,
                // /0/Test0.cs(8,13): warning CA3000: Avoid `System.Reflection.AssemblyName.CodeBase` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(8, 13, 8, 23).WithArguments("System.Reflection.AssemblyName.CodeBase"),
                // /0/Test0.cs(9,13): warning CA3000: Avoid `System.Reflection.AssemblyName.EscapedCodeBase` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(9, 13, 9, 30).WithArguments("System.Reflection.AssemblyName.EscapedCodeBase")
                );
        }

        [Fact]
        public Task FalsePositive()
        {
            // This is an OK use of Location and GetFile since these assemblies were loaded from
            // a file, but the analyzer is conservative
            var src = @"
using System.Reflection;
class C
{
    public void M()
    {
        var a = Assembly.LoadFrom(""/some/path/not/in/bundle"");
        _ = a.Location;
        _ = a.GetFiles();
    }
}";
            return VerifyDiagnosticsAsync(src,
                // /0/Test0.cs(8,13): warning CA3000: Avoid `System.Reflection.Assembly.Location` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(8, 13, 8, 23).WithArguments("System.Reflection.Assembly.Location"),
                // /0/Test0.cs(9,13): warning CA3000: Avoid `System.Reflection.Assembly.GetFiles()` when publishing as a single-file. Assemblies inside a single-file bundle do not have a file or file path. If the path to the app directory is needed, consider calling System.AppContext.BaseDirectory.
                VerifyCS.Diagnostic().WithSpan(9, 13, 9, 25).WithArguments("System.Reflection.Assembly.GetFiles()"));
        }

        private Task VerifyDiagnosticsAsync(string source, params DiagnosticResult[] expected)
        {
            const string singleFilePublishConfig = @"
build_property." + PublishSingleFile + " = true";

            var test = new VerifyCS.Test
            {
                TestCode = source,
                AnalyzerConfigDocument = singleFilePublishConfig
            };

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }
    }
}