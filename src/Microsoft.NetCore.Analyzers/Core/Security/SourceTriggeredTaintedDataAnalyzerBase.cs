// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    using ValueContentAnalysisResult = DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>;

    /// <summary>
    /// Base class to aid in implementing tainted data analyzers.
    /// </summary>
    public abstract class SourceTriggeredTaintedDataAnalyzerBase : DiagnosticAnalyzer
    {
        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when tainted data enters a sink.
        /// </summary>
        /// <remarks>Format string arguments are:
        /// 0. Sink symbol.
        /// 1. Method name containing the code where the tainted data enters the sink.
        /// 2. Source symbol.
        /// 3. Method name containing the code where the tainted data came from the source.
        /// </remarks>
        protected abstract DiagnosticDescriptor TaintedDataEnteringSinkDescriptor { get; }

        /// <summary>
        /// Kind of tainted data sink.
        /// </summary>
        protected abstract SinkKind SinkKind { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TaintedDataEnteringSinkDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationContext) =>
                {
                    TaintedDataConfig taintedDataConfig = TaintedDataConfig.GetOrCreate(compilationContext.Compilation);
                    TaintedDataSymbolMap<SourceInfo> sourceInfoSymbolMap = taintedDataConfig.GetSourceSymbolMap(this.SinkKind);
                    if (sourceInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    TaintedDataSymbolMap<SinkInfo> sinkInfoSymbolMap = taintedDataConfig.GetSinkSymbolMap(this.SinkKind);
                    if (sinkInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    compilationContext.RegisterOperationBlockStartAction(
                        operationBlockStartContext =>
                        {
                            ISymbol owningSymbol = operationBlockStartContext.OwningSymbol;
                            Dictionary<IOperation, bool> rootOperationsNeedingAnalysis = new Dictionary<IOperation, bool>();

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    IPropertyReferenceOperation propertyReferenceOperation = (IPropertyReferenceOperation)operationAnalysisContext.Operation;
                                    IOperation rootOperation = operationAnalysisContext.Operation.GetRoot();
                                    if (sourceInfoSymbolMap.IsSourceProperty(propertyReferenceOperation.Property) && !rootOperationsNeedingAnalysis.ContainsKey(rootOperation))
                                    {
                                        rootOperationsNeedingAnalysis[rootOperation] = false;
                                    }
                                },
                                OperationKind.PropertyReference);

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    IInvocationOperation invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;
                                    bool needValueContentAnalysis = false;
                                    if (sourceInfoSymbolMap.IsSourceMethod(invocationOperation.TargetMethod, out var evaluateWithPointsToAnalysis, out var evaluateWithValueContentAnslysis))
                                    {
                                        IOperation rootOperation = operationAnalysisContext.Operation.GetRoot();
                                        PointsToAnalysisResult pointsToAnalysisResult;
                                        ValueContentAnalysisResult valueContentAnalysisResultOpt;
                                        if (evaluateWithPointsToAnalysis.Count != 0)
                                        {
                                            pointsToAnalysisResult = PointsToAnalysis.TryGetOrComputeResult(
                                                rootOperation.GetEnclosingControlFlowGraph(),
                                                owningSymbol,
                                                WellKnownTypeProvider.GetOrCreate(operationAnalysisContext.Compilation),
                                                InterproceduralAnalysisConfiguration.Create(
                                                    operationAnalysisContext.Options,
                                                    SupportedDiagnostics,
                                                    defaultInterproceduralAnalysisKind: InterproceduralAnalysisKind.ContextSensitive,
                                                    cancellationToken: operationAnalysisContext.CancellationToken),
                                                interproceduralAnalysisPredicateOpt: null);
                                            if (pointsToAnalysisResult == null
                                                || !evaluateWithPointsToAnalysis.Any(s => s(
                                                    invocationOperation.Arguments.Select(o => pointsToAnalysisResult[o.Kind, o.Syntax]))))
                                            {
                                                return;
                                            }
                                        }
                                        else if (evaluateWithValueContentAnslysis.Count != 0)
                                        {
                                            valueContentAnalysisResultOpt = ValueContentAnalysis.TryGetOrComputeResult(
                                                rootOperation.GetEnclosingControlFlowGraph(),
                                                owningSymbol,
                                                WellKnownTypeProvider.GetOrCreate(operationAnalysisContext.Compilation),
                                                InterproceduralAnalysisConfiguration.Create(
                                                    operationAnalysisContext.Options,
                                                    SupportedDiagnostics,
                                                    defaultInterproceduralAnalysisKind: InterproceduralAnalysisKind.ContextSensitive,
                                                    cancellationToken: operationAnalysisContext.CancellationToken),
                                                out var copyAnalysisResult,
                                                out pointsToAnalysisResult);
                                            if (valueContentAnalysisResultOpt == null
                                                || !evaluateWithValueContentAnslysis.Any(s => s(
                                                        invocationOperation.Arguments.Select(
                                                            o => pointsToAnalysisResult[o.Kind, o.Syntax]),
                                                        invocationOperation.Arguments.Select(
                                                            o => valueContentAnalysisResultOpt[o.Kind, o.Syntax]))))
                                            {
                                                return;
                                            }

                                            needValueContentAnalysis = true;
                                        }
                                        else
                                        {
                                            return;
                                        }

                                        if (needValueContentAnalysis || !rootOperationsNeedingAnalysis.ContainsKey(rootOperation))
                                        {
                                            rootOperationsNeedingAnalysis[rootOperation] = needValueContentAnalysis;
                                        }
                                    }
                                },
                                OperationKind.Invocation);

                            if (taintedDataConfig.HasTaintArraySource(SinkKind))
                            {
                                operationBlockStartContext.RegisterOperationAction(
                                    operationAnalysisContext =>
                                    {
                                        IArrayInitializerOperation arrayInitializerOperation = (IArrayInitializerOperation)operationAnalysisContext.Operation;
                                        if (sourceInfoSymbolMap.IsSourceArray(arrayInitializerOperation.Parent.Type as IArrayTypeSymbol))
                                        {

                                            rootOperationsNeedingAnalysis[operationAnalysisContext.Operation.GetRoot()] = true;
                                        }
                                    },
                                    OperationKind.ArrayInitializer);
                            }

                            operationBlockStartContext.RegisterOperationBlockEndAction(
                                operationBlockAnalysisContext =>
                                {
                                    if (!rootOperationsNeedingAnalysis.Any())
                                    {
                                        return;
                                    }

                                    foreach (KeyValuePair<IOperation, bool> kvp in rootOperationsNeedingAnalysis)
                                    {
                                        TaintedDataAnalysisResult taintedDataAnalysisResult = TaintedDataAnalysis.TryGetOrComputeResult(
                                            kvp.Key.GetEnclosingControlFlowGraph(),
                                            operationBlockAnalysisContext.Compilation,
                                            operationBlockAnalysisContext.OwningSymbol,
                                            operationBlockAnalysisContext.Options,
                                            TaintedDataEnteringSinkDescriptor,
                                            sourceInfoSymbolMap,
                                            taintedDataConfig.GetSanitizerSymbolMap(this.SinkKind),
                                            sinkInfoSymbolMap,
                                            operationBlockAnalysisContext.CancellationToken,
                                            kvp.Value);
                                        if (taintedDataAnalysisResult == null)
                                        {
                                            return;
                                        }

                                        foreach (TaintedDataSourceSink sourceSink in taintedDataAnalysisResult.TaintedDataSourceSinks)
                                        {
                                            if (!sourceSink.SinkKinds.Contains(this.SinkKind))
                                            {
                                                continue;
                                            }

                                            foreach (SymbolAccess sourceOrigin in sourceSink.SourceOrigins)
                                            {
                                                // Something like:
                                                // CA3001: Potential SQL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
                                                Diagnostic diagnostic = Diagnostic.Create(
                                                    this.TaintedDataEnteringSinkDescriptor,
                                                    sourceSink.Sink.Location,
                                                    additionalLocations: new Location[] { sourceOrigin.Location },
                                                    messageArgs: new object[] {
                                                        sourceSink.Sink.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                        sourceSink.Sink.AccessingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                        sourceOrigin.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                        sourceOrigin.AccessingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)});
                                                operationBlockAnalysisContext.ReportDiagnostic(diagnostic);
                                            }
                                        }
                                    }
                                });
                        });
                });
        }
    }
}