// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    using NullAnalysisData = IDictionary<AnalysisEntity, NullAbstractValue>;
    using NullAnalysisDomain = MapAbstractDomain<AnalysisEntity, NullAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track null-ness of <see cref="AnalysisEntity"/>/<see cref="IOperation"/> instances.
    /// </summary>
    internal partial class NullAnalysis : ForwardDataFlowAnalysis<NullAnalysisData, NullBlockAnalysisResult, NullAbstractValue>
    {
        private NullAnalysis(NullAnalysisDomain analysisDomain, NullDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            INamedTypeSymbol containingTypeSymbol,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt = null)
        {
            var analysisDomain = new NullAnalysisDomain(NullAbstractValueDomain.Default);
            var operationVisitor = new NullDataFlowOperationVisitor(NullAbstractValueDomain.Default, containingTypeSymbol, pointsToAnalysisResultOpt);
            var nullAnalysis = new NullAnalysis(analysisDomain, operationVisitor);
            return nullAnalysis.GetOrComputeResultCore(cfg);
        }

        internal override NullBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<IDictionary<AnalysisEntity, NullAbstractValue>> blockAnalysisData) => new NullBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
