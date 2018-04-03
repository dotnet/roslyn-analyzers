// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis
{
    using CopyAnalysisData = IDictionary<AnalysisEntity, CopyAbstractValue>;
    
    /// <summary>
    /// Dataflow analysis to track <see cref="AnalysisEntity"/> instances that share the same value.
    /// </summary>
    internal partial class CopyAnalysis : ForwardDataFlowAnalysis<CopyAnalysisData, CopyBlockAnalysisResult, CopyAbstractValue>
    {
        private CopyAnalysis(CopyAnalysisDomain analysisDomain, CopyDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt = null,
            bool pessimisticAnalysis = true)
        {
            var operationVisitor = new CopyDataFlowOperationVisitor(CopyAbstractValueDomain.Default, owningSymbol, 
                wellKnownTypeProvider, pessimisticAnalysis, nullAnalysisResultOpt, pointsToAnalysisResultOpt);
            var copyAnalysis = new CopyAnalysis(CopyAnalysisDomain.Instance, operationVisitor);
            return copyAnalysis.GetOrComputeResultCore(cfg, cacheResult: true);
        }

        [Conditional("DEBUG")]
        public static void AssertValidCopyAnalysisEntity(AnalysisEntity analysisEntity)
        {
            Debug.Assert(!analysisEntity.HasUnknownInstanceLocation, "Don't track entities if do not know about it's instance location");
        }

        [Conditional("DEBUG")]
        public static void AssertValidCopyAnalysisData(CopyAnalysisData map)
        {
            foreach (var kvp in map)
            {
                AssertValidCopyAnalysisEntity(kvp.Key);
                Debug.Assert(kvp.Value.AnalysisEntities.Contains(kvp.Key));
                foreach (var analysisEntity in kvp.Value.AnalysisEntities)
                {
                    AssertValidCopyAnalysisEntity(analysisEntity);
                    Debug.Assert(map[analysisEntity] == kvp.Value);
                }
            }
        }

        internal override CopyBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<CopyAnalysisData> blockAnalysisData) => new CopyBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
