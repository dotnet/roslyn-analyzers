// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track string content of <see cref="AnalysisEntity"/>/<see cref="IOperation"/>.
    /// </summary>
    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        private StringContentAnalysis(StringContentAnalysisDomain analysisDomain, StringContentDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<StringContentBlockAnalysisResult, StringContentAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            DataFlowAnalysisResult<CopyAnalysis.CopyBlockAnalysisResult, CopyAnalysis.CopyAbstractValue> copyAnalysisResultOpt,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt = null,
            bool pessimisticAnalsysis = true)
        {
            var operationVisitor = new StringContentDataFlowOperationVisitor(StringContentAbstractValueDomain.Default, owningSymbol,
                wellKnownTypeProvider, pessimisticAnalsysis, copyAnalysisResultOpt, pointsToAnalysisResultOpt);
            var nullAnalysis = new StringContentAnalysis(StringContentAnalysisDomain.Instance, operationVisitor);
            return nullAnalysis.GetOrComputeResultCore(cfg, cacheResult: false);
        }

        internal override StringContentBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<StringContentAnalysisData> blockAnalysisData) => new StringContentBlockAnalysisResult(basicBlock, blockAnalysisData);
        protected override StringContentAnalysisData GetInputData(StringContentBlockAnalysisResult result) => result.InputData;
    }
}
