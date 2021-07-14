// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.Extensions.Logging.Analyzers.LoggerMessageDefineAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.Extensions.Logging.Analyzer
{
    public class FormatStringAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA2250:""{0}""|}", "1", 1)]
        public async Task CA2250IsProducedForNumericFormatArgument(string format)
        {
            // Make sure CA1727 is enabled for this test so we can verify it does not trigger on numeric arguments.
            await TriggerCodeAsync(format);
        }

        [Theory]
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @"{|CA2251:$""{string.Empty}""|}", "")]
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @"{|CA2251:""string"" + 2|}", "")]
        public async Task CA2251IsProducedForDynamicFormatArgument(string format)
        {
            await TriggerCodeAsync(format);
        }

        [Theory]
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA2252:{|CA1727:""{string}""|}|}", "1, 2", 2)]
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA2252:{|CA1727:""{str"" + ""ing}""|}|}", "1, 2", 2)]
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @"{|CA2252:""{"" + nameof(ILogger) + ""}""|}", "")]
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @"{|CA2252:{|CA1727:""{"" + Const + ""}""|}|}", "")]
        public async Task CA2252IsProducedForFormatArgumentCountMismatch(string format)
        {
            await TriggerCodeAsync(format);
        }

        [Theory]
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA1727:""{camelCase}""|}", "1", 1)]
        public async Task CA1727IsProducedForCamelCasedFormatArgument(string format)
        {
            await TriggerCodeAsync(format);
        }

        [Theory]
        // Concat would be optimized by compiler
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @"nameof(ILogger) + "" string""", "")]
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @""" string"" + "" string""", "")]
        [MemberData(nameof(GenerateTemplateAndDefineUsages), @"$"" string"" + $"" string""", "")]
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA1727:""{st"" + ""ring}""|}", "1", 1)]

        // we are unable to parse expressions
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA1727:{|CA1727:""{string} {string}""|}|}", "new object[] { 1 }", 2)]

        // CA2250 is not enabled by default.
        [MemberData(nameof(GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope), @"{|CA1727:""{camelCase}""|}", "1", 1)]
        public async Task TemplateDiagnosticsAreNotProduced(string format)
        {
            await TriggerCodeAsync(format);
        }

        [Theory]
        [InlineData(@"LoggerMessage.Define(LogLevel.Information, 42, {|CA2252:""{One} {Two} {Three}""|});")]
        [InlineData(@"LoggerMessage.Define<int>(LogLevel.Information, 42, {|CA2252:""{One} {Two} {Three}""|});")]
        [InlineData(@"LoggerMessage.Define<int, int>(LogLevel.Information, 42, {|CA2252:""{One} {Two} {Three}""|});")]
        [InlineData(@"LoggerMessage.Define<int, int, int>(LogLevel.Information, 42, {|CA2252:""{One} {Two}""|});")]
        [InlineData(@"LoggerMessage.Define<int, int, int, int>(LogLevel.Information, 42, {|CA2252:""{One} {Two} {Three}""|});")]
        [InlineData(@"LoggerMessage.DefineScope<int>({|CA2252:""{One} {Two} {Three}""|});")]
        [InlineData(@"LoggerMessage.DefineScope<int, int>({|CA2252:""{One} {Two} {Three}""|});")]
        [InlineData(@"LoggerMessage.DefineScope<int, int, int>({|CA2252:""{One} {Two}""|});")]
        public async Task CA2252IsProducedForDefineMessageTypeParameterMismatch(string expression)
        {
            await TriggerCodeAsync(expression);
        }

        [Theory]
        [InlineData("LogTrace", @"""This is a test {Message}"", ""Foo""")]
        [InlineData("LogDebug", @"""This is a test {Message}"", ""Foo""")]
        [InlineData("LogInformation", @"""This is a test {Message}"", ""Foo""")]
        [InlineData("LogWarning", @"""This is a test {Message}"", ""Foo""")]
        [InlineData("LogError", @"""This is a test {Message}"", ""Foo""")]
        [InlineData("LogCritical", @"""This is a test {Message}"", ""Foo""")]
        [InlineData("BeginScope", @"""This is a test {Message}"", ""Foo""")]
        public async Task CA1839IsProducedForInvocationsOfAllLoggerExtensions(string method, string args)
        {
            string expression = @$"{{|CA1839:logger.{method}({args})|}};";
            await TriggerCodeAsync(expression);
        }

        public static IEnumerable<object[]> GenerateTemplateAndDefineUsages(string template, string arguments)
        {
            return GenerateTemplateUsages(template, arguments, skipDiagnosticForBeginScope: true).Concat(GenerateDefineUsages(template, numArgs: -1));
        }

        public static IEnumerable<object[]> GenerateTemplateAndDefineUsagesWithDiagnosticForBeginScope(string template, string arguments, int numArgs = -1)
        {
            return GenerateTemplateUsages(template, arguments, skipDiagnosticForBeginScope: false).Concat(GenerateDefineUsages(template, numArgs));
        }

        private static IEnumerable<object[]> GenerateDefineUsages(string template, int numArgs)
        {
            // This is super rudimentary, but it works
            int numberOfArguments = template.Count(c => c == '{');
            if (numArgs != -1)
            {
                numberOfArguments = numArgs;
            }
            yield return new[] { $"LoggerMessage.{GenerateGenericInvocation(numberOfArguments, "DefineScope")}({template});" };
            yield return new[] { $"LoggerMessage.{GenerateGenericInvocation(numberOfArguments, "Define")}(LogLevel.Information, 42, {template});" };
        }

        public static IEnumerable<object[]> GenerateTemplateUsages(string template, string arguments, bool skipDiagnosticForBeginScope)
        {
            var templateAndArguments = template;
            if (!string.IsNullOrEmpty(arguments))
            {
                templateAndArguments = $"{template}, {arguments}";
            }
            var methods = new[] { "LogTrace", "LogError", "LogWarning", "LogInformation", "LogDebug", "LogCritical" };
            var formats = new[]
            {
                "",
                "0, ",
                "1, new System.Exception(), ",
                "2, null, "
            };
            foreach (var method in methods)
            {
                foreach (var format in formats)
                {
                    yield return new[] { $"{{|CA1839:logger.{method}({format}{templateAndArguments})|}};" };
                }
            }

            if (skipDiagnosticForBeginScope)
            {
                yield return new[] { $"logger.BeginScope({templateAndArguments});" };
            }
            else
            {
                yield return new[] { $"{{|CA1839:logger.BeginScope({templateAndArguments})|}};" };
            }
        }

        private static string GenerateGenericInvocation(int i, string method)
        {
            if (i > 0)
            {
                var types = string.Join(", ", Enumerable.Range(0, i).Select(_ => "int"));
                method += $"<{types}>";
            }

            return method;
        }

        private async Task TriggerCodeAsync(string expression)
        {
            string code = @$"
using Microsoft.Extensions.Logging;
public class Program
{{
    public const string Const = ""const"";
    public static void Main()
    {{
        ILogger logger = null;
        {expression}
    }}
}}";
            await new VerifyCS.Test
            {
                LanguageVersion = CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                TestState =
                {
                    Sources = { code }
                },
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithMELogging,
            }.RunAsync();
        }
    }
}