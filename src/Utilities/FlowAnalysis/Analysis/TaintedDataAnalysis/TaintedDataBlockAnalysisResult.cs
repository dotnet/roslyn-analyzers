// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="TaintedDataAnalysis"/> on a basic block.
    /// </summary>
    internal class TaintedDataBlockAnalysisResult : AbstractBlockAnalysisResult<AnalysisEntity, TaintedDataAbstractValue>
    {
        public TaintedDataBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<TaintedDataAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input?.CoreAnalysisData, blockAnalysisData.Output?.CoreAnalysisData)
        {
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public bool IsReachable { get; }
    }
}
