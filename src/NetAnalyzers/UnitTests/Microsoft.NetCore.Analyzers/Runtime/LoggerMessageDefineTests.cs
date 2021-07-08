// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information. 

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging.Analyzers;
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
            return GenerateTemplateUsages(template, arguments).Concat(GenerateDefineUsages(template));
        }

        public static IEnumerable<object[]> GenerateTemplateUsages(string template, string arguments)
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
                    yield return new[] { $"logger.{method}({format}{templateAndArguments});" };
                }
            }

            yield return new[] { $"logger.BeginScope({templateAndArguments});" };
        }

        private static IEnumerable<object[]> GenerateDefineUsages(string template)
        {
            // This is super rudimentary, but it works
            var braceCount = template.Count(c => c == '{');
            yield return new[] { $"LoggerMessage.{GenerateGenericInvocation(braceCount, "DefineScope")}({template});" };
            yield return new[] { $"LoggerMessage.{GenerateGenericInvocation(braceCount, "Define")}(LogLevel.Information, 42, {template});" };
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