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
        private NullAnalysis(NullAnalysisDomain analysisDomain, NullDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> GetOrComputeResult(ControlFlowGraph cfg)
        {
            var analysisDomain = new NullAnalysisDomain(NullAbstractValueDomain.Default);
            var operationVisitor = new NullDataFlowOperationVisitor(NullAbstractValueDomain.Default);
            var nullAnalysis = new NullAnalysis(analysisDomain, operationVisitor);
            return nullAnalysis.GetOrComputeResultCore(cfg);
        }

        internal override NullBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<IDictionary<ISymbol, NullAbstractValue>> blockAnalysisData) => new NullBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
