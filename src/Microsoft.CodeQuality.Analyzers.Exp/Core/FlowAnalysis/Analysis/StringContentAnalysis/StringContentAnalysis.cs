// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<ISymbol, StringContentAbstractValue>;
    using StringContentAnalysisDomain = MapAbstractDomain<ISymbol, StringContentAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track string content of symbols and operations.
    /// </summary>
    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        private StringContentAnalysis(StringContentAnalysisDomain analysisDomain, StringContentDataFlowOperationWalker dataflowOperationWalker, DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null)
            : base(analysisDomain, dataflowOperationWalker, nullAnalysisResultOpt)
        {
        }

        public static DataFlowAnalysisResult<StringContentBlockAnalysisResult, StringContentAbstractValue> GetOrComputeResult(ControlFlowGraph cfg, DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null)
        {
            var analysisDomain = new StringContentAnalysisDomain(StringContentAbstractValueDomain.Default);
            var dataflowOperationWalker = new StringContentDataFlowOperationWalker(StringContentAbstractValueDomain.Default, nullAnalysisResultOpt);
            var nullAnalysis = new StringContentAnalysis(analysisDomain, dataflowOperationWalker, nullAnalysisResultOpt);
            return nullAnalysis.GetOrComputeResultCore(cfg);
        }

        internal override StringContentBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<StringContentAnalysisData> blockAnalysisData) => new StringContentBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
