// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Security.Helpers;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    class DoNotDisableHttpClientCRLCheck : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor DefinitelyDisableHttpClientCRLCheckRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5399",
            nameof(MicrosoftNetCoreAnalyzersResources.DefinitelyDisableHttpClientCRLCheck),
            nameof(MicrosoftNetCoreAnalyzersResources.DefinitelyDisableHttpClientCRLCheckMessage),
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null,
            descriptionResourceStringName: nameof(MicrosoftNetCoreAnalyzersResources.DoNotDisableHttpClientCRLCheckDescription),
            customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);
        internal static DiagnosticDescriptor MaybeDisableHttpClientCRLCheckRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5400",
            nameof(MicrosoftNetCoreAnalyzersResources.MaybeDisableHttpClientCRLCheck),
            nameof(MicrosoftNetCoreAnalyzersResources.MaybeDisableHttpClientCRLCheckMessage),
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null,
            descriptionResourceStringName: nameof(MicrosoftNetCoreAnalyzersResources.DoNotDisableHttpClientCRLCheckDescription),
            customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                DefinitelyDisableHttpClientCRLCheckRule,
                MaybeDisableHttpClientCRLCheckRule);

        /// <summary>
        /// PropertySetAbstractValue index for CheckCertificateRevocationList property.
        /// </summary>
        private const int CheckCertificateRevocationListIndex = 0;

        private static readonly ConstructorMapper ConstructorMapper = new ConstructorMapper(
            (IMethodSymbol constructorMethod, IReadOnlyList<PointsToAbstractValue> argumentPointsToAbstractValues) =>
            {
                return PropertySetAbstractValue.GetInstance(PropertySetAbstractValueKind.Flagged);
            });

        private static readonly PropertyMapperCollection PropertyMappers = new PropertyMapperCollection(
            new PropertyMapper(
                "CheckCertificateRevocationList",
                (ValueContentAbstractValue valueContentAbstractValue) =>
                    PropertySetCallbacks.EvaluateLiteralValues(
                        valueContentAbstractValue,
                        (object o) => o is bool booleanValue && booleanValue == false),
                CheckCertificateRevocationListIndex));

        private static readonly HazardousUsageEvaluatorCollection HazardousUsageEvaluators = new HazardousUsageEvaluatorCollection(
            new HazardousUsageEvaluator(
                WellKnownTypeNames.SystemNetHttpHttpClient,
                ".ctor",
                "handler",
                PropertySetCallbacks.HazardousIfAllFlaggedAndAtLeastOneKnown,
                true));

        private static readonly ImmutableHashSet<string> typeToTrackMetadataNames = ImmutableHashSet.Create<string>(
            WellKnownTypeNames.SystemNetHttpWinHttpHandler,
            WellKnownTypeNames.SystemNetHttpHttpClientHandler,
            WellKnownTypeNames.SystemNetHttpUnixCurlHandler);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemNetHttpHttpClient, out INamedTypeSymbol httpClientTypeSymbol))
                    {
                        return;
                    }

                    if (typeToTrackMetadataNames.All(s => !wellKnownTypeProvider.TryGetTypeByMetadataName(s, out _)))
                    {
                        return;
                    }

                    var rootOperationsNeedingAnalysis = PooledHashSet<(IOperation, ISymbol)>.GetInstance();

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            ISymbol owningSymbol = operationBlockStartAnalysisContext.OwningSymbol;

                            if (owningSymbol.IsConfiguredToSkipAnalysis(
                                    operationBlockStartAnalysisContext.Options,
                                    DefinitelyDisableHttpClientCRLCheckRule,
                                    operationBlockStartAnalysisContext.Compilation,
                                    operationBlockStartAnalysisContext.CancellationToken) &&
                                owningSymbol.IsConfiguredToSkipAnalysis(
                                    operationBlockStartAnalysisContext.Options,
                                    MaybeDisableHttpClientCRLCheckRule,
                                    operationBlockStartAnalysisContext.Compilation,
                                    operationBlockStartAnalysisContext.CancellationToken))
                            {
                                return;
                            }

                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    var objectCreationOperation =
                                        (IObjectCreationOperation)operationAnalysisContext.Operation;

                                    if (objectCreationOperation.Type.GetBaseTypesAndThis().Contains(httpClientTypeSymbol))
                                    {
                                        if (objectCreationOperation.Arguments.Length != 0)
                                        {
                                            lock (rootOperationsNeedingAnalysis)
                                            {
                                                rootOperationsNeedingAnalysis.Add(
                                                    (objectCreationOperation.GetRoot(), operationAnalysisContext.ContainingSymbol));
                                            }
                                        }
                                    }
                                },
                                OperationKind.ObjectCreation);
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

                                    allResults = PropertySetAnalysis.BatchGetOrComputeHazardousUsages(
                                        compilationAnalysisContext.Compilation,
                                        rootOperationsNeedingAnalysis,
                                        compilationAnalysisContext.Options,
                                        typeToTrackMetadataNames,
                                        ConstructorMapper,
                                        PropertyMappers,
                                        HazardousUsageEvaluators,
                                        InterproceduralAnalysisConfiguration.Create(
                                            compilationAnalysisContext.Options,
                                            SupportedDiagnostics,
                                            defaultInterproceduralAnalysisKind: InterproceduralAnalysisKind.ContextSensitive,
                                            cancellationToken: compilationAnalysisContext.CancellationToken));
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
                                            descriptor = DefinitelyDisableHttpClientCRLCheckRule;
                                            break;

                                        case HazardousUsageEvaluationResult.MaybeFlagged:
                                            descriptor = MaybeDisableHttpClientCRLCheckRule;
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
