// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class Context : AbstractDataFlowAnalysisContext<Data, Context, Result, Value>
        {
            private Context(
                AbstractValueDomain<Value> valueDomain,
                WellKnownTypeProvider wellKnownTypeProvider,
                ControlFlowGraph controlFlowGraph,
                ISymbol owningSymbol,
                AnalyzerOptions analyzerOptions,
                InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
                bool pessimisticAnalysis,
                bool predicateAnalysis,
                bool exceptionPathsAnalysis,
                DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue> copyAnalysisResultOpt,
                PointsToAnalysisResult pointsToAnalysisResultOpt,
                DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue> valueContentAnalysisResultOpt,
                Func<Context, Result> tryGetOrComputeAnalysisResult,
                ControlFlowGraph parentControlFlowGraphOpt,
                InterproceduralAnalysisData<Data, Context, Value> interproceduralAnalysisDataOpt,
                InterproceduralAnalysisPredicate interproceduralAnalysisPredicateOpt)
                : base(valueDomain, wellKnownTypeProvider, controlFlowGraph, owningSymbol, analyzerOptions, interproceduralAnalysisConfig, pessimisticAnalysis, predicateAnalysis, exceptionPathsAnalysis, copyAnalysisResultOpt, pointsToAnalysisResultOpt, valueContentAnalysisResultOpt, tryGetOrComputeAnalysisResult, parentControlFlowGraphOpt, interproceduralAnalysisDataOpt, interproceduralAnalysisPredicateOpt)
            {
            }

            internal static Context Create(
                ControlFlowGraph controlFlowGraph,
                ISymbol owningSymbol,
                AnalyzerOptions analyzerOptions,
                WellKnownTypeProvider wellKnownTypeProvider,
                InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
                Func<Context, Result> tryGetOrComputeResultForAnalysisContext)
            {
                return new Context(
                    valueDomain: ThreadDependencyAnalysis.ValueDomain.Default,
                    wellKnownTypeProvider,
                    controlFlowGraph,
                    owningSymbol,
                    analyzerOptions,
                    interproceduralAnalysisConfig,
                    pessimisticAnalysis: true,
                    predicateAnalysis: true,
                    exceptionPathsAnalysis: true,
                    copyAnalysisResultOpt: null,
                    pointsToAnalysisResultOpt: null,
                    valueContentAnalysisResultOpt: null,
                    tryGetOrComputeAnalysisResult: tryGetOrComputeResultForAnalysisContext,
                    parentControlFlowGraphOpt: null,
                    interproceduralAnalysisDataOpt: null,
                    interproceduralAnalysisPredicateOpt: null);
            }

            public override Context ForkForInterproceduralAnalysis(
                IMethodSymbol invokedMethod,
                ControlFlowGraph invokedCfg,
                IOperation operation,
                PointsToAnalysisResult pointsToAnalysisResultOpt,
                DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue> copyAnalysisResultOpt,
                DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue> valueContentAnalysisResultOpt,
                InterproceduralAnalysisData<Data, Context, Value> interproceduralAnalysisData)
            {
                return new Context(
                    ValueDomain,
                    WellKnownTypeProvider,
                    invokedCfg,
                    invokedMethod,
                    AnalyzerOptions,
                    InterproceduralAnalysisConfiguration,
                    PessimisticAnalysis,
                    PredicateAnalysis,
                    ExceptionPathsAnalysis,
                    copyAnalysisResultOpt,
                    pointsToAnalysisResultOpt,
                    valueContentAnalysisResultOpt,
                    TryGetOrComputeAnalysisResult,
                    ParentControlFlowGraphOpt,
                    interproceduralAnalysisData,
                    InterproceduralAnalysisPredicateOpt);
            }

            protected override void ComputeHashCodePartsSpecific(Action<int> builder)
            {
            }
        }
    }
}
