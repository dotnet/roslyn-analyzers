// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.DisposeAnalysis
{
    using DisposeAnalysisData = IDictionary<AbstractLocation, DisposeAbstractValue>;
    using DisposeAnalysisDomain = MapAbstractDomain<AbstractLocation, DisposeAbstractValue>;

    /// <summary>
    /// Dataflow analysis to track dispose state of <see cref="AbstractLocation"/>/<see cref="IOperation"/> instances.
    /// </summary>
    internal partial class DisposeAnalysis : ForwardDataFlowAnalysis<DisposeAnalysisData, DisposeBlockAnalysisResult, DisposeAbstractValue>
    {
        public static readonly DisposeAnalysisDomain DisposeAnalysisDomainInstance = new DisposeAnalysisDomain(DisposeAbstractValueDomain.Default);
        private DisposeAnalysis(DisposeAnalysisDomain analysisDomain, DisposeDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            INamedTypeSymbol iDisposable,
            INamedTypeSymbol taskType,
            ImmutableHashSet<INamedTypeSymbol> collectionTypes,
            ImmutableHashSet<INamedTypeSymbol> disposeOwnershipTransferLikelyTypes,
            ISymbol owningSymbol,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResult,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null)
        {
            return GetOrComputeResultCore(cfg, iDisposable, taskType, collectionTypes,
                disposeOwnershipTransferLikelyTypes, owningSymbol, pointsToAnalysisResult,
                nullAnalysisResultOpt, trackInstanceFields: false, trackedInstanceFieldPointsToMap: out var _);
        }

        public static DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> GetOrComputeResult(
            ControlFlowGraph cfg,
            INamedTypeSymbol iDisposable,
            INamedTypeSymbol taskType,
            ImmutableHashSet<INamedTypeSymbol> collectionTypes,
            ImmutableHashSet<INamedTypeSymbol> disposeOwnershipTransferLikelyTypes,
            ISymbol owningSymbol,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResult,
            bool trackInstanceFields,
            out ImmutableDictionary<IFieldSymbol, PointsToAnalysis.PointsToAbstractValue> trackedInstanceFieldPointsToMap,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt = null)
        {
            return GetOrComputeResultCore(cfg, iDisposable, taskType, collectionTypes,
                disposeOwnershipTransferLikelyTypes, owningSymbol, pointsToAnalysisResult,
                nullAnalysisResultOpt, trackInstanceFields, out trackedInstanceFieldPointsToMap);
        }

        private static DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> GetOrComputeResultCore(
            ControlFlowGraph cfg,
            INamedTypeSymbol iDisposable,
            INamedTypeSymbol taskType,
            ImmutableHashSet<INamedTypeSymbol> collectionTypes,
            ImmutableHashSet<INamedTypeSymbol> disposeOwnershipTransferLikelyTypes,
            ISymbol owningSymbol,
            DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResult,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt,
            bool trackInstanceFields,
            out ImmutableDictionary<IFieldSymbol, PointsToAnalysis.PointsToAbstractValue> trackedInstanceFieldPointsToMap)
        {
            Debug.Assert(cfg != null);
            Debug.Assert(iDisposable != null);
            Debug.Assert(owningSymbol != null);
            Debug.Assert(pointsToAnalysisResult != null);

            var operationVisitor = new DisposeDataFlowOperationVisitor(iDisposable, taskType, collectionTypes,
                disposeOwnershipTransferLikelyTypes, DisposeAbstractValueDomain.Default, owningSymbol, trackInstanceFields, pointsToAnalysisResult, nullAnalysisResultOpt);
            var disposeAnalysis = new DisposeAnalysis(DisposeAnalysisDomainInstance, operationVisitor);
            var result = disposeAnalysis.GetOrComputeResultCore(cfg);
            trackedInstanceFieldPointsToMap = trackInstanceFields ? operationVisitor.TrackedInstanceFieldPointsToMap : null;
            return result;
        }

        internal override DisposeBlockAnalysisResult ToResult(BasicBlock basicBlock, DataFlowAnalysisInfo<IDictionary<AbstractLocation, DisposeAbstractValue>> blockAnalysisData) => new DisposeBlockAnalysisResult(basicBlock, blockAnalysisData);
    }
}
