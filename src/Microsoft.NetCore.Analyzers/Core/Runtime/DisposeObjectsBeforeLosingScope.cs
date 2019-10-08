// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DisposeObjectsBeforeLosingScope : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2000";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DisposeObjectsBeforeLosingScopeTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableNotDisposedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DisposeObjectsBeforeLosingScopeNotDisposedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMayBeDisposedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DisposeObjectsBeforeLosingScopeMayBeDisposedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableNotDisposedOnExceptionPathsMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DisposeObjectsBeforeLosingScopeNotDisposedOnExceptionPathsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMayBeDisposedOnExceptionPathsMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DisposeObjectsBeforeLosingScopeMayBeDisposedOnExceptionPathsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DisposeObjectsBeforeLosingScopeDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

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

        internal static DiagnosticDescriptor NotDisposedOnExceptionPathsRule = new DiagnosticDescriptor(RuleId,
                                                                                                        s_localizableTitle,
                                                                                                        s_localizableNotDisposedOnExceptionPathsMessage,
                                                                                                        DiagnosticCategory.Reliability,
                                                                                                        DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                                        isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                                                        description: s_localizableDescription,
                                                                                                        helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope",
                                                                                                        customTags: FxCopWellKnownDiagnosticTags.PortedFxCopDataflowRule);

        internal static DiagnosticDescriptor MayBeDisposedOnExceptionPathsRule = new DiagnosticDescriptor(RuleId,
                                                                                                          s_localizableTitle,
                                                                                                          s_localizableMayBeDisposedOnExceptionPathsMessage,
                                                                                                          DiagnosticCategory.Reliability,
                                                                                                          DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                                          isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                                                          description: s_localizableDescription,
                                                                                                          helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope",
                                                                                                          customTags: FxCopWellKnownDiagnosticTags.PortedFxCopDataflowRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(NotDisposedRule, MayBeDisposedRule, NotDisposedOnExceptionPathsRule, MayBeDisposedOnExceptionPathsRule);

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

                var reportedLocations = new ConcurrentDictionary<Location, bool>();
                compilationContext.RegisterOperationBlockAction(operationBlockContext =>
                {
                    if (!(operationBlockContext.OwningSymbol is IMethodSymbol containingMethod) ||
                        !disposeAnalysisHelper.HasAnyDisposableCreationDescendant(operationBlockContext.OperationBlocks, containingMethod) ||
                        containingMethod.IsConfiguredToSkipAnalysis(operationBlockContext.Options,
                            NotDisposedRule, operationBlockContext.Compilation, operationBlockContext.CancellationToken))
                    {
                        return;
                    }

                    var disposeAnalysisKind = operationBlockContext.Options.GetDisposeAnalysisKindOption(NotDisposedOnExceptionPathsRule, DisposeAnalysisKind.NonExceptionPaths, operationBlockContext.CancellationToken);
                    var trackExceptionPaths = disposeAnalysisKind.AreExceptionPathsEnabled();

                    // For non-exception paths analysis, we can skip interprocedural analysis for certain invocations.
                    var interproceduralAnalysisPredicateOpt = !trackExceptionPaths ?
                        new InterproceduralAnalysisPredicate(
                            skipAnalysisForInvokedMethodPredicateOpt: SkipInterproceduralAnalysis,
                            skipAnalysisForInvokedLambdaOrLocalFunctionPredicateOpt: null,
                            skipAnalysisForInvokedContextPredicateOpt: null) :
                        null;

                    if (disposeAnalysisHelper.TryGetOrComputeResult(operationBlockContext.OperationBlocks, containingMethod,
                        operationBlockContext.Options, NotDisposedRule, trackInstanceFields: false, trackExceptionPaths,
                        operationBlockContext.CancellationToken, out var disposeAnalysisResult, out var pointsToAnalysisResult,
                        interproceduralAnalysisPredicateOpt))
                    {
                        var notDisposedDiagnostics = ArrayBuilder<Diagnostic>.GetInstance();
                        var mayBeNotDisposedDiagnostics = ArrayBuilder<Diagnostic>.GetInstance();
                        try
                        {
                            // Compute diagnostics for undisposed objects at exit block for non-exceptional exit paths.
                            var exitBlock = disposeAnalysisResult.ControlFlowGraph.GetExit();
                            var disposeDataAtExit = disposeAnalysisResult.ExitBlockOutput.Data;
                            ComputeDiagnostics(disposeDataAtExit,
                                notDisposedDiagnostics, mayBeNotDisposedDiagnostics, disposeAnalysisResult, pointsToAnalysisResult,
                                disposeAnalysisKind, isDisposeDataForExceptionPaths: false);

                            if (trackExceptionPaths)
                            {
                                // Compute diagnostics for undisposed objects at handled exception exit paths.
                                var disposeDataAtHandledExceptionPaths = disposeAnalysisResult.ExceptionPathsExitBlockOutputOpt.Data;
                                ComputeDiagnostics(disposeDataAtHandledExceptionPaths,
                                    notDisposedDiagnostics, mayBeNotDisposedDiagnostics, disposeAnalysisResult, pointsToAnalysisResult,
                                    disposeAnalysisKind, isDisposeDataForExceptionPaths: true);

                                // Compute diagnostics for undisposed objects at unhandled exception exit paths, if any.
                                var disposeDataAtUnhandledExceptionPaths = disposeAnalysisResult.MergedStateForUnhandledThrowOperationsOpt?.Data;
                                if (disposeDataAtUnhandledExceptionPaths != null)
                                {
                                    ComputeDiagnostics(disposeDataAtUnhandledExceptionPaths,
                                        notDisposedDiagnostics, mayBeNotDisposedDiagnostics, disposeAnalysisResult, pointsToAnalysisResult,
                                        disposeAnalysisKind, isDisposeDataForExceptionPaths: true);
                                }
                            }

                            if (!notDisposedDiagnostics.Any() && !mayBeNotDisposedDiagnostics.Any())
                            {
                                return;
                            }

                            if (disposeAnalysisResult.ControlFlowGraph.OriginalOperation.HasAnyOperationDescendant(o => o.Kind == OperationKind.None))
                            {
                                // Workaround for https://github.com/dotnet/roslyn/issues/32100
                                // Bail out in presence of OperationKind.None - not implemented IOperation.
                                return;
                            }

                            // Report diagnostics preferring *not* disposed diagnostics over may be not disposed diagnostics
                            // and avoiding duplicates.
                            foreach (var diagnostic in notDisposedDiagnostics.Concat(mayBeNotDisposedDiagnostics))
                            {
                                if (reportedLocations.TryAdd(diagnostic.Location, true))
                                {
                                    operationBlockContext.ReportDiagnostic(diagnostic);
                                }
                            }
                        }
                        finally
                        {
                            notDisposedDiagnostics.Free();
                            mayBeNotDisposedDiagnostics.Free();
                        }
                    }
                });

                return;

                // Local functions.

                bool SkipInterproceduralAnalysis(IMethodSymbol invokedMethod)
                {
                    // Skip interprocedural analysis if we are invoking a method and not passing any disposable object as an argument
                    // and not receiving a disposable object as a return value.
                    // We also check that we are not passing any object type argument which might hold disposable object
                    // and also check that we are not passing delegate type argument which can
                    // be a lambda or local function that has access to disposable object in current method's scope.

                    if (CanBeDisposable(invokedMethod.ReturnType))
                    {
                        return false;
                    }

                    foreach (var p in invokedMethod.Parameters)
                    {
                        if (CanBeDisposable(p.Type))
                        {
                            return false;
                        }
                    }

                    return true;

                    bool CanBeDisposable(ITypeSymbol type)
                        => type.SpecialType == SpecialType.System_Object ||
                            type.DerivesFrom(disposeAnalysisHelper.IDisposable) ||
                            type.TypeKind == TypeKind.Delegate;
                }
            });
        }

        private static void ComputeDiagnostics(
            ImmutableDictionary<AbstractLocation, DisposeAbstractValue> disposeData,
            ArrayBuilder<Diagnostic> notDisposedDiagnostics,
            ArrayBuilder<Diagnostic> mayBeNotDisposedDiagnostics,
            DisposeAnalysisResult disposeAnalysisResult,
            PointsToAnalysisResult pointsToAnalysisResult,
            DisposeAnalysisKind disposeAnalysisKind,
            bool isDisposeDataForExceptionPaths)
        {
            foreach (var kvp in disposeData)
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
                     disposeValue.DisposingOrEscapingOperations.All(d => d.IsInsideCatchRegion(disposeAnalysisResult.ControlFlowGraph) && !location.GetTopOfCreationCallStackOrCreation().IsInsideCatchRegion(disposeAnalysisResult.ControlFlowGraph)));
                var isMayBeNotDisposed = !isNotDisposed && (disposeValue.Kind == DisposeAbstractValueKind.MaybeDisposed || disposeValue.Kind == DisposeAbstractValueKind.NotDisposedOrEscaped);

                if (isNotDisposed ||
                    (isMayBeNotDisposed && disposeAnalysisKind.AreMayBeNotDisposedViolationsEnabled()))
                {
                    var syntax = location.TryGetNodeToReportDiagnostic(pointsToAnalysisResult);
                    if (syntax == null)
                    {
                        continue;
                    }

                    // CA2000: Call System.IDisposable.Dispose on object created by '{0}' before all references to it are out of scope.
                    var rule = GetRule(isNotDisposed);

                    // Ensure that we do not include multiple lines for the object creation expression in the diagnostic message.
                    var argument = syntax.ToString();
                    var indexOfNewLine = argument.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (indexOfNewLine > 0)
                    {
                        argument = argument.Substring(0, indexOfNewLine);
                    }

                    var diagnostic = syntax.CreateDiagnostic(rule, argument);
                    if (isNotDisposed)
                    {
                        notDisposedDiagnostics.Add(diagnostic);
                    }
                    else
                    {
                        mayBeNotDisposedDiagnostics.Add(diagnostic);
                    }
                }
            }

            DiagnosticDescriptor GetRule(bool isNotDisposed)
            {
                if (isNotDisposed)
                {
                    return isDisposeDataForExceptionPaths ? NotDisposedOnExceptionPathsRule : NotDisposedRule;
                }
                else
                {
                    return isDisposeDataForExceptionPaths ? MayBeDisposedOnExceptionPathsRule : MayBeDisposedRule;
                }
            }
        }
    }
}
