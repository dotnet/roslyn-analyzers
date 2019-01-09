// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.DisposeAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DisposableFieldsShouldBeDisposed : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2213";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftUsageAnalyzersResources.DisposableFieldsShouldBeDisposedTitle), MicrosoftUsageAnalyzersResources.ResourceManager, typeof(MicrosoftUsageAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftUsageAnalyzersResources.DisposableFieldsShouldBeDisposedMessage), MicrosoftUsageAnalyzersResources.ResourceManager, typeof(MicrosoftUsageAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftUsageAnalyzersResources.DisposableFieldsShouldBeDisposedDescription), MicrosoftUsageAnalyzersResources.ResourceManager, typeof(MicrosoftUsageAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2213-disposable-fields-should-be-disposed",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

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

                var fieldDisposeValueMap = new ConcurrentDictionary<IFieldSymbol, /*disposed*/bool>();
                void addOrUpdateFieldDisposedValue(IFieldSymbol field, bool disposed)
                {
                    Debug.Assert(!field.IsStatic);
                    Debug.Assert(field.Type.IsDisposable(disposeAnalysisHelper.IDisposable));

                    fieldDisposeValueMap.AddOrUpdate(field,
                        addValue: disposed,
                        updateValueFactory: (f, currentValue) => currentValue || disposed);
                };

                // Disposable fields with initializer at declaration must be disposed.
                compilationContext.RegisterOperationAction(operationContext =>
                {
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
                    if (!(operationBlockStartContext.OwningSymbol is IMethodSymbol containingMethod))
                    {
                        return;
                    }

                    if (disposeAnalysisHelper.HasAnyDisposableCreationDescendant(operationBlockStartContext.OperationBlocks, containingMethod))
                    {
                        DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> disposeAnalysisResult;
                        DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResult;
                        if (disposeAnalysisHelper.TryGetOrComputeResult(operationBlockStartContext.OperationBlocks, containingMethod, out disposeAnalysisResult, out pointsToAnalysisResult))
                        {
                            Debug.Assert(disposeAnalysisResult != null);
                            Debug.Assert(pointsToAnalysisResult != null);

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
                                    PointsToAbstractValue assignedPointsToValue = pointsToAnalysisResult[simpleAssignmentOperation.Value];
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
                    }

                    // Mark fields disposed in Dispose method(s).
                    if (containingMethod.GetDisposeMethodKind(disposeAnalysisHelper.IDisposable, disposeAnalysisHelper.Task) != DisposeMethodKind.None)
                    {
                        var disposableFields = disposeAnalysisHelper.GetDisposableFields(containingMethod.ContainingType);
                        if (!disposableFields.IsEmpty)
                        {
                            DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> disposeAnalysisResult;
                            DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResult;
                            ImmutableDictionary<IFieldSymbol, PointsToAbstractValue> trackedInstanceFieldPointsToMap;
                            if (disposeAnalysisHelper.TryGetOrComputeResult(operationBlockStartContext.OperationBlocks, containingMethod, trackInstanceFields: true,
                                disposeAnalysisResult: out disposeAnalysisResult, pointsToAnalysisResult: out pointsToAnalysisResult, trackedInstanceFieldPointsToMap: out trackedInstanceFieldPointsToMap))
                            {
                                BasicBlock exitBlock = disposeAnalysisResult.ControlFlowGraph.Exit;
                                foreach (var fieldWithPointsToValue in trackedInstanceFieldPointsToMap)
                                {
                                    IFieldSymbol field = fieldWithPointsToValue.Key;
                                    PointsToAbstractValue pointsToValue = fieldWithPointsToValue.Value;

                                    Debug.Assert(field.Type.IsDisposable(disposeAnalysisHelper.IDisposable));
                                    ImmutableDictionary<AbstractLocation, DisposeAbstractValue> disposeDataAtExit = disposeAnalysisResult[exitBlock].OutputData;
                                    var disposed = false;
                                    foreach (var location in pointsToValue.Locations)
                                    {
                                        if (disposeDataAtExit.TryGetValue(location, out DisposeAbstractValue disposeValue))
                                        {
                                            switch (disposeValue.Kind)
                                            {
                                                // For MaybeDisposed, conservatively mark the field as disposed as we don't support path sensitive analysis.
                                                case DisposeAbstractValueKind.MaybeDisposed:
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
            });
        }
    }
}
