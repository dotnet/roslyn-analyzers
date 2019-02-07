// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// Base class for insecure deserializer analyzers.
    /// </summary>
    /// <remarks>This aids in implementing:
    /// 1. SerializationBinder not set at the time of deserialization.
    /// </remarks>
    public abstract class DoNotUseInsecureDeserializerWithoutBinderBase : DiagnosticAnalyzer
    {
        /// <summary>
        /// Metadata name of the potentially insecure deserializer type.
        /// </summary>
        protected abstract string DeserializerTypeMetadataName { get; }

        /// <summary>
        /// Name of the <see cref="System.Runtime.Serialization.SerializationBinder"/> property.
        /// </summary>
        protected abstract string SerializationBinderPropertyMetadataName { get; }

        /// <summary>
        /// Metadata names of deserialization methods.
        /// </summary>
        /// <remarks>Use <see cref="StringComparer.Ordinal"/>.</remarks>
        protected abstract ImmutableHashSet<string> DeserializationMethodNames { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a deserialization method is invoked and its Binder property is definitely not set.
        /// </summary>
        /// <remarks>The string format message argument is the method signature.</remarks>
        protected abstract DiagnosticDescriptor BinderDefinitelyNotSetDescriptor { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a deserialization method is invoked and its Binder property is possibly not set.
        /// </summary>
        /// <remarks>The string format message argument is the method signature.</remarks>
        protected abstract DiagnosticDescriptor BinderMaybeNotSetDescriptor { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create<DiagnosticDescriptor>(
                this.BinderDefinitelyNotSetDescriptor,
                this.BinderMaybeNotSetDescriptor);

        /// <summary>
        /// For PropertySetAnalysis dataflow analysis; new instances always start out as flagged.
        /// </summary>
        private static readonly ConstructorMapper ConstructorMapper = new ConstructorMapper(ImmutableArray.Create(PropertySetAbstractValueKind.Flagged));

        /// <summary>
        /// For PropertySetAnalysis dataflow analysis; only tracking one property, the <see cref="SerializationBinderPropertyMetadataName"/>.
        /// </summary>
        private const int SerializationBinderIndex = 0;

        /// <summary>
        /// For PropertySetAnalysis dataflow analysis; hazardous usage evaluation callback.
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <param name="propertySetAbstractValue"></param>
        /// <returns></returns>
        private static HazardousUsageEvaluationResult HazardousIfNull(IMethodSymbol methodSymbol, PropertySetAbstractValue propertySetAbstractValue)
        {
            switch (propertySetAbstractValue[SerializationBinderIndex])
            {
                case PropertySetAbstractValueKind.Flagged:
                    return HazardousUsageEvaluationResult.Flagged;
                case PropertySetAbstractValueKind.Unflagged:
                    return HazardousUsageEvaluationResult.Unflagged;
                default:
                    return HazardousUsageEvaluationResult.MaybeFlagged;
            }
        }

        public sealed override void Initialize(AnalysisContext context)
        {
            ImmutableHashSet<string> cachedDeserializationMethodNames = this.DeserializationMethodNames;

            Debug.Assert(!String.IsNullOrWhiteSpace(this.DeserializerTypeMetadataName));
            Debug.Assert(!String.IsNullOrWhiteSpace(this.SerializationBinderPropertyMetadataName));
            Debug.Assert(cachedDeserializationMethodNames != null);
            Debug.Assert(!cachedDeserializationMethodNames.IsEmpty);
            Debug.Assert(this.BinderDefinitelyNotSetDescriptor != null);
            Debug.Assert(this.BinderMaybeNotSetDescriptor != null);

            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            // For PropertySetAnalysis dataflow analysis.
            PropertyMapperCollection propertyMappers = new PropertyMapperCollection(
                new PropertyMapper(
                    this.SerializationBinderPropertyMetadataName,
                    (NullAbstractValue nullAbstractValue) =>
                    {
                        // A null SerializationBinder is what we want to flag as hazardous.
                        switch (nullAbstractValue)
                        {
                            case NullAbstractValue.Null:
                                return PropertySetAbstractValueKind.Flagged;

                            case NullAbstractValue.NotNull:
                                return PropertySetAbstractValueKind.Unflagged;

                            default:
                                return PropertySetAbstractValueKind.MaybeFlagged;
                        }
                    }));

            HazardousUsageEvaluatorCollection hazardousUsageEvaluators =
                new HazardousUsageEvaluatorCollection(
                    cachedDeserializationMethodNames.Select(
                        methodName => new HazardousUsageEvaluator(methodName, DoNotUseInsecureDeserializerWithoutBinderBase.HazardousIfNull)));

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    WellKnownTypeProvider wellKnownTypeProvider =
                        WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(
                            this.DeserializerTypeMetadataName,
                            out INamedTypeSymbol deserializerTypeSymbol))
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            HashSet<IOperation> rootOperationsNeedingAnalysis = new HashSet<IOperation>();

                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    IInvocationOperation invocationOperation =
                                        (IInvocationOperation)operationAnalysisContext.Operation;
                                    if (invocationOperation.Instance?.Type == deserializerTypeSymbol
                                        && cachedDeserializationMethodNames.Contains(invocationOperation.TargetMethod.Name))
                                    {
                                        rootOperationsNeedingAnalysis.Add(operationAnalysisContext.Operation.GetRoot());
                                    }
                                },
                                OperationKind.Invocation);

                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    IMethodReferenceOperation methodReferenceOperation =
                                        (IMethodReferenceOperation)operationAnalysisContext.Operation;
                                    if (methodReferenceOperation.Instance?.Type == deserializerTypeSymbol
                                       && cachedDeserializationMethodNames.Contains(
                                            methodReferenceOperation.Method.MetadataName))
                                    {
                                        rootOperationsNeedingAnalysis.Add(operationAnalysisContext.Operation.GetRoot());
                                    }
                                },
                                OperationKind.MethodReference);

                            operationBlockStartAnalysisContext.RegisterOperationBlockEndAction(
                                (OperationBlockAnalysisContext operationBlockAnalysisContext) =>
                                {
                                    if (!rootOperationsNeedingAnalysis.Any())
                                    {
                                        return;
                                    }

                                    // Only instantiated if there are any results to report.
                                    Dictionary<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> allResults = null;
                                    List<ControlFlowGraph> cfgs = new List<ControlFlowGraph>();

                                    var interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
                                        operationBlockAnalysisContext.Options, SupportedDiagnostics,
                                        defaultInterproceduralAnalysisKind: InterproceduralAnalysisKind.None,
                                        cancellationToken: operationBlockAnalysisContext.CancellationToken,
                                        defaultMaxInterproceduralMethodCallChain: 1); // By default, we only want to track method calls one level down.

                                    foreach (IOperation rootOperation in rootOperationsNeedingAnalysis)
                                    {
                                        ImmutableDictionary<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> dfaResult =
                                            PropertySetAnalysis.GetOrComputeHazardousUsages(
                                                rootOperation.GetEnclosingControlFlowGraph(),
                                                operationBlockAnalysisContext.Compilation,
                                                operationBlockAnalysisContext.OwningSymbol,
                                                this.DeserializerTypeMetadataName,
                                                DoNotUseInsecureDeserializerWithoutBinderBase.ConstructorMapper,
                                                propertyMappers,
                                                hazardousUsageEvaluators,
                                                interproceduralAnalysisConfig);
                                        if (dfaResult.IsEmpty)
                                        {
                                            continue;
                                        }

                                        if (allResults == null)
                                        {
                                            allResults = new Dictionary<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult>();
                                        }

                                        foreach (KeyValuePair<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> kvp
                                            in dfaResult)
                                        {
                                            allResults.Add(kvp.Key, kvp.Value);
                                        }
                                    }

                                    if (allResults == null)
                                    {
                                        return;
                                    }

                                    foreach (KeyValuePair<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> kvp
                                        in allResults)
                                    {
                                        DiagnosticDescriptor descriptor;
                                        switch (kvp.Value)
                                        {
                                            case HazardousUsageEvaluationResult.Flagged:
                                                descriptor = this.BinderDefinitelyNotSetDescriptor;
                                                break;

                                            case HazardousUsageEvaluationResult.MaybeFlagged:
                                                descriptor = this.BinderMaybeNotSetDescriptor;
                                                break;

                                            default:
                                                Debug.Fail($"Unhandled abstract value {kvp.Value}");
                                                continue;
                                        }

                                        operationBlockAnalysisContext.ReportDiagnostic(
                                            Diagnostic.Create(
                                                descriptor,
                                                kvp.Key.Location,
                                                kvp.Key.Method.ToDisplayString(
                                                    SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                    }
                                });
                        });
                });
        }
    }
}
