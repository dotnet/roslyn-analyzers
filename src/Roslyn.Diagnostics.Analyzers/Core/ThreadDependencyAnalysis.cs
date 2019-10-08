// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    internal partial class ThreadDependencyAnalysis
        : ForwardDataFlowAnalysis<ThreadDependencyAnalysis.Data, ThreadDependencyAnalysis.Context, ThreadDependencyAnalysis.Result, ThreadDependencyAnalysis.BlockResult, ThreadDependencyAnalysis.Value>
    {
        private ThreadDependencyAnalysis(Domain analysisDomain, OperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        public static Result TryGetOrComputeResult(
            ControlFlowGraph controlFlowGraph,
            ISymbol owningSymbol,
            AnalyzerOptions analyzerOptions,
            WellKnownTypeProvider wellKnownTypeProvider,
            InterproceduralAnalysisConfiguration interproceduralAnalysisConfig)
        {
            var context = Context.Create(controlFlowGraph, owningSymbol, analyzerOptions, wellKnownTypeProvider, interproceduralAnalysisConfig, TryGetOrComputeResultForAnalysisContext);
            return TryGetOrComputeResultForAnalysisContext(context);
        }

        private static Result TryGetOrComputeResultForAnalysisContext(Context context)
        {
            var domain = new Domain();
            var operationVisitor = new OperationVisitor(context);
            var analysis = new ThreadDependencyAnalysis(domain, operationVisitor);
            return analysis.TryGetOrComputeResultCore(context, cacheResult: true);
        }

        protected override BlockResult ToBlockResult(BasicBlock basicBlock, Data blockAnalysisData)
        {
            return new BlockResult(basicBlock);
        }

        protected override Result ToResult(Context analysisContext, DataFlowAnalysisResult<BlockResult, Value> dataFlowAnalysisResult)
        {
            return new Result(dataFlowAnalysisResult);
        }
    }
}
