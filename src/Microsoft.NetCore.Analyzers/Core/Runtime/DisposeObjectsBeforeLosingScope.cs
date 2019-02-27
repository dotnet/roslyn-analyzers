// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DisposeObjectsBeforeLosingScope : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2000";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposeObjectsBeforeLosingScopeTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableNotDisposedMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposeObjectsBeforeLosingScopeNotDisposedMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMayBeDisposedMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposeObjectsBeforeLosingScopeMayBeDisposedMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposeObjectsBeforeLosingScopeDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor NotDisposedRule = new DiagnosticDescriptor(RuleId,
                                                                                        s_localizableTitle,
                                                                                        s_localizableNotDisposedMessage,
                                                                                        DiagnosticCategory.Reliability,
                                                                                        DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                        isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                                        description: s_localizableDescription,
                                                                                        helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope",
                                                                                        customTags: FxCopWellKnownDiagnosticTags.PortedFxCopDataflowRule);

        internal static DiagnosticDescriptor MayBeDisposedRule = new DiagnosticDescriptor(RuleId,
                                                                                          s_localizableTitle,
                                                                                          s_localizableMayBeDisposedMessage,
                                                                                          DiagnosticCategory.Reliability,
                                                                                          DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                          isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                                          description: s_localizableDescription,
                                                                                          helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope",
                                                                                          customTags: FxCopWellKnownDiagnosticTags.PortedFxCopDataflowRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NotDisposedRule, MayBeDisposedRule);

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

                var reportedNodes = new ConcurrentDictionary<SyntaxNode, bool>();
                compilationContext.RegisterOperationBlockAction(operationBlockContext =>
                {
                    if (!(operationBlockContext.OwningSymbol is IMethodSymbol containingMethod) ||
                        !disposeAnalysisHelper.HasAnyDisposableCreationDescendant(operationBlockContext.OperationBlocks, containingMethod))
                    {
                        return;
                    }

                    if (disposeAnalysisHelper.TryGetOrComputeResult(operationBlockContext.OperationBlocks, containingMethod,
                        operationBlockContext.Options, NotDisposedRule, operationBlockContext.CancellationToken,
                        out var disposeAnalysisResult, out var pointsToAnalysisResult))
                    {
                        BasicBlock exitBlock = disposeAnalysisResult.ControlFlowGraph.GetExit();
                        ImmutableDictionary<AbstractLocation, DisposeAbstractValue> disposeDataAtExit = disposeAnalysisResult.ExitBlockOutput.Data;
                        foreach (var kvp in disposeDataAtExit)
                        {
                            AbstractLocation location = kvp.Key;
                            DisposeAbstractValue disposeValue = kvp.Value;
                            if (disposeValue.Kind == DisposeAbstractValueKind.NotDisposable ||
                                location.CreationOpt == null)
                            {
                                continue;
                            }

                            var isNotDisposed = disposeValue.Kind == DisposeAbstractValueKind.NotDisposed ||
                                (disposeValue.DisposingOrEscapingOperations.Count > 0 &&
                                 disposeValue.DisposingOrEscapingOperations.All(d => d.IsInsideCatchRegion(disposeAnalysisResult.ControlFlowGraph)));
                            var isMayBeDisposed = !isNotDisposed && (disposeValue.Kind == DisposeAbstractValueKind.MaybeDisposed || disposeValue.Kind == DisposeAbstractValueKind.NotDisposedOrEscaped);

                            if (isNotDisposed || isMayBeDisposed)
                            {
                                var syntax = location.GetNodeToReportDiagnostic(pointsToAnalysisResult);
                                if (!reportedNodes.TryAdd(syntax, true))
                                {
                                    // Avoid duplicates. 
                                    continue;
                                }

                                // NotDisposedRule
                                //      CA2000: In method '{0}', call System.IDisposable.Dispose on object created by '{1}' before all references to it are out of scope.
                                // MayBeDisposedRule
                                //      CA2000: In method '{0}', ensure that disposable object created by '{1}' is disposed on all program paths by wrapping it within a 'using' statement or a try-finally region, with Dispose invocation in finally, such as 'x.Dispose()' or 'x?.Dispose()', which is executed unconditionally.
                                var rule = isNotDisposed ? NotDisposedRule : MayBeDisposedRule;
                                var arg1 = containingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                                var arg2 = syntax.ToString();
                                var diagnostic = syntax.CreateDiagnostic(rule, arg1, arg2);
                                operationBlockContext.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                });
            });
        }
    }
}
