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
        protected static Task AnalyzeCSAsync(string source, params DiagnosticResult[] expected) =>
            VerifyCSForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        protected static Task FixCSAsync(string originalSource, string fixedSource, params DiagnosticResult[] expected) =>
            VerifyCSForVersionAsync(originalSource, fixedSource, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        protected static Task AnalyzeCSUnsupportedAsync(string source, params DiagnosticResult[] expected) =>
            VerifyCSForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp20, expected);

        protected static Task AnalyzeVBAsync(string source, params DiagnosticResult[] expected) =>
            VerifyVBForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        protected static Task FixVBAsync(string originalSource, string fixedSource, params DiagnosticResult[] expected) =>
            VerifyVBForVersionAsync(originalSource, fixedSource, ReferenceAssemblies.NetCore.NetCoreApp50, expected);

        protected static Task AnalyzeVBUnsupportedAsync(string source, params DiagnosticResult[] expected) =>
            VerifyVBForVersionAsync(source, null, ReferenceAssemblies.NetCore.NetCoreApp20, expected);

        protected static DiagnosticResult GetCSResultForRule(int startLine, int startColumn, int endLine, int endColumn, DiagnosticDescriptor rule, string methodName, string methodPreferredName)
            => VerifyCS.Diagnostic(rule)
                .WithSpan(startLine, startColumn, endLine, endColumn)
                .WithArguments(methodName, methodPreferredName);

        protected static DiagnosticResult GetVBResultForRule(int startLine, int startColumn, int endLine, int endColumn, DiagnosticDescriptor rule, string methodName, string methodPreferredName)
            => VerifyVB.Diagnostic(rule)
                .WithSpan(startLine, startColumn, endLine, endColumn)
                .WithArguments(methodName, methodPreferredName);

        protected static string GetFormattedSourceCode(string source, string asyncMethodPrefix, string args, bool withConfigureAwait, string language)
        {
            string configureAwait = string.Empty;

            if (withConfigureAwait)
            {
                char booleanArgumentInitial = 'f';
                if (language == LanguageNames.VisualBasic)
                {
                    booleanArgumentInitial = 'F';
                }
                configureAwait = $".ConfigureAwait({booleanArgumentInitial}alse)";
            }

            asyncMethodPrefix = string.Format("s.{0}Async({1}){2}", asyncMethodPrefix, args, configureAwait);

            return string.Format(source, asyncMethodPrefix);
        }

        private static Task VerifyCSForVersionAsync(string originalSource, string fixedSource, ReferenceAssemblies version, params DiagnosticResult[] expected)
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

        private static Task VerifyVBForVersionAsync(string originalSource, string fixedSource, ReferenceAssemblies version, params DiagnosticResult[] expected)
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
    }
}