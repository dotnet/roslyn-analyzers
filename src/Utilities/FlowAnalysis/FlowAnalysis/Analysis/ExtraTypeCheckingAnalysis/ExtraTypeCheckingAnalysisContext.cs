// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T>

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System;
    using Analyzer.Utilities;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
    using CopyAnalysisResult = Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DataFlowAnalysisResult<Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis.CopyBlockAnalysisResult, Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis.CopyAbstractValue>;
    using InterproceduralExtraTypeCheckingAccessAnalysisData = InterproceduralAnalysisData<ExtraTypeCheckingAnalysisData, ExtraTypeCheckingAnalysisContext, SimpleAbstractValue>;

    /// <summary>
    /// Analysis context for execution of <see cref="ExtraTypeCheckingAnalysis"/> on a control flow graph.
    /// </summary>
    internal sealed class ExtraTypeCheckingAnalysisContext : AbstractDataFlowAnalysisContext<ExtraTypeCheckingAnalysisData, ExtraTypeCheckingAnalysisContext, ExtraTypeCheckingAnalysisResult, SimpleAbstractValue>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysisContext"/> class.
        /// </summary>
        /// <param name="valueDomain">The value domain.</param>
        /// <param name="wellKnownTypeProvider">The well known type provider.</param>
        /// <param name="controlFlowGraph">The call flow graph.</param>
        /// <param name="owningSymbol">The owning symbol.</param>
        /// <param name="analyzerOptions">Analyzer options.</param>
        /// <param name="interproceduralAnalysisConfig">The interprocedural analysis config.</param>
        /// <param name="pessimisticAnalysis">Enables pessimistic analysis.</param>
        /// <param name="pointsToAnalysisResult">Points to analysis information.</param>
        /// <param name="valueContentAnalysisResult">result value.</param>
        /// <param name="getOrComputeAnalysisResult">Get or compute analysis information.</param>
        /// <param name="parentControlFlowGraph">Parent control flow graph.</param>
        /// <param name="interproceduralAnalysisData">Interprocedural analysis data.</param>
        private ExtraTypeCheckingAnalysisContext(
            AbstractValueDomain<SimpleAbstractValue> valueDomain,
            WellKnownTypeProvider wellKnownTypeProvider,
            ControlFlowGraph controlFlowGraph,
            ISymbol owningSymbol,
            AnalyzerOptions analyzerOptions,
            InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
            bool pessimisticAnalysis,
            PointsToAnalysisResult? pointsToAnalysisResult,
            DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>? valueContentAnalysisResult,
            Func<ExtraTypeCheckingAnalysisContext, ExtraTypeCheckingAnalysisResult?> getOrComputeAnalysisResult,
            ControlFlowGraph? parentControlFlowGraph,
            InterproceduralExtraTypeCheckingAccessAnalysisData? interproceduralAnalysisData)
            : base(
                valueDomain,
                wellKnownTypeProvider,
                controlFlowGraph,
                owningSymbol,
                analyzerOptions,
                interproceduralAnalysisConfig,
                pessimisticAnalysis,
                predicateAnalysis: false,
                exceptionPathsAnalysis: false,
                copyAnalysisResult: null,
                pointsToAnalysisResult: pointsToAnalysisResult,
                valueContentAnalysisResult: valueContentAnalysisResult,
                tryGetOrComputeAnalysisResult: getOrComputeAnalysisResult,
                parentControlFlowGraph: parentControlFlowGraph,
                interproceduralAnalysisData: interproceduralAnalysisData,
                interproceduralAnalysisPredicate: null)
        {
        }

        /// <summary>
        /// Creates a new analysis context.
        /// </summary>
        /// <param name="valueDomain">The value domain.</param>
        /// <param name="wellKnownTypeProvider">The well known type provider.</param>
        /// <param name="controlFlowGraph">The control flow graph.</param>
        /// <param name="owningSymbol">The owning symbol.</param>
        /// <param name="analyzerOptions">Analyzer options.</param>
        /// <param name="interproceduralAnalysisConfig">The interprocedural analysis config.</param>
        /// <param name="pessimisticAnalysis">Pessimistic analysis enabled setting.</param>
        /// <param name="pointsToAnalysisResult">Points to analysis information.</param>
        /// <param name="getOrComputeAnalysisResult">Compute analysis result information.</param>
        /// <returns>The context for analyzing extra dictionary accesses.</returns>
        public static ExtraTypeCheckingAnalysisContext Create(
            AbstractValueDomain<SimpleAbstractValue> valueDomain,
            WellKnownTypeProvider wellKnownTypeProvider,
            ControlFlowGraph controlFlowGraph,
            ISymbol owningSymbol,
            AnalyzerOptions analyzerOptions,
            InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
            bool pessimisticAnalysis,
            PointsToAnalysisResult pointsToAnalysisResult,
            Func<ExtraTypeCheckingAnalysisContext, ExtraTypeCheckingAnalysisResult?> getOrComputeAnalysisResult)
        {
            return new ExtraTypeCheckingAnalysisContext(
                valueDomain,
                wellKnownTypeProvider,
                controlFlowGraph,
                owningSymbol,
                analyzerOptions,
                interproceduralAnalysisConfig,
                pessimisticAnalysis,
                pointsToAnalysisResult,
                default,
                getOrComputeAnalysisResult,
                parentControlFlowGraph: default,
                interproceduralAnalysisData: default);
        }

        /// <summary>
        /// Forks for interprocedural analysis.
        /// </summary>
        /// <param name="invokedMethod">The invoke method.</param>
        /// <param name="invokedControlFlowGraph">The invoked control flow graph.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="pointsToAnalysisResult">The points to analysis data.</param>
        /// <param name="copyAnalysisResult">The copy analysis information.</param>
        /// <param name="valueContentAnalysisResult">The value content analysis result opt.</param>
        /// <param name="interproceduralAnalysisData">Interprocedural analysis data.</param>
        /// <returns>The context data for the forked analysis.</returns>
        public override ExtraTypeCheckingAnalysisContext ForkForInterproceduralAnalysis(
            IMethodSymbol invokedMethod,
            ControlFlowGraph invokedControlFlowGraph,
            IOperation operation,
            PointsToAnalysisResult? pointsToAnalysisResult,
            CopyAnalysisResult? copyAnalysisResult,
            DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>? valueContentAnalysisResult,
            InterproceduralExtraTypeCheckingAccessAnalysisData? interproceduralAnalysisData)
        {
            return new ExtraTypeCheckingAnalysisContext(
                this.ValueDomain,
                this.WellKnownTypeProvider,
                invokedControlFlowGraph,
                invokedMethod,
                this.AnalyzerOptions,
                this.InterproceduralAnalysisConfiguration,
                this.PessimisticAnalysis,
                pointsToAnalysisResult,
                valueContentAnalysisResult,
                this.TryGetOrComputeAnalysisResult,
                this.ControlFlowGraph,
                interproceduralAnalysisData);
        }

        // <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets object specific hash code parts.
        /// </summary>
        /// <param name="action">Action that accumulates hash codes to combine.</param>
        protected override void ComputeHashCodePartsSpecific(Action<int> action)
        {
            // There is nothing unique in this subclass to update the hashcode with.
        }
    }
}
