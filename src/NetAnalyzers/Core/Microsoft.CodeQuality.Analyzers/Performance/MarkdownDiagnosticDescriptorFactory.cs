// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    using System.Text;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a method for creating markdown formatted diagnostic descriptors.
    /// </summary>
    internal class MarkdownDiagnosticDescriptorFactory : IDiagnosticDescriptorFactory
    {
        /// <summary>
        /// Creates a diagnostic descriptor.
        /// </summary>
        /// <param name="summary">The summary description of the descriptor to be created.</param>
        /// <param name="severity">Severity of the diagnostic.</param>
        /// <returns>A diagnostic descriptor.</returns>
        public DiagnosticDescriptor CreateDiagnosticDescriptor(IDiagnosticDescriptorSummary summary, DiagnosticSeverity severity)
        {
            int initialSize = summary.DescriptorMessageFormat.Length + summary.ImprovementDescription.Length + "****\r\n".Length;

            StringBuilder sb = new StringBuilder(initialSize);
            sb.Append("**");
            sb.Append(summary.ImprovementDescription);
            sb.Append("**\r\n");
            sb.Append(summary.DescriptorMessageFormat);

            // The analyzer when used from the UI is very sensitive to the title and description parameters.
            // For some reason they both must be specified and either the same, or just summary.DescriptorTitle.
            //
            // TODO: Investigate: It makes no sense.
            //
            // Note: Duplicate descriptions will occur if you place the same type of diagnostic on the same node.
            //       Need to ensure this doesn't happen.
            return new DiagnosticDescriptor(
                               id: summary.DescriptorId,
                               title: summary.DescriptorTitle,
                               description: summary.DescriptorTitle,
                               messageFormat: sb.ToString(),
                               category: DescriptorConstants.DescriptorCategory,
                               defaultSeverity: severity,
                               isEnabledByDefault: true,
                               helpLinkUri: DescriptorConstants.DescriptorHelpUrl);
        }
    }
}
