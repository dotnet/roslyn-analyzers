// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    /// <summary>
    /// Represents the basic information needed to create a diagnostic descriptor.
    /// </summary>
    public interface IDiagnosticDescriptorSummary
    {
        /// <summary>
        /// Gets the descriptor identifier.
        /// </summary>
        string DescriptorId { get; }

        /// <summary>
        /// Gets the descriptor title.
        /// </summary>
        string DescriptorTitle { get; }

        /// <summary>
        /// Gets the descriptor message format.
        /// </summary>
        string DescriptorMessageFormat { get; }

        /// <summary>
        /// Gets the type of improvement a fix would have on the affected code.
        /// </summary>
        string ImprovementDescription { get; }
    }
}
