// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferStreamAsyncMemoryOverloads,
    Microsoft.NetCore.Analyzers.Runtime.PreferStreamAsyncMemoryOverloadsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Runtime.PreferStreamAsyncMemoryOverloads,
    Microsoft.NetCore.Analyzers.Runtime.PreferStreamAsyncMemoryOverloadsFixer>;

#pragma warning disable CA1305 // Specify IFormatProvider in string.Format

namespace Microsoft.NetCore.Analyzers.Runtime.UnitTests
{
    public class PreferStreamAsyncMemoryOverloadsTestBase
    {
        // Verifies that the analyzer generates the specified C# diagnostic results, if any.
        protected static Task CSharpVerifyAnalyzerAsync(string source, params DiagnosticResult[] expected) =>
            CSharpVerifyForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        // Verifies that the analyzer generates the specified VB diagnostic results, if any.
        protected static Task VisualBasicVerifyAnalyzerAsync(string source, params DiagnosticResult[] expected) =>
            VisualBasicVerifyForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        // Verifies that the analyzer generates the specified C# diagnostic results, if any, in an unsupported .NET version.
        protected static Task CSharpVerifyAnalyzerForUnsupportedVersionAsync(string source, params DiagnosticResult[] expected) =>
            CSharpVerifyForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp20, expected);

        // Verifies that the analyzer generates the specified VB diagnostic results, if any, in an unsupported .NET version.
        protected static Task VisualBasicVerifyAnalyzerForUnsupportedVersionAsync(string source, params DiagnosticResult[] expected) =>
            VisualBasicVerifyForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp20, expected);

        // Verifies that the fixer generates the fixes for the specified C# diagnostic results, if any.
        protected static Task CSharpVerifyCodeFixAsync(string originalSource, string fixedSource, params DiagnosticResult[] expected) =>
            CSharpVerifyForVersionAsync(originalSource, fixedSource, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        // Verifies that the fixer generates the fixes for the specified VB diagnostic results, if any.
        protected static Task VisualBasicVerifyCodeFixAsync(string originalSource, string fixedSource, params DiagnosticResult[] expected) =>
            VisualBasicVerifyForVersionAsync(originalSource, fixedSource, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        // Verifies that the analyzer generates the specified C# diagnostic results, if any, for the specified originalSource.
        // If fixedSource is provided, also verifies that the fixer generates the fixes for the verified diagnostic results, if any.
        private static Task CSharpVerifyForVersionAsync(string originalSource, string fixedSource, ReferenceAssemblies version, params DiagnosticResult[] expected)
        {
            var test = new VerifyCS.Test
            {
                TestCode = originalSource,
                ReferenceAssemblies = version,
            };

            if (!string.IsNullOrEmpty(fixedSource))
            {
                test.FixedCode = fixedSource;
            }

            test.ExpectedDiagnostics.AddRange(expected);

            return test.RunAsync();
        }

        // Verifies that the analyzer generates the specified VB diagnostic results, if any, for the specified originalSource.
        // If fixedSource is provided, also verifies that the fixer generates the fixes for the verified diagnostic results, if any.
        private static Task VisualBasicVerifyForVersionAsync(string originalSource, string fixedSource, ReferenceAssemblies version, params DiagnosticResult[] expected)
        {
            var test = new VerifyVB.Test
            {
                TestCode = originalSource,
                ReferenceAssemblies = version,
            };

            if (!string.IsNullOrEmpty(fixedSource))
            {
                test.FixedCode = fixedSource;
            }

            test.ExpectedDiagnostics.AddRange(expected);

            return test.RunAsync();
        }

        // Embeds the specified invocation strings into the provided source code and returns it.
        protected static string GetSourceCodeForInvocation(string source, string asyncMethodPrefix, string args, bool withConfigureAwait, string language)
        {
            string configureAwait = string.Empty;

            if (withConfigureAwait)
            {
                string booleanArg = (language == LanguageNames.VisualBasic) ? "False" : "false";
                configureAwait = $".ConfigureAwait({booleanArg})";
            }

            asyncMethodPrefix = string.Format("s.{0}Async({1}){2}", asyncMethodPrefix, args, configureAwait);

            return string.Format(source, asyncMethodPrefix);
        }

        // Retrieves the C# diagnostic for the specified rule, lines, columns, method and preferred method.
        protected static DiagnosticResult GetCSResultForRule(int startLine, int startColumn, int endLine, int endColumn, DiagnosticDescriptor rule, string methodName, string methodPreferredName)
            => VerifyCS.Diagnostic(rule)
                .WithSpan(startLine, startColumn, endLine, endColumn)
                .WithArguments(methodName, methodPreferredName);

        // Retrieves the VB diagnostic for the specified rule, lines, columns, method and preferred method.
        protected static DiagnosticResult GetVBResultForRule(int startLine, int startColumn, int endLine, int endColumn, DiagnosticDescriptor rule, string methodName, string methodPreferredName)
            => VerifyVB.Diagnostic(rule)
                .WithSpan(startLine, startColumn, endLine, endColumn)
                .WithArguments(methodName, methodPreferredName);

    }
}