// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="ValueContentAnalysis"/> on a basic block.
    /// It stores data values for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class ValueContentBlockAnalysisResult : AbstractBlockAnalysisResult<AnalysisEntity, ValueContentAbstractValue>
    {
        public ValueContentBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<ValueContentAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input?.CoreAnalysisData, blockAnalysisData.Output?.CoreAnalysisData)
        {
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public bool IsReachable { get; }
    }
}
