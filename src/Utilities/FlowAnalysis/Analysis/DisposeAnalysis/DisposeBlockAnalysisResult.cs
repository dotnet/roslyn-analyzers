// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis
{
    using DisposeAnalysisData = DictionaryAnalysisData<AbstractLocation, DisposeAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="DisposeAnalysis"/> on a basic block.
    /// It store dispose values for each <see cref="AbstractLocation"/> at the start and end of the basic block.
    /// </summary>
    internal class DisposeBlockAnalysisResult : AbstractBlockAnalysisResult<DisposeAnalysisData>
    {
        public DisposeBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<DisposeAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData)
        {
            InputData = blockAnalysisData.Input?.Seal() ?? ImmutableDictionary<AbstractLocation, DisposeAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.Seal() ?? ImmutableDictionary<AbstractLocation, DisposeAbstractValue>.Empty;
        }

        public IDictionary<AbstractLocation, DisposeAbstractValue> InputData { get; }
        public IDictionary<AbstractLocation, DisposeAbstractValue> OutputData { get; }
    }
}
