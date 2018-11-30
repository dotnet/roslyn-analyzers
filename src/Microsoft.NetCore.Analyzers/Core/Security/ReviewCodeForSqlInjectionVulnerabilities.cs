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
    using System.Collections.Generic;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForSqlInjectionVulnerabilities : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor Rule = SecurityHelpers.CreateDiagnosticDescriptor(
            "CA3001",
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesTitle),
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesMessage),
            isEnabledByDefault: false,
            helpLinkUri: null); // TODO paulming: Help link.  https://github.com/dotnet/roslyn-analyzers/issues/1892

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                compilationContext =>
                {
                    TaintedDataConfig taintedDataConfig = TaintedDataConfig.GetOrCreate(compilationContext.Compilation);
                    TaintedDataSymbolMap<SourceInfo> sourceInfoSymbolMap = taintedDataConfig.GetSourceSymbolMap(SinkKind.Sql);
                    if (sourceInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    TaintedDataSymbolMap<SinkInfo> sinkInfoSymbolMap = taintedDataConfig.GetSinkSymbolMap(SinkKind.Sql);
                    if (sinkInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    compilationContext.RegisterOperationBlockStartAction(
                        operationBlockStartContext =>
                        {
                            ISymbol owningSymbol = operationBlockStartContext.OwningSymbol;

                            HashSet<IOperation> rootOperationsNeedingAnalysis = new HashSet<IOperation>();

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    IPropertyReferenceOperation propertyReferenceOperation = (IPropertyReferenceOperation)operationAnalysisContext.Operation;
                                    if (sourceInfoSymbolMap.IsSourceProperty(propertyReferenceOperation.Property))
                                    {
                                        rootOperationsNeedingAnalysis.Add(operationAnalysisContext.Operation.GetRoot());
                                    }
                                },
                                OperationKind.PropertyReference);

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    IInvocationOperation invocationOperation = (IInvocationOperation) operationAnalysisContext.Operation;
                                    if (sourceInfoSymbolMap.IsSourceMethod(invocationOperation.TargetMethod))
                                    {
                                        rootOperationsNeedingAnalysis.Add(operationAnalysisContext.Operation.GetRoot());
                                    }
                                },
                                OperationKind.Invocation);

                            operationBlockStartContext.RegisterOperationBlockEndAction(
                                operationBlockAnalysisContext =>
                                {
                                    if (!rootOperationsNeedingAnalysis.Any())
                                    {
                                        return;
                                    }

                                    foreach (IOperation rootOperation in rootOperationsNeedingAnalysis)
                                    {
                                        TaintedDataAnalysisResult taintedDataAnalysisResult = TaintedDataAnalysis.GetOrComputeResult(
                                            rootOperation.GetEnclosingControlFlowGraph(),
                                            operationBlockAnalysisContext.Compilation,
                                            operationBlockAnalysisContext.OwningSymbol,
                                            sourceInfoSymbolMap,
                                            taintedDataConfig.GetSanitizerSymbolMap(SinkKind.Sql),
                                            sinkInfoSymbolMap);
                                        foreach (TaintedDataSourceSink sourceSink in taintedDataAnalysisResult.TaintedDataSourceSinks)
                                        {
                                            if (sourceSink.SinkKind != SinkKind.Sql)
                                            {
                                                continue;
                                            }

                                            foreach (SymbolAccess sourceOrigin in sourceSink.SourceOrigins)
                                            {
                                                // CA3001: Potential SQL injection vulnerability was found where '{0}' in method '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
                                                Diagnostic diagnostic = Diagnostic.Create(
                                                    Rule,
                                                    sourceSink.Sink.Location,
                                                    additionalLocations: new Location[] { sourceOrigin.Location },
                                                    messageArgs: new object[] {
                                                    sourceSink.Sink.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                    sourceSink.Sink.AccessingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                    sourceOrigin.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                    sourceOrigin.AccessingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)});
                                                operationBlockAnalysisContext.ReportDiagnostic(diagnostic);
                                            }
                                        }
                                    }
                                });
                        });
                });
        }
    }
}