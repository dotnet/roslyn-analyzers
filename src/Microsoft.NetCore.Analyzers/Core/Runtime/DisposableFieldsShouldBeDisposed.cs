﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Threading;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DisposableFieldsShouldBeDisposed : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2213";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposableFieldsShouldBeDisposedTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposableFieldsShouldBeDisposedMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DisposableFieldsShouldBeDisposedDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopDataflowRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                if (!DisposeAnalysisHelper.TryGetOrCreate(compilationContext.Compilation, out DisposeAnalysisHelper disposeAnalysisHelper))
                {
                    return;
                }

                var fieldDisposeValueMap = new ConcurrentDictionary<IFieldSymbol, /*disposed*/bool>();
                void addOrUpdateFieldDisposedValue(IFieldSymbol field, bool disposed)
                {
                    Debug.Assert(!field.IsStatic);
                    Debug.Assert(field.Type.IsDisposable(disposeAnalysisHelper.IDisposable));

                    fieldDisposeValueMap.AddOrUpdate(field,
                        addValue: disposed,
                        updateValueFactory: (f, currentValue) => currentValue || disposed);
                };

                var hasErrors = false;
                compilationContext.RegisterOperationAction(_ => hasErrors = true, OperationKind.Invalid);

                // Disposable fields with initializer at declaration must be disposed.
                compilationContext.RegisterOperationAction(operationContext =>
                {
                    if (!ShouldAnalyze(operationContext.ContainingSymbol.ContainingType))
                    {
                        return;
                    }

                    var initializedFields = ((IFieldInitializerOperation)operationContext.Operation).InitializedFields;
                    foreach (var field in initializedFields)
                    {
                        if (!field.IsStatic &&
                            disposeAnalysisHelper.GetDisposableFields(field.ContainingType).Contains(field))
                        {
                            addOrUpdateFieldDisposedValue(field, disposed: false);
                        }
                    }
                },
                OperationKind.FieldInitializer);

                // Instance fields initialized in constructor/method body with a locally created disposable object must be disposed.
                compilationContext.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    if (!(operationBlockStartContext.OwningSymbol is IMethodSymbol containingMethod) ||
                        !ShouldAnalyze(containingMethod.ContainingType))
                    {
                        return;
                    }

                    if (disposeAnalysisHelper.HasAnyDisposableCreationDescendant(operationBlockStartContext.OperationBlocks, containingMethod))
                    {
                        PointsToAnalysisResult lazyPointsToAnalysisResult = null;

                        operationBlockStartContext.RegisterOperationAction(operationContext =>
                        {
                            var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                            var field = fieldReference.Field;

                            // Only track instance fields on the current instance.
                            if (field.IsStatic || fieldReference.Instance?.Kind != OperationKind.InstanceReference)
                            {
                                return;
                            }

                            // Check if this is a Disposable field that is not currently being tracked.
                            if (fieldDisposeValueMap.ContainsKey(field) ||
                                !disposeAnalysisHelper.GetDisposableFields(field.ContainingType).Contains(field))
                            {
                                return;
                            }

                            // We have a field reference for a disposable field.
                            // Check if it is being assigned a locally created disposable object.
                            if (fieldReference.Parent is ISimpleAssignmentOperation simpleAssignmentOperation &&
                                simpleAssignmentOperation.Target == fieldReference)
                            {
                                if (lazyPointsToAnalysisResult == null)
                                {
                                    var cfg = operationBlockStartContext.OperationBlocks.GetControlFlowGraph();
                                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(operationContext.Compilation);
                                    var interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
                                        operationBlockStartContext.Options, Rule, InterproceduralAnalysisKind.None, operationBlockStartContext.CancellationToken);
                                    var pointsToAnalysisResult = PointsToAnalysis.TryGetOrComputeResult(cfg,
                                        containingMethod, wellKnownTypeProvider, interproceduralAnalysisConfig,
                                        interproceduralAnalysisPredicateOpt: null,
                                        pessimisticAnalysis: false, performCopyAnalysis: false);
                                    if (pointsToAnalysisResult == null)
                                    {
                                        hasErrors = true;
                                        return;
                                    }

                                    Interlocked.CompareExchange(ref lazyPointsToAnalysisResult, pointsToAnalysisResult, null);
                                }

                                PointsToAbstractValue assignedPointsToValue = lazyPointsToAnalysisResult[simpleAssignmentOperation.Value.Kind, simpleAssignmentOperation.Value.Syntax];
                                foreach (var location in assignedPointsToValue.Locations)
                                {
                                    if (disposeAnalysisHelper.IsDisposableCreationOrDisposeOwnershipTransfer(location, containingMethod))
                                    {
                                        addOrUpdateFieldDisposedValue(field, disposed: false);
                                        break;
                                    }
                                }
                            }
                        },
                        OperationKind.FieldReference);
                    }

                    // Mark fields disposed in Dispose method(s).
                    if (containingMethod.GetDisposeMethodKind(disposeAnalysisHelper.IDisposable, disposeAnalysisHelper.Task) != DisposeMethodKind.None)
                    {
                        var disposableFields = disposeAnalysisHelper.GetDisposableFields(containingMethod.ContainingType);
                        if (!disposableFields.IsEmpty)
                        {
                            if (disposeAnalysisHelper.TryGetOrComputeResult(operationBlockStartContext.OperationBlocks, containingMethod,
                                operationBlockStartContext.Options, Rule, trackInstanceFields: true, trackExceptionPaths: false, cancellationToken: operationBlockStartContext.CancellationToken,
                                disposeAnalysisResult: out var disposeAnalysisResult, pointsToAnalysisResult: out var pointsToAnalysisResult))
                            {
                                BasicBlock exitBlock = disposeAnalysisResult.ControlFlowGraph.GetExit();
                                foreach (var fieldWithPointsToValue in disposeAnalysisResult.TrackedInstanceFieldPointsToMap)
                                {
                                    IFieldSymbol field = fieldWithPointsToValue.Key;
                                    PointsToAbstractValue pointsToValue = fieldWithPointsToValue.Value;

                                    Debug.Assert(field.Type.IsDisposable(disposeAnalysisHelper.IDisposable));
                                    ImmutableDictionary<AbstractLocation, DisposeAbstractValue> disposeDataAtExit = disposeAnalysisResult.ExitBlockOutput.Data;
                                    var disposed = false;
                                    foreach (var location in pointsToValue.Locations)
                                    {
                                        if (disposeDataAtExit.TryGetValue(location, out DisposeAbstractValue disposeValue))
                                        {
                                            switch (disposeValue.Kind)
                                            {
                                                // For MaybeDisposed, conservatively mark the field as disposed as we don't support path sensitive analysis.
                                                case DisposeAbstractValueKind.MaybeDisposed:
                                                case DisposeAbstractValueKind.Unknown:
                                                case DisposeAbstractValueKind.Escaped:
                                                case DisposeAbstractValueKind.Disposed:
                                                    disposed = true;
                                                    addOrUpdateFieldDisposedValue(field, disposed);
                                                    break;
                                            }
                                        }

                                        if (disposed)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

                compilationContext.RegisterCompilationEndAction(compilationEndContext =>
                {
                    if (hasErrors)
                    {
                        return;
                    }

                    foreach (var kvp in fieldDisposeValueMap)
                    {
                        IFieldSymbol field = kvp.Key;
                        bool disposed = kvp.Value;
                        if (!disposed)
                        {
                            // '{0}' contains field '{1}' that is of IDisposable type '{2}', but it is never disposed. Change the Dispose method on '{0}' to call Close or Dispose on this field.
                            var arg1 = field.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            var arg2 = field.Name;
                            var arg3 = field.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                            var diagnostic = field.CreateDiagnostic(Rule, arg1, arg2, arg3);
                            compilationEndContext.ReportDiagnostic(diagnostic);
                        }
                    }
                });

                return;

                // Local functions
                bool ShouldAnalyze(INamedTypeSymbol namedType)
                {
                    // We only want to analyze types which are disposable (implement System.IDisposable directly or indirectly)
                    // and have at least one disposable field.
                    return !hasErrors &&
                        namedType.IsDisposable(disposeAnalysisHelper.IDisposable) &&
                        !disposeAnalysisHelper.GetDisposableFields(namedType).IsEmpty;
                }
            });
        }
    }
}
