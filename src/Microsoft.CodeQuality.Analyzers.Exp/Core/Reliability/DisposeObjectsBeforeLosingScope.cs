// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.DisposeAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp.Reliability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DisposeObjectsBeforeLosingScope : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2000";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftReliabilityAnalyzersResources.DisposeObjectsBeforeLosingScopeTitle), MicrosoftReliabilityAnalyzersResources.ResourceManager, typeof(MicrosoftReliabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftReliabilityAnalyzersResources.DisposeObjectsBeforeLosingScopeMessage), MicrosoftReliabilityAnalyzersResources.ResourceManager, typeof(MicrosoftReliabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftReliabilityAnalyzersResources.DisposeObjectsBeforeLosingScopeDescription), MicrosoftReliabilityAnalyzersResources.ResourceManager, typeof(MicrosoftReliabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(compilationContext =>
            {
                if (!DisposeAnalysisHelper.TryGetOrCreate(compilationContext.Compilation, out DisposeAnalysisHelper disposeAnalysisHelper))
                {
                    return;
                }

                compilationContext.RegisterOperationBlockAction(operationBlockContext =>
                {
                    if (!(operationBlockContext.OwningSymbol is IMethodSymbol containingMethod) ||
                        !disposeAnalysisHelper.HasAnyDisposableCreationDescendant(operationBlockContext.OperationBlocks, containingMethod))
                    {
                        return;
                    }

                    DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> disposeAnalysisResult;
                    if (disposeAnalysisHelper.TryGetOrComputeResult(operationBlockContext.OperationBlocks, containingMethod, out disposeAnalysisResult))
                    {
                        BasicBlock exitBlock = disposeAnalysisResult.ControlFlowGraph.GetExit();
                        ImmutableDictionary<AbstractLocation, DisposeAbstractValue> disposeDataAtExit = disposeAnalysisResult[exitBlock].OutputData;
                        foreach (var kvp in disposeDataAtExit)
                        {
                            AbstractLocation location = kvp.Key;
                            DisposeAbstractValue disposeValue = kvp.Value;
                            if (disposeValue.Kind == DisposeAbstractValueKind.NotDisposable ||
                                location.CreationOpt == null)
                            {
                                continue;
                            }

                            if (disposeValue.Kind == DisposeAbstractValueKind.NotDisposed ||
                                (disposeValue.DisposingOrEscapingOperations.Count > 0 &&
                                 disposeValue.DisposingOrEscapingOperations.All(d => d.IsInsideCatchRegion(disposeAnalysisResult.ControlFlowGraph))))
                            {
                                // CA2000: In method '{0}', call System.IDisposable.Dispose on object created by '{1}' before all references to it are out of scope.
                                var arg1 = containingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                                var arg2 = location.CreationOpt.Syntax.ToString();
                                var diagnostic = location.CreationOpt.Syntax.CreateDiagnostic(Rule, arg1, arg2);
                                operationBlockContext.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                });
            });
        }
    }
}
