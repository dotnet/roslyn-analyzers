// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using PointsToAnalysisData = IDictionary<AnalysisEntity, PointsToAbstractValue>;
    using PointsToAnalysisDomain = MapAbstractDomain<AnalysisEntity, PointsToAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track locations pointed to by <see cref="AnalysisEntity"/> and <see cref="IOperation"/> instances.
    /// </summary>
    internal partial class PointsToAnalysis : ForwardDataFlowAnalysis<PointsToAnalysisData, PointsToBlockAnalysisResult, PointsToAbstractValue>
    {
        private PointsToAnalysis(PointsToAnalysisDomain analysisDomain, PointsToDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            INamedTypeSymbol containingTypeSymbol,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null)
        {
            var analysisDomain = new PointsToAnalysisDomain(PointsToAbstractValueDomain.Default);
            var operationVisitor = new PointsToDataFlowOperationVisitor(PointsToAbstractValueDomain.Default, containingTypeSymbol, nullAnalysisResultOpt);
            var pointsToAnalysis = new PointsToAnalysis(analysisDomain, operationVisitor);
            return pointsToAnalysis.GetOrComputeResultCore(cfg);
        }

        internal override PointsToBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<PointsToAnalysisData> blockAnalysisData) => new PointsToBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
