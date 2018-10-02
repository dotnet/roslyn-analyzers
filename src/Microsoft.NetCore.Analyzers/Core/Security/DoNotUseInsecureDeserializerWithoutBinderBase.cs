// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
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
        protected abstract ImmutableHashSet<string> DeserializationMethodNames { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a deserialization method is invoked and its Binder property is definitely not set.
        /// </summary>
        protected abstract DiagnosticDescriptor BinderDefinitelyNotSetDescriptor { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a deserialization method is invoked and its Binder property is possibly not set.
        /// </summary>
        protected abstract DiagnosticDescriptor BinderMaybeNotSetDescriptor { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create<DiagnosticDescriptor>(
                BinderDefinitelyNotSetDescriptor,
                BinderMaybeNotSetDescriptor);

        public override void Initialize(AnalysisContext context)
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

                    if (!wellKnownTypeProvider.TryGetKnownType(
                            this.DeserializerTypeMetadataName,
                            out INamedTypeSymbol deserializerTypeSymbol))
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            ISymbol owningSymbol = operationBlockStartAnalysisContext.OwningSymbol;
                            bool requiresBinderMustBeSetDataFlowAnalysis = false;

                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    IInvocationOperation invocationOperation =
                                        (IInvocationOperation) operationAnalysisContext.Operation;
                                    if (invocationOperation.TargetMethod.ContainingType == deserializerTypeSymbol
                                        && cachedDeserializationMethodNames.Contains(invocationOperation.TargetMethod.Name))
                                    {
                                        requiresBinderMustBeSetDataFlowAnalysis = true;
                                    }
                                },
                                OperationKind.Invocation);

                            operationBlockStartAnalysisContext.RegisterOperationBlockEndAction(
                                (OperationBlockAnalysisContext operationBlockAnalysisContext) =>
                                {
                                    if (!requiresBinderMustBeSetDataFlowAnalysis)
                                    {
                                        return;
                                    }

                                    ImmutableDictionary<IInvocationOperation, PropertySetAbstractValue> dfaResult =
                                        PropertySetAnalysis.GetOrComputeHazardousParameterUsages(
                                            operationBlockAnalysisContext.OperationBlocks[0].GetEnclosingControlFlowGraph(),
                                            operationBlockAnalysisContext.Compilation,
                                            operationBlockAnalysisContext.OwningSymbol,
                                            this.DeserializerTypeMetadataName,
                                            true /* isNewInstanceFlagged */,
                                            this.SerializationBinderPropertyMetadataName,
                                            true /* isNullPropertyFlagged */,
                                            cachedDeserializationMethodNames);
                                    foreach (KeyValuePair<IInvocationOperation, PropertySetAbstractValue> kvp in dfaResult)
                                    {
                                        switch (kvp.Value)
                                        {
                                            case PropertySetAbstractValue.Flagged:
                                                operationBlockAnalysisContext.ReportDiagnostic(
                                                    Diagnostic.Create(
                                                        BinderDefinitelyNotSetDescriptor,
                                                        kvp.Key.Syntax.GetLocation(),
                                                        kvp.Key.TargetMethod.ToDisplayString(
                                                            SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                                break;

                                            case PropertySetAbstractValue.MaybeFlagged:
                                                operationBlockAnalysisContext.ReportDiagnostic(
                                                    Diagnostic.Create(
                                                        BinderMaybeNotSetDescriptor,
                                                        kvp.Key.Syntax.GetLocation(),
                                                        kvp.Key.TargetMethod.ToDisplayString(
                                                            SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                                break;

                                            default:
                                                Debug.Assert(false, $"Unhandled abstract value {kvp.Value}");
                                                break;
                                        }
                                    }
                                });
                        });
                });
        }

        /// <summary>
        /// Gets a <see cref="LocalizableResourceString"/> from <see cref="MicrosoftNetCoreSecurityResources"/>.
        /// </summary>
        /// <param name="name">Name of the resource string to retrieve.</param>
        /// <returns>The corresponding <see cref="LocalizableResourceString"/>.</returns>
        protected static LocalizableResourceString GetResourceString(string name)
        {
            return new LocalizableResourceString(
                    name,
                    MicrosoftNetCoreSecurityResources.ResourceManager,
                    typeof(MicrosoftNetCoreSecurityResources));
        }
    }
}
