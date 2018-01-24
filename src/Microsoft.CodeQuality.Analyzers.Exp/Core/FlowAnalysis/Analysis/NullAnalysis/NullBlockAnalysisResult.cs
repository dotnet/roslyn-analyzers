// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    using NullAnalysisData = IDictionary<ISymbol, NullAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="NullAnalysis"/> on a basic block.
    /// It store null values for symbols at the start and end of the basic block.
    /// </summary>
    internal class NullBlockAnalysisResult : AbstractBlockAnalysisResult<NullAnalysisData, NullAbstractValue>
    {
        public NullBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<NullAnalysisData> blockAnalysisData)
            : base (basicBlock)
        {
            InputData = blockAnalysisData.Input?.ToImmutableDictionary() ?? ImmutableDictionary<ISymbol, NullAbstractValue>.Empty;
            OutputData = blockAnalysisData.Output?.ToImmutableDictionary() ?? ImmutableDictionary<ISymbol, NullAbstractValue>.Empty;
        }

        public ImmutableDictionary<ISymbol, NullAbstractValue> InputData { get; }
        public ImmutableDictionary<ISymbol, NullAbstractValue> OutputData { get; }
    }
}
