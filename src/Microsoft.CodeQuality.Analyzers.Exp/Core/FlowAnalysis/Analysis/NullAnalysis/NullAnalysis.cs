// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    using NullAnalysisData = IDictionary<ISymbol, NullAbstractValue>;
    using NullAnalysisDomain = MapAbstractDomain<ISymbol, NullAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track null-ness of symbols and operations.
    /// </summary>
    internal partial class NullAnalysis : ForwardDataFlowAnalysis<NullAnalysisData, NullBlockAnalysisResult, NullAbstractValue>
    {
        private NullAnalysis(NullAnalysisDomain analysisDomain, NullDataFlowOperationWalker dataflowOperationWalker)
            : base(analysisDomain, dataflowOperationWalker, nullAnalysisResultOpt: null)
        {
        }

        public static DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> GetOrComputeResult(ControlFlowGraph cfg)
        {
            var analysisDomain = new NullAnalysisDomain(NullAbstractValueDomain.Default);
            var dataflowOperationWalker = new NullDataFlowOperationWalker(NullAbstractValueDomain.Default);
            var nullAnalysis = new NullAnalysis(analysisDomain, dataflowOperationWalker);
            return nullAnalysis.GetOrComputeResultCore(cfg);
        }

        internal override NullBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<NullAnalysisData> blockAnalysisData) => new NullBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
