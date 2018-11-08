// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.Security
{
    using System.Linq;
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
                    TaintedDataSymbolMap<SourceInfo> sourceInfoSymbolMap = new TaintedDataSymbolMap<SourceInfo>(
                        wellKnownTypeProvider, 
                        WebInputSources.SourceInfos);
                    if (sourceInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    TaintedDataSymbolMap<SinkInfo> sinkInfoSymbolMap = new TaintedDataSymbolMap<SinkInfo>(
                        wellKnownTypeProvider,
                        SqlSinks.SinkInfos);
                    if (sinkInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

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
                                    if (sourceInfoSymbolMap.IsSourceProperty(propertyReferenceOperation.Property))
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
                                    if (sourceInfoSymbolMap.IsSourceMethod(invocationOperation.TargetMethod))
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
                                        sourceInfoSymbolMap,
                                        new TaintedDataSymbolMap<SanitizerInfo>(
                                            wellKnownTypeProvider,
                                            PrimitiveTypeConverterSanitizers.SanitizerInfos),
                                        sinkInfoSymbolMap);
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