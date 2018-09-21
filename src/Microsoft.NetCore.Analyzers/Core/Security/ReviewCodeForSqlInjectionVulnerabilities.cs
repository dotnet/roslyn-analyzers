// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Analyzer.Utilities;
    using Analyzer.Utilities.Extensions;
    using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
    using Microsoft.CodeAnalysis.Operations;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForSqlInjectionVulnerabilities : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3001";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesTitle),
            MicrosoftNetCoreSecurityResources.ResourceManager,
            typeof(MicrosoftNetCoreSecurityResources));

        private static readonly LocalizableString Message = new LocalizableResourceString(
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesMessage),
            MicrosoftNetCoreSecurityResources.ResourceManager,
            typeof(MicrosoftNetCoreSecurityResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            Title,
            Message,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(
                compilationContext =>
                {
                    WellKnownTypeProvider wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationContext.Compilation);
                    if (!WebInputSources.DoesCompilationIncludeSources(wellKnownTypeProvider)
                        || !SqlSinks.DoesCompilationIncludeSinks(wellKnownTypeProvider))
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
                                    if (WebInputSources.IsTaintedProperty(wellKnownTypeProvider, propertyReferenceOperation))
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
                                    if (WebInputSources.IsTaintedMethod(wellKnownTypeProvider, invocationOperation.Instance, invocationOperation.TargetMethod))
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
                                        operationBlockAnalysisContext.OwningSymbol);
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
                                                sourceSink.Sink.SyntaxNode.GetLocation(),
                                                new Location[] { sourceOrigin.SyntaxNode.GetLocation() },
                                                sourceSink.Sink.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                sourceSink.Sink.AccessingMethod.Name,
                                                sourceOrigin.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                sourceOrigin.AccessingMethod.Name);
                                            operationBlockAnalysisContext.ReportDiagnostic(diagnostic);
                                        }
                                    }
                                });
                        });
                });
        }
    }
}