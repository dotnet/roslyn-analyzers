// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents functionality needed to create a diagnostic descriptor.
    /// </summary>
    public interface IDiagnosticDescriptorFactory
    {
        /// <summary>
        /// Creates a diagnostic descriptor from the summary description.
        /// </summary>
        /// <param name="summary">The descriptor summary.</param>
        /// <param name="severity">The severity of the descriptor.</param>
        /// <returns>A diagnostic descriptor.</returns>
        DiagnosticDescriptor CreateDiagnosticDescriptor(IDiagnosticDescriptorSummary summary, DiagnosticSeverity severity);
    }
}
