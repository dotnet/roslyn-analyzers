// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    /// <summary>
    /// Result from execution of <see cref="TaintedDataAnalysis"/> on a basic block.
    /// </summary>
    internal class TaintedDataBlockAnalysisResult : AbstractBlockAnalysisResult<TaintedDataAnalysisData>
    {
        public TaintedDataBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<TaintedDataAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData)
        {
            InputData = blockAnalysisData.Input?.CoreAnalysisData.Seal() ?? ImmutableDictionary<AnalysisEntity, TaintedDataAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.CoreAnalysisData.Seal() ?? ImmutableDictionary<AnalysisEntity, TaintedDataAbstractValue>.Empty;
            IsReachable = blockAnalysisData.Input?.IsReachableBlockData ?? true;
        }

        public IDictionary<AnalysisEntity, TaintedDataAbstractValue> InputData { get; }
        public IDictionary<AnalysisEntity, TaintedDataAbstractValue> OutputData { get; }
        public bool IsReachable { get; }
    }
}
