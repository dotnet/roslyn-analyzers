// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Result from execution of <see cref="ExtraTypeCheckingAnalysis"/> on a basic block.
    /// It store dispose values for each <see cref="AbstractLocation"/> at the start and end of the basic block.
    /// </summary>
    internal class ExtraTypeCheckingBlockAnalysisResult : AbstractBlockAnalysisResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingBlockAnalysisResult"/> class.
        /// </summary>
        /// <param name="basicBlock">The basic block.</param>
        /// <param name="blockAnalysisData">The block analysis data.</param>
        public ExtraTypeCheckingBlockAnalysisResult(BasicBlock basicBlock, ExtraTypeCheckingAnalysisData blockAnalysisData)
            : base(basicBlock)
        {
            this.Data = blockAnalysisData?.Data.ToImmutableDictionary() ?? ImmutableDictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue>.Empty;
        }

        /// <summary>
        /// Gets the analysis data.
        /// </summary>
        public ImmutableDictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> Data { get; }
    }
}
