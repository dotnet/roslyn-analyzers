﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
    public sealed class DoNotInstallRootCert : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor DefinitelyInstallRootCertRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5380",
            typeof(SystemSecurityCryptographyResources),
            nameof(SystemSecurityCryptographyResources.DefinitelyInstallRootCert),
            nameof(SystemSecurityCryptographyResources.DefinitelyInstallRootCertMessage),
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null,
            descriptionResourceStringName: nameof(SystemSecurityCryptographyResources.DoNotInstallRootCertDescription),
            customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);
        internal static DiagnosticDescriptor MaybeInstallRootCertRule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA5381",
            typeof(SystemSecurityCryptographyResources),
            nameof(SystemSecurityCryptographyResources.MaybeInstallRootCert),
            nameof(SystemSecurityCryptographyResources.MaybeInstallRootCertMessage),
            DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            helpLinkUri: null,
            descriptionResourceStringName: nameof(SystemSecurityCryptographyResources.DoNotInstallRootCertDescription),
            customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
                                                                                        DefinitelyInstallRootCertRule,
                                                                                        MaybeInstallRootCertRule);

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

                case PropertySetAbstractValueKind.MaybeFlagged:
                    return HazardousUsageEvaluationResult.MaybeFlagged;

                default:
                    return HazardousUsageEvaluationResult.Unflagged;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            HazardousUsageEvaluatorCollection hazardousUsageEvaluators = new HazardousUsageEvaluatorCollection(
                new HazardousUsageEvaluator("Add", HazardousUsageCallback));

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyX509CertificatesX509Store, out var x509TypeSymbol))
                    {
                        return;
                    }

                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemSecurityCryptographyX509CertificatesStoreName, out var storeNameTypeSymbol))
                    {
                        return;
                    }

                    // If X509Store is initialized with Root store, then that instance is flagged.
                    var constructorMapper = new ConstructorMapper(
                        (IMethodSymbol constructorMethod, IReadOnlyList<ValueContentAbstractValue> argumentValueContentAbstractValues,
                        IReadOnlyList<PointsToAbstractValue> argumentPointsToAbstractValues) =>
                        {
                            var kind = PropertySetAbstractValueKind.Unflagged;

                            if (constructorMethod.Parameters.Length > 0)
                            {
                                if (constructorMethod.Parameters[0].Type.Equals(storeNameTypeSymbol))
                                {
                                    kind = PropertySetAnalysis.EvaluateLiteralValues(argumentValueContentAbstractValues[0], o => o.Equals(6));
                                }
                                else if (constructorMethod.Parameters[0].Type.SpecialType == SpecialType.System_String)
                                {
                                    kind = PropertySetAnalysis.EvaluateLiteralValues(
                                        argumentValueContentAbstractValues[0],
                                        s => string.Equals(s.ToString(), "root", StringComparison.OrdinalIgnoreCase));
                                }
                            }

                            return PropertySetAbstractValue.GetInstance(kind);
                        });

                    var rootOperationsNeedingAnalysis = PooledHashSet<(IOperation, ISymbol)>.GetInstance();

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;

                                    if (x509TypeSymbol.Equals(invocationOperation.Instance?.Type) &&
                                        invocationOperation.TargetMethod.Name == "Add")
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
                                    var argumentOperation = (IArgumentOperation)operationAnalysisContext.Operation;

                                    if (x509TypeSymbol.Equals(argumentOperation.Parameter.Type))
                                    {
                                        lock (rootOperationsNeedingAnalysis)
                                        {
                                            rootOperationsNeedingAnalysis.Add((argumentOperation.GetRoot(), operationAnalysisContext.ContainingSymbol));
                                        }
                                    }
                                },
                                OperationKind.Argument);
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
                                        WellKnownTypeNames.SystemSecurityCryptographyX509CertificatesX509Store,
                                        constructorMapper,
                                        PropertyMappers,
                                        hazardousUsageEvaluators,
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
                                            descriptor = DefinitelyInstallRootCertRule;
                                            break;

                                        case HazardousUsageEvaluationResult.MaybeFlagged:
                                            descriptor = MaybeInstallRootCertRule;
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
