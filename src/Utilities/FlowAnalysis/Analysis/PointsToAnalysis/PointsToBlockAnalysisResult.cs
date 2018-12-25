// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="PointsToAnalysis"/> on a basic block.
    /// It stores the PointsTo value for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class PointsToBlockAnalysisResult : AbstractBlockAnalysisResult<PointsToAnalysisData>
    {
        public PointsToBlockAnalysisResult(
            BasicBlock basicBlock,
            DataFlowAnalysisInfo<PointsToAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData)
        {
            InputData = blockAnalysisData.Input?.CoreAnalysisData.Seal() ?? ImmutableDictionary<AnalysisEntity, PointsToAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.CoreAnalysisData.Seal() ?? ImmutableDictionary<AnalysisEntity, PointsToAbstractValue>.Empty;
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public IDictionary<AnalysisEntity, PointsToAbstractValue> InputData { get; }
        public IDictionary<AnalysisEntity, PointsToAbstractValue> OutputData { get; }
        public bool IsReachable { get; }
    }
}
