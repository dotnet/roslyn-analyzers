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

            var outputMarkdown = args
                .Select(arg => new AnalyzerFileReference(arg, loader))
                .SelectMany(analyzerReference => analyzerReference.GetAnalyzersForAllLanguages())
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Distinct(comparer)
                .OrderBy(descriptor => descriptor.Id)
                .Select(GenerateDescriptorText)
                .Join(Environment.NewLine + Environment.NewLine);

            Console.Write(outputMarkdown);
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
