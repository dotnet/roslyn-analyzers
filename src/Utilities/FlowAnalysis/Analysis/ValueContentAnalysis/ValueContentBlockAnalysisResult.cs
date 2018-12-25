// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="ValueContentAnalysis"/> on a basic block.
    /// It stores data values for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class ValueContentBlockAnalysisResult : AbstractBlockAnalysisResult<ValueContentAnalysisData>
    {
        public ValueContentBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<ValueContentAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData)
        {
            InputData = blockAnalysisData.Input?.CoreAnalysisData.Seal() ?? ImmutableDictionary<AnalysisEntity, ValueContentAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.CoreAnalysisData.Seal() ?? ImmutableDictionary<AnalysisEntity, ValueContentAbstractValue>.Empty;
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public IDictionary<AnalysisEntity, ValueContentAbstractValue> InputData { get; }
        public IDictionary<AnalysisEntity, ValueContentAbstractValue> OutputData { get; }
        public bool IsReachable { get; }
    }
}
