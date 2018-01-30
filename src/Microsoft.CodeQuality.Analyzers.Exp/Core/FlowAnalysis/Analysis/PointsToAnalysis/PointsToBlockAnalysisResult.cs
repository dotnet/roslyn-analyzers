// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using PointsToAnalysisData = IDictionary<AnalysisEntity, PointsToAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="PointsToAnalysis"/> on a basic block.
    /// It store points to values for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class PointsToBlockAnalysisResult : AbstractBlockAnalysisResult<PointsToAnalysisData, PointsToAbstractValue>
    {
        public PointsToBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<PointsToAnalysisData> blockAnalysisData)
            : base (basicBlock)
        {
            InputData = blockAnalysisData.Input?.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, PointsToAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, PointsToAbstractValue>.Empty;
        }

        public ImmutableDictionary<AnalysisEntity, PointsToAbstractValue> InputData { get; }
        public ImmutableDictionary<AnalysisEntity, PointsToAbstractValue> OutputData { get; }
    }
}
