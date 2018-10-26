// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.Security
{
    using System.Collections.Immutable;
    using Analyzer.Utilities.Extensions;
    using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.Operations;
    using Microsoft.NetCore.Analyzers.Security.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForSqlInjectionVulnerabilities : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor Rule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA3001",
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesTitle),
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesMessage),
            isEnabledByDefault: false,
            helpLinkUri: null); // TODO paulming: Help link.

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                compilationContext =>
                {
                    WellKnownTypeProvider wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationContext.Compilation);
                    if (!WebInputSources.DoesCompilationIncludeSources(wellKnownTypeProvider)
                        || !SqlSinks.DoesCompilationIncludeSinks(wellKnownTypeProvider))
                    {
                        return;
                    }

                    ImmutableDictionary<ITypeSymbol, SourceInfo> sourcesBySymbol = WebInputSources.BuildBySymbolMap(wellKnownTypeProvider);

                    compilationContext.RegisterOperationBlockStartAction(
                        operationBlockStartContext =>
                        {
                            ISymbol owningSymbol = operationBlockStartContext.OwningSymbol;

                            bool requiresTaintedDataAnalysis = false;

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    if (requiresTaintedDataAnalysis)
                                    {
                                        return;
                                    }

                                    IPropertyReferenceOperation propertyReferenceOperation = (IPropertyReferenceOperation)operationAnalysisContext.Operation;
                                    if (WebInputSources.IsTaintedProperty(sourcesBySymbol, propertyReferenceOperation))
                                    {
                                        requiresTaintedDataAnalysis = true;
                                    }
                                },
                                OperationKind.PropertyReference);

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    if (requiresTaintedDataAnalysis)
                                    {
                                        return;
                                    }

                                    IInvocationOperation invocationOperation = (IInvocationOperation) operationAnalysisContext.Operation;
                                    if (WebInputSources.IsTaintedMethod(sourcesBySymbol, invocationOperation.Instance, invocationOperation.TargetMethod))
                                    {
                                        requiresTaintedDataAnalysis = true;
                                    }
                                },
                                OperationKind.Invocation);

                            operationBlockStartContext.RegisterOperationBlockEndAction(
                                operationBlockAnalysisContext =>
                                {
                                    if (!requiresTaintedDataAnalysis)
                                    {
                                        return;
                                    }

                                    TaintedDataAnalysisResult taintedDataAnalysisResult = TaintedDataAnalysis.GetOrComputeResult(
                                        operationBlockAnalysisContext.OperationBlocks[0].GetEnclosingControlFlowGraph(),
                                        operationBlockAnalysisContext.Compilation,
                                        operationBlockAnalysisContext.OwningSymbol,
                                        sourcesBySymbol,
                                        PrimitiveTypeConverterSanitizers.BuildConcreteSanitizersBySymbolMap(wellKnownTypeProvider),
                                        SqlSinks.BuildConcreteSinksBySymbolMap(wellKnownTypeProvider),
                                        SqlSinks.BuildInterfaceSinksBySymbolMap(wellKnownTypeProvider));
                                    foreach (TaintedDataSourceSink sourceSink in taintedDataAnalysisResult.TaintedDataSourceSinks)
                                    {
                                        if (sourceSink.SinkKind != SinkKind.Sql)
                                        {
                                            continue;
                                        }

                                        foreach (SymbolAccess sourceOrigin in sourceSink.SourceOrigins)
                                        {
                                            Diagnostic diagnostic = Diagnostic.Create(
                                                Rule,
                                                sourceSink.Sink.Location,
                                                new Location[] { sourceOrigin.Location },
                                                sourceSink.Sink.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                sourceSink.Sink.AccessingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                sourceOrigin.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                sourceOrigin.AccessingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                                            operationBlockAnalysisContext.ReportDiagnostic(diagnostic);
                                        }
                                    }
                                });
                        });
                });
        }
    }
}