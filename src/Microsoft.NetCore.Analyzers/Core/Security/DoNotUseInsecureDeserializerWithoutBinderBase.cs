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
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
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
                                    if (invocationOperation.TargetMethod.ContainingType == deserializerTypeSymbol
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
                                    if (methodReferenceOperation.Method.ContainingType == deserializerTypeSymbol
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
                                    Dictionary<OperationMethodKey, PropertySetAbstractValue> allResults = null;

                                    foreach (IOperation rootOperation in rootOperationsNeedingAnalysis)
                                    {
                                        ImmutableDictionary<OperationMethodKey, PropertySetAbstractValue> dfaResult =
                                            PropertySetAnalysis.GetOrComputeHazardousParameterUsages(
                                                rootOperation.GetEnclosingControlFlowGraph(),
                                                operationBlockAnalysisContext.Compilation,
                                                operationBlockAnalysisContext.OwningSymbol,
                                                this.DeserializerTypeMetadataName,
                                                true /* isNewInstanceFlagged */,
                                                this.SerializationBinderPropertyMetadataName,
                                                true /* isNullPropertyFlagged */,
                                                cachedDeserializationMethodNames);
                                        if (!dfaResult.Any())
                                        {
                                            continue;
                                        }

                                        if (allResults == null)
                                        {
                                            allResults = new Dictionary<OperationMethodKey, PropertySetAbstractValue>();
                                        }

                                        foreach (KeyValuePair<OperationMethodKey, PropertySetAbstractValue> kvp in dfaResult)
                                        {
                                            allResults.Add(kvp.Key, kvp.Value);
                                        }
                                    }

                                    if (allResults == null)
                                    {
                                        return;
                                    }

                                    foreach (OperationMethodKey operationMethodKey in allResults.Keys.OrderBy(OperationMethodKey.Comparer))
                                    {
                                        PropertySetAbstractValue abstractValue = allResults[operationMethodKey];
                                        DiagnosticDescriptor descriptor;
                                        switch (abstractValue)
                                        {
                                            case PropertySetAbstractValue.Flagged:
                                                descriptor = this.BinderDefinitelyNotSetDescriptor;
                                                break;

                                            case PropertySetAbstractValue.MaybeFlagged:
                                                descriptor = this.BinderMaybeNotSetDescriptor;
                                                break;

                                            default:
                                                Debug.Assert(false, $"Unhandled abstract value {abstractValue}");
                                                continue;
                                        }

                                        operationBlockAnalysisContext.ReportDiagnostic(
                                            Diagnostic.Create(
                                                descriptor,
                                                operationMethodKey.Operation.Syntax.GetLocation(),
                                                operationMethodKey.Method.ToDisplayString(
                                                    SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                    }
                                });
                        });
                });
        }
    }
}
