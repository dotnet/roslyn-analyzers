// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeQuality.Analyzers.Performance
{
    using System;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Coding pattern description.
    /// </summary>
    internal class CodingPattern : IDiagnosticDescriptorSummary
    {
        /// <summary>
        /// Cpu reduction improvement.
        /// </summary>
        internal const string CpuImprovement = "CPU reduction";

        /// <summary>
        /// Memory reduction improvement.
        /// </summary>
        internal const string MemoryImprovement = "Memory reduction";

        /// <summary>
        /// Cpu and Memory reduction.
        /// </summary>
        internal const string CpuAndMemoryImprovement = "CPU and Memory reduction";

        /// <summary>
        /// Lock contention issue.
        /// </summary>
        internal const string LockContentionImprovement = "Lock contention";

        /// <summary>
        /// Reduction in allocations improves memory and GC processing.
        /// </summary>
        internal const string GcCpuAndMemoryImprovement = "Memory and GC CPU reduction";

        /// <summary>
        /// Fix that should be made for services running in production data center.
        /// </summary>
        internal const string ProductionReadyImprovement = "Production ready";

        /// <summary>
        /// Memory reduction (large object heap).
        /// </summary>
        internal const string MemoryAndLargeObjectHeapImprovement = "Memory reduction (Large object heap)";

        /// <summary>
        /// CPU usage or scheduling optimization.
        /// </summary>
        internal const string CpuOptimizationImprovement = "CPU optimization";

        /// <summary>
        /// CPU reduction due to (spin) lock contention issues.
        /// </summary>
        internal const string CpuAndLockContentionImprovement = "CPU reduction and Lock contention";

        /// <summary>
        /// Initializes a new instance of the <see cref="CodingPattern" /> class.
        /// </summary>
        /// <param name="descriptorId">the id.</param>
        /// <param name="descriptorTitle">the title.</param>
        /// <param name="descriptorMessageFormat">the message.</param>
        /// <param name="improvementDescription">Description of what the fix will improve.</param>
        public CodingPattern(
            string descriptorId,
            string descriptorTitle,
            string descriptorMessageFormat,
            string improvementDescription)
        {
            this.DescriptorId = descriptorId ?? throw new ArgumentNullException(nameof(descriptorId));
            this.DescriptorTitle = descriptorTitle ?? throw new ArgumentNullException(nameof(descriptorTitle));
            this.DescriptorMessageFormat = descriptorMessageFormat ?? throw new ArgumentNullException(nameof(descriptorMessageFormat));
            this.ImprovementDescription = improvementDescription ?? throw new ArgumentNullException(nameof(improvementDescription));
        }

        /// <summary>
        /// Gets the descriptor identifier.
        /// </summary>
        public string DescriptorId { get; }

        /// <summary>
        /// Gets the descriptor title.
        /// </summary>
        public string DescriptorTitle { get; }

        /// <summary>
        /// Gets the descriptor message format.
        /// </summary>
        public string DescriptorMessageFormat { get; }

        /// <summary>
        /// Gets the type of improvement a fix would have on the affected code.
        /// </summary>
        /// <remarks>
        /// This text is used in the markdown version of the comment description.
        /// </remarks>
        public string ImprovementDescription { get; }

        /// <summary>
        /// Gets Fix provider Type if any.
        /// </summary>
        public Type? FixProviderType { get; }

        /// <summary>
        /// Gets a value indicating whether HasFixProvider.
        /// </summary>
        public bool HasFixProvider
        {
            get { return this.FixProviderType != null; }
        }

        /// <summary>
        /// Gets the syntax node types this applies to.
        /// </summary>
        public SyntaxKind[]? SyntaxKinds { get; }
    }
}
