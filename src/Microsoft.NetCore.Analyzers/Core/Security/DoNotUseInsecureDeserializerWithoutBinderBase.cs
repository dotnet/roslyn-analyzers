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
    /// 1. Banned methods.
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
        /// Metadata names of banned methods, which should not be used at all.
        /// </summary>
        protected abstract ImmutableHashSet<string> DeserializationMethodNames { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a deserialization method is invoked and its Binder property is definitely not set.
        /// </summary>
        protected abstract DiagnosticDescriptor BinderDefinitelyNotSetDescriptor { get; }

        /// <summary>
        /// <see cref="DiagnosticDescriptor"/> for when a deserialization method is invoked and its Binder property is definitely not set.
        /// </summary>
        protected abstract DiagnosticDescriptor BinderMaybeNotSetDescriptor { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create<DiagnosticDescriptor>(
                BinderDefinitelyNotSetDescriptor,
                BinderMaybeNotSetDescriptor);

        // Statically cache things, so derived classes can be lazy and just return a new collection
        // everytime in their BannedMethodNames, etc overrides.
        private static object StaticCacheInitializationLock = new object();
        private static bool IsStaticCacheInitialized = false;
        private static ImmutableHashSet<string> CachedDeserializationMethodNames;

        public override void Initialize(AnalysisContext context)
        {
            if (!IsStaticCacheInitialized)
            {
                lock (StaticCacheInitializationLock)
                {
                    if (!IsStaticCacheInitialized)
                    {
                        CachedDeserializationMethodNames = this.DeserializationMethodNames;
                        IsStaticCacheInitialized = true;
                    }
                }
            }

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
                                    if (requiresBinderMustBeSetDataFlowAnalysis)
                                    {
                                        // Already know we need to perform DFA.
                                        return;
                                    }

                                    IInvocationOperation invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;
                                    if (invocationOperation.TargetMethod.ContainingType == deserializerTypeSymbol
                                        && CachedDeserializationMethodNames.Contains(invocationOperation.TargetMethod.Name))
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
                                            CachedDeserializationMethodNames);
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
