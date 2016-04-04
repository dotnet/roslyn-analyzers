// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace a2md
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Loader loader = new Loader();
            DescriptorEqualityComparer comparer = new DescriptorEqualityComparer();

            string outputMarkdown = args
                .Select(arg => new AnalyzerFileReference(arg, loader))
                .SelectMany(analyzerReference => analyzerReference.GetAnalyzersForAllLanguages())
                .Where(HasImplementation)
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Distinct(comparer)
                .OrderBy(descriptor => descriptor.Id)
                .Select(GenerateDescriptorText)
                .Join(Environment.NewLine + Environment.NewLine);

            Console.Write(outputMarkdown);
        }

        /// <summary>
        /// Check the method body of the Initialize method of an analyzer and if that's empty,
        /// then the analyzer hasn't been implemented yet.
        /// </summary>
        private static bool HasImplementation(DiagnosticAnalyzer analyzer)
        {
            System.Reflection.MethodInfo method = analyzer.GetType().GetMethod("Initialize");
            if (method != null)
            {
                System.Reflection.MethodBody body = method.GetMethodBody();
                int? ilInstructionCount = body?.GetILAsByteArray()?.Count();
                // An empty method has two IL instructions - nop and ret.
                return ilInstructionCount != 2;
            }

            return true;
        }

        private static string GenerateDescriptorText(DiagnosticDescriptor descriptor)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"### {descriptor.Id}: {descriptor.Title} ###");

            if (!string.IsNullOrWhiteSpace(descriptor.Description.ToString()))
            {
                builder
                    .AppendLine()
                    .AppendLine()
                    .Append(descriptor.Description.ToString());
            }

            builder
                .AppendLine()
                .AppendLine()
                .AppendLine($"Category: {descriptor.Category}")
                .AppendLine()
                .Append($"Severity: {descriptor.DefaultSeverity}");

            if (!string.IsNullOrWhiteSpace(descriptor.HelpLinkUri))
            {
                builder
                    .AppendLine()
                    .AppendLine()
                    .Append($"Help: [{descriptor.HelpLinkUri}]({descriptor.HelpLinkUri})");
            }

            return builder.ToString();
        }
    }
}
