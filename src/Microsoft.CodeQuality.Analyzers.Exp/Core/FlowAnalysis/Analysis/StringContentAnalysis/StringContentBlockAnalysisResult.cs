// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="StringContentAnalysis"/> on a basic block.
    /// It stores string content values for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
    /// </summary>
    internal class StringContentBlockAnalysisResult : AbstractBlockAnalysisResult<StringContentAnalysisData, StringContentAbstractValue>
    {
        public StringContentBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<StringContentAnalysisData> blockAnalysisData)
            : base(basicBlock)
        {
            InputData = blockAnalysisData.Input?.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, StringContentAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, StringContentAbstractValue>.Empty;
        }

        public ImmutableDictionary<AnalysisEntity, StringContentAbstractValue> InputData { get; }

        public ImmutableDictionary<AnalysisEntity, StringContentAbstractValue> OutputData { get; }
    }
}
