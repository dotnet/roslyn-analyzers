// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="CopyAnalysis"/> on a basic block.
    /// It store copy values for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal sealed class CopyBlockAnalysisResult : AbstractBlockAnalysisResult<AnalysisEntity, CopyAbstractValue>
    {
        public CopyBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<CopyAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input?.CoreAnalysisData, blockAnalysisData.Output?.CoreAnalysisData)
        {
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public bool IsReachable { get; }
    }
}
