// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using PointsToAnalysisData = IDictionary<AnalysisEntity, PointsToAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track locations pointed to by <see cref="AnalysisEntity"/> and <see cref="IOperation"/> instances.
    /// </summary>
    internal partial class PointsToAnalysis : ForwardDataFlowAnalysis<PointsToAnalysisData, PointsToBlockAnalysisResult, PointsToAbstractValue>
    {
        public static readonly AbstractValueDomain<PointsToAbstractValue> PointsToAbstractValueDomainInstance = PointsToAbstractValueDomain.Default;

        private PointsToAnalysis(PointsToAnalysisDomain analysisDomain, PointsToDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResultOpt = null,
            DataFlowAnalysisResult<CopyAnalysis.CopyBlockAnalysisResult, CopyAnalysis.CopyAbstractValue> copyAnalysisResultOpt = null,
            bool pessimisticAnalysis = true)
        {
            var defaultPointsToValueGenerator = new DefaultPointsToValueGenerator();
            var analysisDomain = new PointsToAnalysisDomain(defaultPointsToValueGenerator, PointsToAbstractValueDomainInstance);
            var operationVisitor = new PointsToDataFlowOperationVisitor(analysisDomain.DefaultPointsToValueGenerator, analysisDomain,
                PointsToAbstractValueDomain.Default, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, copyAnalysisResultOpt);
            var pointsToAnalysis = new PointsToAnalysis(analysisDomain, operationVisitor);
            return pointsToAnalysis.GetOrComputeResultCore(cfg, cacheResult: true, seedResultOpt: pointsToAnalysisResultOpt);
        }

        internal override PointsToBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<PointsToAnalysisData> blockAnalysisData) => new PointsToBlockAnalysisResult(basicBlock, blockAnalysisData, ((PointsToAnalysisDomain)AnalysisDomain).DefaultPointsToValueGenerator.GetDefaultPointsToValueMap());
        protected override PointsToAnalysisData GetInputData(PointsToBlockAnalysisResult result) => result.InputData;
    }
}
