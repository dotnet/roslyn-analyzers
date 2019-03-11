// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    /// <summary>
    /// For detecting deserialization with <see cref="System.Web.Script.Serialization.JavaScriptSerializer"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class DoNotUseInsecureDeserializerJavaScriptSerializerWithSimpleTypeResolver : DiagnosticAnalyzer
    {
        // TODO paulming: Help links URLs.
        internal static readonly DiagnosticDescriptor DefinitelyWithSimpleTypeResolver =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2321",
                nameof(MicrosoftNetCoreSecurityResources.JavaScriptSerializerWithSimpleTypeResolverTitle),
                nameof(MicrosoftNetCoreSecurityResources.JavaScriptSerializerWithSimpleTypeResolverMessage),
                isEnabledByDefault: false,
                helpLinkUri: null);
        internal static readonly DiagnosticDescriptor MaybeWithSimpleTypeResolver =
            SecurityHelpers.CreateDiagnosticDescriptor(
                "CA2322",
                nameof(MicrosoftNetCoreSecurityResources.JavaScriptSerializerWithSimpleTypeResolverTitle),
                nameof(MicrosoftNetCoreSecurityResources.JavaScriptSerializerWithSimpleTypeResolverMessage),
                isEnabledByDefault: false,
                helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                DefinitelyWithSimpleTypeResolver,
                MaybeWithSimpleTypeResolver);

        private static readonly PropertyMapperCollection PropertyMappers = new PropertyMapperCollection(
            new PropertyMapper(
                "...dummy name",    // There isn't *really* a property for what we're tracking; just the constructor argument.
                (PointsToAbstractValue v) => PropertySetAbstractValueKind.Unknown));

        private static HazardousUsageEvaluationResult HazardousUsageCallback(IMethodSymbol methodSymbol, PropertySetAbstractValue propertySetAbstractValue)
        {
            switch (propertySetAbstractValue[0])
            {
                case PropertySetAbstractValueKind.Flagged:
                    return HazardousUsageEvaluationResult.Flagged;
                case PropertySetAbstractValueKind.Unflagged:
                    return HazardousUsageEvaluationResult.Unflagged;
                default:
                    return HazardousUsageEvaluationResult.MaybeFlagged;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            HazardousUsageEvaluatorCollection hazardousUsageEvaluators = new HazardousUsageEvaluatorCollection(
                SecurityHelpers.JavaScriptSerializerDeserializationMethods.Select(
                    (string methodName) => new HazardousUsageEvaluator(methodName, HazardousUsageCallback)));

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    WellKnownTypeProvider wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);
                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemWebScriptSerializationJavaScriptSerializer, out INamedTypeSymbol javaScriptSerializerSymbol)
                        || !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemWebScriptSerializationJavaScriptTypeResolver, out INamedTypeSymbol javaScriptTypeResolverSymbol)
                        || !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemWebScriptSerializationSimpleTypeResolver, out INamedTypeSymbol simpleTypeResolverSymbol))
                    {
                        return;
                    }

                    // If JavaScriptSerializer is initialized with a SimpleTypeResolver, then that instance is flagged.
                    ConstructorMapper constructorMapper = new ConstructorMapper(
                        (IMethodSymbol constructorMethod, IReadOnlyList<PointsToAbstractValue> argumentPointsToAbstractValues) =>
                        {
                            PropertySetAbstractValueKind kind;
                            if (constructorMethod.Parameters.Length == 0)
                            {
                                kind = PropertySetAbstractValueKind.Unflagged;
                            }
                            else if (constructorMethod.Parameters.Length == 1
                                && javaScriptTypeResolverSymbol.Equals(constructorMethod.Parameters[0].Type))
                            {
                                PointsToAbstractValue pointsTo = argumentPointsToAbstractValues[0];
                                switch (pointsTo.Kind)
                                {
                                    case PointsToAbstractValueKind.Invalid:
                                        kind = PropertySetAbstractValueKind.Unflagged;
                                        break;

                                    case PointsToAbstractValueKind.KnownLocations:
                                        if (pointsTo.Locations.Any(l => !l.IsNull && simpleTypeResolverSymbol.Equals(l.LocationTypeOpt)))
                                        {
                                            kind = PropertySetAbstractValueKind.Flagged;
                                        }
                                        else if (pointsTo.Locations.Any(l =>
                                                    !l.IsNull
                                                    && javaScriptTypeResolverSymbol.Equals(l.LocationTypeOpt)
                                                    && (l.CreationOpt == null || l.CreationOpt.Kind != OperationKind.ObjectCreation)))
                                        {
                                            // Points to a JavaScriptTypeResolver, but we don't know if the instance is a SimpleTypeResolver.
                                            kind = PropertySetAbstractValueKind.MaybeFlagged;
                                        }
                                        else
                                        {
                                            kind = PropertySetAbstractValueKind.Unflagged;
                                        }

                                        break;

                                    default:
                                        Debug.Fail($"Unhandled PointsToAbstractValueKind {pointsTo.Kind}");
                                        kind = PropertySetAbstractValueKind.Unflagged;
                                        break;
                                }
                            }
                            else
                            {
                                Debug.Fail($"Unhandled JavaScriptSerializer constructor {constructorMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}");
                                kind = PropertySetAbstractValueKind.Unflagged;
                            }

                            return PropertySetAbstractValue.GetInstance(kind);
                        });

                    PooledHashSet<(IOperation Operation, ISymbol ContainingSymbol)> rootOperationsNeedingAnalysis = PooledHashSet<(IOperation, ISymbol)>.GetInstance();

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    IInvocationOperation invocationOperation =
                                        (IInvocationOperation)operationAnalysisContext.Operation;
                                    if ((javaScriptSerializerSymbol.Equals(invocationOperation.Instance?.Type)
                                            && SecurityHelpers.JavaScriptSerializerDeserializationMethods.Contains(invocationOperation.TargetMethod.Name))
                                        || simpleTypeResolverSymbol.Equals(invocationOperation.TargetMethod.ReturnType)
                                        || javaScriptTypeResolverSymbol.Equals(invocationOperation.TargetMethod.ReturnType))
                                    {
                                        lock (rootOperationsNeedingAnalysis)
                                        {
                                            rootOperationsNeedingAnalysis.Add((invocationOperation.GetRoot(), operationAnalysisContext.ContainingSymbol));
                                        }
                                    }
                                },
                                OperationKind.Invocation);

                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    IMethodReferenceOperation methodReferenceOperation =
                                        (IMethodReferenceOperation)operationAnalysisContext.Operation;
                                    if (javaScriptSerializerSymbol.Equals(methodReferenceOperation.Instance?.Type)
                                        && SecurityHelpers.JavaScriptSerializerDeserializationMethods.Contains(methodReferenceOperation.Method.Name))
                                    {
                                        lock (rootOperationsNeedingAnalysis)
                                        {
                                            rootOperationsNeedingAnalysis.Add((operationAnalysisContext.Operation.GetRoot(), operationAnalysisContext.ContainingSymbol));
                                        }
                                    }
                                },
                                OperationKind.MethodReference);
                        });

                    compilationStartAnalysisContext.RegisterCompilationEndAction(
                        (CompilationAnalysisContext compilationAnalysisContext) =>
                        {
                            PooledDictionary<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> allResults = null;
                            try
                            {
                                lock (rootOperationsNeedingAnalysis)
                                {
                                    if (!rootOperationsNeedingAnalysis.Any())
                                    {
                                        return;
                                    }

                                    InterproceduralAnalysisConfiguration interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
                                        compilationAnalysisContext.Options, SupportedDiagnostics,
                                        defaultInterproceduralAnalysisKind: InterproceduralAnalysisKind.ContextSensitive,
                                        cancellationToken: compilationAnalysisContext.CancellationToken);

                                    foreach ((IOperation Operation, ISymbol ContainingSymbol) pair in rootOperationsNeedingAnalysis)
                                    {
                                        ImmutableDictionary<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> dfaResult =
                                            PropertySetAnalysis.GetOrComputeHazardousUsages(
                                                pair.Operation.GetEnclosingControlFlowGraph(),
                                                compilationAnalysisContext.Compilation,
                                                pair.ContainingSymbol,
                                                WellKnownTypeNames.SystemWebScriptSerializationJavaScriptSerializer,
                                                constructorMapper,
                                                PropertyMappers,
                                                hazardousUsageEvaluators,
                                                interproceduralAnalysisConfig);
                                        if (dfaResult.IsEmpty)
                                        {
                                            continue;
                                        }

                                        if (allResults == null)
                                        {
                                            allResults = PooledDictionary<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult>.GetInstance();
                                        }

                                        foreach (KeyValuePair<(Location Location, IMethodSymbol Method), HazardousUsageEvaluationResult> kvp
                                            in dfaResult)
                                        {
                                            if (allResults.TryGetValue(kvp.Key, out HazardousUsageEvaluationResult existingValue))
                                            {
                                                allResults[kvp.Key] = PropertySetAnalysis.MergeHazardousUsageEvaluationResult(existingValue, kvp.Value);
                                            }
                                            else
                                            {
                                                allResults.Add(kvp.Key, kvp.Value);
                                            }
                                        }
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
                                            descriptor = DefinitelyWithSimpleTypeResolver;
                                            break;

                                        case HazardousUsageEvaluationResult.MaybeFlagged:
                                            descriptor = MaybeWithSimpleTypeResolver;
                                            break;

                                        default:
                                            Debug.Fail($"Unhandled result value {kvp.Value}");
                                            continue;
                                    }

                                    compilationAnalysisContext.ReportDiagnostic(
                                        Diagnostic.Create(
                                            descriptor,
                                            kvp.Key.Location,
                                            kvp.Key.Method.ToDisplayString(
                                                SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                }
                            }
                            finally
                            {
                                rootOperationsNeedingAnalysis.Free();
                                allResults?.Free();
                            }
                        });

                });
        }
    }
}
