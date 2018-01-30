// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;
    using StringContentAnalysisDomain = MapAbstractDomain<AnalysisEntity, StringContentAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track string content of <see cref="AnalysisEntity"/>/<see cref="IOperation"/>.
    /// </summary>
    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        private static readonly StringContentAnalysisDomain s_StringContentAnalysisDomain = new StringContentAnalysisDomain(StringContentAbstractValueDomain.Default);

        private StringContentAnalysis(StringContentAnalysisDomain analysisDomain, StringContentDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<StringContentBlockAnalysisResult, StringContentAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            INamedTypeSymbol containingTypeSymbol,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt = null)
        {
            var operationVisitor = new StringContentDataFlowOperationVisitor(StringContentAbstractValueDomain.Default, containingTypeSymbol, nullAnalysisResultOpt, pointsToAnalysisResultOpt);
            var nullAnalysis = new StringContentAnalysis(s_StringContentAnalysisDomain, operationVisitor);
            return nullAnalysis.GetOrComputeResultCore(cfg);
        }

        internal override StringContentBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<StringContentAnalysisData> blockAnalysisData) => new StringContentBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
