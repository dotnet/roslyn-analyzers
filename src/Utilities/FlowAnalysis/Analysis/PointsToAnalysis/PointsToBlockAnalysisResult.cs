// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="PointsToAnalysis"/> on a basic block.
    /// It stores the PointsTo value for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class PointsToBlockAnalysisResult : AbstractBlockAnalysisResult<AnalysisEntity, PointsToAbstractValue>
    {
        public PointsToBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<PointsToAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input?.CoreAnalysisData, blockAnalysisData.Output?.CoreAnalysisData)
        {
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public bool IsReachable { get; }
    }
}
