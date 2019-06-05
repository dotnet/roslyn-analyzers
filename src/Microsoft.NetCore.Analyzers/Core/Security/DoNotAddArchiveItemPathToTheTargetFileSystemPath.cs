// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotAddArchiveItemPathToTheTargetFileSystemPath : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5389";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotAddArchiveItemPathToTheTargetFileSystemPath),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotAddArchiveItemPathToTheTargetFileSystemPathMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.DoNotAddArchiveItemPathToTheTargetFileSystemPathDescription),
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

        /// <summary>
        /// Kind of tainted data sink.
        /// </summary>
        private static SinkKind SinkKind { get { return SinkKind.ZipSlip; } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationStartAnalysisContext.Compilation);
                    var taintedDataConfig = TaintedDataConfig.GetOrCreate(compilationStartAnalysisContext.Compilation);
                    var sourceInfoSymbolMap = taintedDataConfig.GetSourceSymbolMap(SinkKind);

                    if (sourceInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    var sinkInfoSymbolMap = taintedDataConfig.GetSinkSymbolMap(SinkKind);

                    if (sinkInfoSymbolMap.IsEmpty)
                    {
                        return;
                    }

                    compilationStartAnalysisContext.RegisterOperationBlockStartAction(
                        operationBlockStartContext =>
                        {
                            var owningSymbol = operationBlockStartContext.OwningSymbol;
                            var rootOperationsNeedingAnalysis = new HashSet<IOperation>();

                            operationBlockStartContext.RegisterOperationAction(
                                operationAnalysisContext =>
                                {
                                    var propertyReferenceOperation = (IPropertyReferenceOperation)operationAnalysisContext.Operation;

                                    if (sourceInfoSymbolMap.IsSourceProperty(propertyReferenceOperation.Property))
                                    {
                                        rootOperationsNeedingAnalysis.Add(operationAnalysisContext.Operation.GetRoot());
                                    }
                                },
                                OperationKind.PropertyReference);

                            operationBlockStartContext.RegisterOperationBlockEndAction(
                                operationBlockAnalysisContext =>
                                {
                                    if (!rootOperationsNeedingAnalysis.Any())
                                    {
                                        return;
                                    }

                                    foreach (IOperation rootOperation in rootOperationsNeedingAnalysis)
                                    {
                                        var taintedDataAnalysisResult = TaintedDataAnalysis.GetOrComputeResult(
                                            rootOperation.GetEnclosingControlFlowGraph(),
                                            operationBlockAnalysisContext.Compilation,
                                            operationBlockAnalysisContext.OwningSymbol,
                                            operationBlockAnalysisContext.Options,
                                            Rule,
                                            sourceInfoSymbolMap,
                                            taintedDataConfig.GetSanitizerSymbolMap(SinkKind),
                                            sinkInfoSymbolMap,
                                            operationBlockAnalysisContext.CancellationToken);

                                        foreach (TaintedDataSourceSink sourceSink in taintedDataAnalysisResult.TaintedDataSourceSinks)
                                        {
                                            if (!sourceSink.SinkKinds.Contains(SinkKind))
                                            {
                                                continue;
                                            }

                                            foreach (SymbolAccess sourceOrigin in sourceSink.SourceOrigins)
                                            {
                                                Diagnostic diagnostic = Diagnostic.Create(
                                                    Rule,
                                                    sourceOrigin.Location,
                                                    additionalLocations: new Location[] { sourceSink.Sink.Location },
                                                    messageArgs: new object[] {
                                                        sourceOrigin.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                        "",
                                                        sourceSink.Sink.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                                                        ""});
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
