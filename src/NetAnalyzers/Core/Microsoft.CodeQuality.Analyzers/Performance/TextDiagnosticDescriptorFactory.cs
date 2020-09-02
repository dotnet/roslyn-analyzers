// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a method for creating plain text diagnostic descriptors.
    /// </summary>
    public class TextDiagnosticDescriptorFactory : IDiagnosticDescriptorFactory
    {
        /// <summary>
        /// Creates a diagnostic descriptor from the summary.
        /// </summary>
        /// <param name="summary">The flagged member rule that needs the diagnostic.</param>
        /// <param name="severity">Severity of the diagnostic.</param>
        /// <returns>A diagnostic descriptor.</returns>
        public DiagnosticDescriptor CreateDiagnosticDescriptor(IDiagnosticDescriptorSummary summary, DiagnosticSeverity severity)
        {
            // The analyzer when used from the UI is very sensitive to the title and description parameters.
            // For some reason they both must be specified and either the same, or just summary.DescriptorTitle.
            // TODO: Investigate: It makes no sense.
            return new DiagnosticDescriptor(
                               id: summary.DescriptorId,
                               title: summary.DescriptorTitle,
                               description: summary.DescriptorTitle,
                               messageFormat: summary.DescriptorMessageFormat,
                               category: DescriptorConstants.DescriptorCategory,
                               defaultSeverity: severity,
                               isEnabledByDefault: true,
                               helpLinkUri: DescriptorConstants.DescriptorHelpUrl);
        }
    }
}
