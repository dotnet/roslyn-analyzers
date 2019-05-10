// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseSecureCookiesASPNetCore : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5381";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseSecureCookiesASPNetCore),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseSecureCookiesASPNetCoreMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseSecureCookiesASPNetCoreDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                s_Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTagsExtensions.DataflowAndTelemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly PropertyMapperCollection PropertyMappers = new PropertyMapperCollection(
            new PropertyMapper(
                "Secure",
                (ValueContentAbstractValue valueContentAbstractValue) =>
                {
                    switch (valueContentAbstractValue.NonLiteralState)
                    {
                        case ValueContainsNonLiteralState.No:
                            // We know all values, so we can say Flagged or Unflagged.
                            return valueContentAbstractValue.LiteralValues.Contains(true)
                                ? PropertySetAbstractValueKind.Unflagged
                                : PropertySetAbstractValueKind.Flagged;
                        case ValueContainsNonLiteralState.Maybe:
                            // We don't know all values, so we can say Flagged, or who knows.
                            return valueContentAbstractValue.LiteralValues.Contains(true)
                                ? PropertySetAbstractValueKind.Unflagged
                                : PropertySetAbstractValueKind.Unknown;
                        default:
                            return PropertySetAbstractValueKind.Unknown;
                    }
                }));

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

            // If there are more classes implement IResponseCookies, add them here later.
            HazardousUsageEvaluatorCollection hazardousUsageEvaluators = new HazardousUsageEvaluatorCollection(
                new HazardousUsageEvaluator(
                    WellKnownTypeNames.MicrosoftAspNetCoreHttpResponseCookies,
                    "Append",
                    "options",
                    HazardousUsageCallback));

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);

                    if (!wellKnownTypeProvider.TryGetTypeByMetadataName(
                            WellKnownTypeNames.MicrosoftAspNetCoreHttpIResponseCookies,
                            out var iResponseCookiesTypeSymbol))
                    {
                        return;
                    }

                    wellKnownTypeProvider.TryGetTypeByMetadataName(
                        WellKnownTypeNames.MicrosoftAspNetCoreHttpCookieOptions,
                        out var cookieOptionsTypeSymbol);

                    var constructorMapper = new ConstructorMapper(
                        ImmutableArray.Create<PropertySetAbstractValueKind>(
                            PropertySetAbstractValueKind.Flagged));

                    var rootOperationsNeedingAnalysis = PooledHashSet<(IOperation, ISymbol)>.GetInstance();

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        (OperationBlockStartAnalysisContext operationBlockStartAnalysisContext) =>
                        {
                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;
                                    var methodSymbol = invocationOperation.TargetMethod;

                                    if (methodSymbol.ContainingType is INamedTypeSymbol namedTypeSymbol &&
                                        namedTypeSymbol.Interfaces.Contains(iResponseCookiesTypeSymbol) &&
                                        invocationOperation.TargetMethod.Name == "Append")
                                    {
                                        if (methodSymbol.Parameters.Length < 3)
                                        {
                                            operationAnalysisContext.ReportDiagnostic(
                                                invocationOperation.CreateDiagnostic(
                                                    Rule));
                                        }
                                        else
                                        {
                                            lock (rootOperationsNeedingAnalysis)
                                            {
                                                rootOperationsNeedingAnalysis.Add((invocationOperation.GetRoot(), operationAnalysisContext.ContainingSymbol));
                                            }
                                        }
                                    }
                                },
                                OperationKind.Invocation);

                            operationBlockStartAnalysisContext.RegisterOperationAction(
                                (OperationAnalysisContext operationAnalysisContext) =>
                                {
                                    var argumentOperation = (IArgumentOperation)operationAnalysisContext.Operation;

                                    if (argumentOperation.Parameter.Type.Equals(cookieOptionsTypeSymbol))
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
                                        WellKnownTypeNames.MicrosoftAspNetCoreHttpCookieOptions,
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

                                foreach (var kvp in allResults)
                                {
                                    if (kvp.Value.Equals(HazardousUsageEvaluationResult.Flagged))
                                    {
                                        compilationAnalysisContext.ReportDiagnostic(
                                            Diagnostic.Create(
                                                Rule,
                                                kvp.Key.Location,
                                                kvp.Key.Method.ToDisplayString(
                                                    SymbolDisplayFormat.MinimallyQualifiedFormat)));
                                    }
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
