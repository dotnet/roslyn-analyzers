// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System.Threading;
    using Analyzer.Utilities;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
    using PointsToAnalysis = Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

    /// <summary>
    /// Dataflow analysis to track dictionary access state of <see cref="AbstractLocation"/>/<see cref="IOperation"/> instances.
    /// </summary>
    internal partial class ExtraTypeCheckingAnalysis : ForwardDataFlowAnalysis<ExtraTypeCheckingAnalysisData, ExtraTypeCheckingAnalysisContext, ExtraTypeCheckingAnalysisResult, ExtraTypeCheckingBlockAnalysisResult, SimpleAbstractValue>
    {
        /// <summary>
        /// The domain instance.
        /// </summary>
        public static readonly ExtraTypeCheckingAnalysisDomain ExtraTypeCheckingAnalysisDomainInstance =
            new ExtraTypeCheckingAnalysisDomain(SimpleAbstractValueDomain.Default);

        /// <summary>
        /// Indicates wheither pessimistic analysis should take place.
        /// </summary>
        /// <remarks>
        /// Invoking an instance method may likely invalidate all the instance field analysis state, i.e.
        /// reference type fields might be re-assigned to point to different objects in the called method.
        /// An optimistic points to analysis assumes that the points to values of instance fields don't change on invoking an instance method.
        /// A pessimistic points to analysis resets all the instance state and assumes the instance field might point to any object, hence has unknown state.
        /// For dispose analysis, we want to perform an optimistic points to analysis as we assume a disposable field is not likely to be re-assigned to a separate object in helper method invocations in Dispose.
        /// </remarks>
        private const bool PessimisticAnalysis = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysis"/> class.
        /// </summary>
        /// <param name="analysisDomain">The analysis domain.</param>
        /// <param name="operationVisitor">The operation visitor.</param>
        private ExtraTypeCheckingAnalysis(ExtraTypeCheckingAnalysisDomain analysisDomain, ExtraTypeCheckingDataFlowOperationVisitor operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        /// <summary>
        /// Does the analysis of dictionary accesses.
        /// </summary>
        /// <param name="cfg">The call flow graph.</param>
        /// <param name="owningSymbol">The owning symbol.</param>
        /// <param name="wellKnownTypeProvider">The well known type provider.</param>
        /// <param name="pointsToAnalysisKind">Points to analysis kind.</param>
        /// <param name="analyzerOptions">Analyzer options.</param>
        /// <param name="rule">The rule.</param>
        /// <param name="compilation">The compilation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="interproceduralAnalysisKind">Intraprocedural analysis kind.</param>
        /// <returns>Returns the results of analyzing dictionary accesses.</returns>
        public static ExtraTypeCheckingAnalysisResult? GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            PointsToAnalysisKind pointsToAnalysisKind,
            AnalyzerOptions analyzerOptions,
            DiagnosticDescriptor rule,
            Compilation compilation,
            CancellationToken cancellationToken,
            InterproceduralAnalysisKind interproceduralAnalysisKind = InterproceduralAnalysisKind.None)
        {
            InterproceduralAnalysisConfiguration interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
                analyzerOptions,
                rule,
                owningSymbol,
                compilation,
                interproceduralAnalysisKind,
                cancellationToken);

            return GetOrComputeResult(
                cfg,
                owningSymbol,
                wellKnownTypeProvider,
                pointsToAnalysisKind,
                interproceduralAnalysisConfig,
                analyzerOptions);
        }

        /// <inheritdoc />
        protected override ExtraTypeCheckingAnalysisResult ToResult(ExtraTypeCheckingAnalysisContext analysisContext, DataFlowAnalysisResult<ExtraTypeCheckingBlockAnalysisResult, SimpleAbstractValue> dataFlowAnalysisResult)
        {
            return new ExtraTypeCheckingAnalysisResult(dataFlowAnalysisResult);
        }

        /// <inheritdoc />
        protected override ExtraTypeCheckingBlockAnalysisResult ToBlockResult(BasicBlock basicBlock, ExtraTypeCheckingAnalysisData blockAnalysisData)
            => new ExtraTypeCheckingBlockAnalysisResult(basicBlock, blockAnalysisData);

        /// <summary>
        /// Does the analysis of dictionary accesses.
        /// </summary>
        /// <param name="cfg">The call flow graph.</param>
        /// <param name="owningSymbol">The owning symbol.</param>
        /// <param name="wellKnownTypeProvider">The well known type provider.</param>
        /// <param name="pointsToAnalysisKind">The points to analysis kind.</param>
        /// <param name="interproceduralAnalysisConfig">The inter procedural analysis configuration.</param>
        /// <param name="analyzerOptions">The analyzer options.</param>
        /// <returns>Returns the results of analyzing dictionary accesses.</returns>
        private static ExtraTypeCheckingAnalysisResult? GetOrComputeResult(
            ControlFlowGraph cfg,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            PointsToAnalysisKind pointsToAnalysisKind,
            InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
            AnalyzerOptions analyzerOptions)
        {
            if (cfg == null ||
                owningSymbol == null)
            {
                return default;
            }

            PointsToAnalysisResult? pointsToAnalysisResult = PointsToAnalysis.PointsToAnalysis.TryGetOrComputeResult(
                cfg,
                owningSymbol,
                analyzerOptions,
                wellKnownTypeProvider,
                pointsToAnalysisKind,
                interproceduralAnalysisConfig,
                interproceduralAnalysisPredicate: null,
                pessimisticAnalysis: PessimisticAnalysis);

            if (pointsToAnalysisResult == null)
            {
                // This happens when an unknown C# construct is found or
                // analysis cannot be done on the only entry we are interested in.
                return default;
            }

            using ExtraTypeCheckingAnalysisContext analysisContext = ExtraTypeCheckingAnalysisContext.Create(
                SimpleAbstractValueDomain.Default,
                wellKnownTypeProvider,
                cfg,
                owningSymbol,
                analyzerOptions,
                interproceduralAnalysisConfig,
                PessimisticAnalysis,
                pointsToAnalysisResult,
                GetOrComputeResultForAnalysisContext);
            return GetOrComputeResultForAnalysisContext(analysisContext);
        }

        /// <summary>
        /// Does the analysis of dictionary accesses.
        /// </summary>
        /// <param name="analysisContext">The analysis context.</param>
        /// <returns>The analysis result.</returns>
        private static ExtraTypeCheckingAnalysisResult? GetOrComputeResultForAnalysisContext(
            ExtraTypeCheckingAnalysisContext analysisContext)
        {
            ExtraTypeCheckingDataFlowOperationVisitor operationVisitor = new ExtraTypeCheckingDataFlowOperationVisitor(analysisContext);
            ExtraTypeCheckingAnalysis analysis = new ExtraTypeCheckingAnalysis(ExtraTypeCheckingAnalysisDomainInstance, operationVisitor);
            return analysis.TryGetOrComputeResultCore(analysisContext, cacheResult: false);
        }
    }
}
