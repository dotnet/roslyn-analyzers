// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis
{
    using CopyAnalysisData = IDictionary<AnalysisEntity, CopyAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="CopyAnalysis"/> on a basic block.
    /// It store copy values for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class CopyBlockAnalysisResult : AbstractBlockAnalysisResult<CopyAnalysisData, CopyAbstractValue>
    {
        public CopyBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<CopyAnalysisData> blockAnalysisData)
            : base (basicBlock)
        {
            InputData = blockAnalysisData.Input?.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, CopyAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, CopyAbstractValue>.Empty;
        }

        public ImmutableDictionary<AnalysisEntity, CopyAbstractValue> InputData { get; }
        public ImmutableDictionary<AnalysisEntity, CopyAbstractValue> OutputData { get; }
    }
}
