// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using Analyzer.Utilities;
    using Analyzer.Utilities.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
    using Microsoft.CodeAnalysis.Operations;

    /// <summary>
    /// Represents analysis of type checking and casting.
    /// </summary>
    internal partial class ExtraTypeCheckingAnalysis : ForwardDataFlowAnalysis<ExtraTypeCheckingAnalysisData, ExtraTypeCheckingAnalysisContext, ExtraTypeCheckingAnalysisResult, ExtraTypeCheckingBlockAnalysisResult, SimpleAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the dispose values across a given statement in a basic block.
        /// </summary>
        private sealed class ExtraTypeCheckingDataFlowOperationVisitor : DataFlowOperationVisitor<ExtraTypeCheckingAnalysisData, ExtraTypeCheckingAnalysisContext, ExtraTypeCheckingAnalysisResult, SimpleAbstractValue>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExtraTypeCheckingDataFlowOperationVisitor"/> class.
            /// </summary>
            /// <param name="analysisContext">The analysis context.</param>
            public ExtraTypeCheckingDataFlowOperationVisitor(ExtraTypeCheckingAnalysisContext analysisContext)
                : base(analysisContext)
            {
                Debug.Assert(analysisContext.PointsToAnalysisResult != null, "No PointsToAnalysisResultOpt available.");
            }

            /// <inheritdoc />
            public override ExtraTypeCheckingAnalysisData GetEmptyAnalysisData()
            {
                return new ExtraTypeCheckingAnalysisData();
            }

            /// <inheritdoc />
            public override SimpleAbstractValue DefaultVisit(IOperation operation, object? argument)
            {
                _ = base.DefaultVisit(operation, argument);
                return SimpleAbstractValue.None;
            }

            /// <inheritdoc />
            public override SimpleAbstractValue VisitIsType(IIsTypeOperation operation, object? argument)
            {
                SimpleAbstractValue value = base.VisitIsType(operation, argument);

                if (operation.ValueOperand is ILocalReferenceOperation localReferenceOperation)
                {
                    this.UpdateAccess(operation, localReferenceOperation, operation.TypeOperand);
                    return SimpleAbstractValue.Unknown;
                }

                return value;
            }

            /// <inheritdoc />
            public override SimpleAbstractValue VisitConversion(IConversionOperation operation, object? argument)
            {
                SimpleAbstractValue value = base.VisitConversion(operation, argument);

                if (operation.Kind == OperationKind.Conversion)
                {
                    if (operation.Operand is ILocalReferenceOperation localReference &&
                        (operation.IsTryCast ||
                         operation.Syntax is CastExpressionSyntax))
                    {
                        // Cast or As operator
                        this.UpdateAccess(operation, localReference, operation.Type);
                        return SimpleAbstractValue.Unknown;
                    }
                }

                return value;
            }

            /// <inheritdoc />
            protected override void StopTrackingDataForParameter(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                Debug.Assert(this.DataFlowAnalysisContext.InterproceduralAnalysisData != null, "Expected data opt to not be null.");
            }

            /// <inheritdoc />
            protected override void EscapeValueForParameterOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                Debug.Assert(SymbolEqualityComparer.Default.Equals(analysisEntity.Symbol, parameter), "Expecting symbolopt to be the parameter.");
            }

            /// <inheritdoc />
            protected override void ResetValueTypeInstanceAnalysisData(AnalysisEntity analysisEntity)
            {
            }

            /// <inheritdoc />
            protected override void ResetReferenceTypeInstanceAnalysisData(PointsToAbstractValue pointsToAbstractValue)
            {
            }

            /// <inheritdoc />
            protected override SimpleAbstractValue GetAbstractDefaultValue(ITypeSymbol type) => SimpleAbstractValue.None;

            /// <inheritdoc />
            protected override bool HasAnyAbstractValue(ExtraTypeCheckingAnalysisData data) => data.Data.Count > 0;

            /// <inheritdoc />
            protected override void ResetCurrentAnalysisData()
            {
                foreach (KeyValuePair<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> kvp in this.CurrentAnalysisData.Data)
                {
                    this.SetAbstractValue(kvp.Key, this.ValueDomain.UnknownOrMayBeValue);
                }
            }

            /// <inheritdoc />
            protected override ExtraTypeCheckingAnalysisData MergeAnalysisData(ExtraTypeCheckingAnalysisData value1, ExtraTypeCheckingAnalysisData value2)
                => ExtraTypeCheckingAnalysisDomainInstance.Merge(value1, value2);

            /// <inheritdoc />
            protected override ExtraTypeCheckingAnalysisData MergeAnalysisDataForBackEdge(ExtraTypeCheckingAnalysisData value1, ExtraTypeCheckingAnalysisData value2, BasicBlock block)
            {
                // Prevent back edge processing by returning the original for now.
                // TODO: It is not processing back edges properly causing endless looping.
                return value1;
            }

            /// <inheritdoc />
            protected override ExtraTypeCheckingAnalysisData GetClonedAnalysisData(ExtraTypeCheckingAnalysisData analysisData)
            {
                return new ExtraTypeCheckingAnalysisData(this.CurrentAnalysisData);
            }

            /// <inheritdoc />
            protected override ExtraTypeCheckingAnalysisData GetExitBlockOutputData(ExtraTypeCheckingAnalysisResult analysisResult)
            {
                return new ExtraTypeCheckingAnalysisData(analysisResult.ExitBlockOutput.Data);
            }

            /// <inheritdoc />
            protected override bool Equals(ExtraTypeCheckingAnalysisData value1, ExtraTypeCheckingAnalysisData value2)
                => EqualsHelper(value1.Data, value2.Data);

            /// <inheritdoc />
            protected override void UpdateValuesForAnalysisData(ExtraTypeCheckingAnalysisData targetAnalysisData)
            {
                // Call base class implementation.
                UpdateValuesForAnalysisData(targetAnalysisData, this.CurrentAnalysisData);
            }

            /// <inheritdoc />
            protected override void ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(ExtraTypeCheckingAnalysisData dataAtException, ThrownExceptionInfo throwBranchWithExceptionType)
            {
                ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(dataAtException, this.CurrentAnalysisData, predicate: default);
            }

            /// <summary>
            /// The base class does not allow us to pass our new class.
            /// </summary>
            /// <param name="coreDataAtException">Core data at exception.</param>
            /// <param name="coreCurrentAnalysisData">Current analysis data.</param>
            /// <param name="predicate">Predicate.</param>
            private static void ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(
                       ExtraTypeCheckingAnalysisData coreDataAtException,
                       ExtraTypeCheckingAnalysisData coreCurrentAnalysisData,
                       Func<ExtraTypeCheckingAbstractLocation, bool>? predicate)
            {
                foreach (var kvp in coreCurrentAnalysisData.Data)
                {
                    if (coreDataAtException.Data.ContainsKey(kvp.Key) ||
                        (predicate != null && !predicate(kvp.Key)))
                    {
                        continue;
                    }

                    coreDataAtException.Data.Add(kvp.Key, kvp.Value);
                }
            }

            /// <summary>
            /// Update values for analysis data.
            /// </summary>
            /// <param name="targetAnalysisData">The target analysis data.</param>
            /// <param name="newAnalysisData">The new analysis data.</param>
            private static void UpdateValuesForAnalysisData(
                    ExtraTypeCheckingAnalysisData targetAnalysisData,
                    ExtraTypeCheckingAnalysisData newAnalysisData)
            {
                foreach (KeyValuePair<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> kvp in targetAnalysisData.Data)
                {
                    if (newAnalysisData.Data.TryGetValue(kvp.Key, out SimpleAbstractValue newValue))
                    {
                        targetAnalysisData.Data[kvp.Key] = newValue;
                    }
                }
            }

            /// <summary>
            /// Updates access information for the location.
            /// </summary>
            /// <param name="instance">The operation instance.</param>
            /// <param name="localReference">The local reference.</param>
            /// <param name="targetType">The target type.</param>
            private void UpdateAccess(
                  IOperation instance,
                  ILocalReferenceOperation localReference,
                  ITypeSymbol targetType)
            {
                if (targetType.IsValueType)
                {
                    // 'is' operator is required for value type that is non-nullable because
                    // 'as' operator cannot be used on non-nullable types.
                    return;
                }

                if (this.DataFlowAnalysisContext.ControlFlowGraph?.OriginalOperation?.SemanticModel?.Compilation != null &&
                    targetType.Inherits(WellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemException)))
                {
                    // 'is' operator with exception types is common in exception handling code.
                    // Don't flag casting with exception types because it is commonly found in exception blocks and doesn't get fixed.
                    return;
                }

                AbstractLocation accessLocation = AbstractLocation.CreateSymbolLocation(localReference.Local, ImmutableStack.Create(instance));
                PointsToAbstractValue instanceLocation = this.GetPointsToAbstractValue(localReference);

                foreach (AbstractLocation scopeLocation in instanceLocation.Locations)
                {
                    ExtraTypeCheckingAbstractLocation lastLocation = new ExtraTypeCheckingAbstractLocation(scopeLocation, targetType, Location.None);
                    ExtraTypeCheckingAbstractLocation nextLocation = new ExtraTypeCheckingAbstractLocation(scopeLocation, targetType, instance.Syntax.GetLocation());
                    SimpleAbstractValue nextValue;
                    if (this.CurrentAnalysisData.Data.TryGetValue(lastLocation, out SimpleAbstractValue lastLocationValue))
                    {
                        bool isTypeChecking = lastLocationValue.AccessLocation?.DiagnosticLocation is BinaryExpressionSyntax binaryExpression && binaryExpression.IsKind(SyntaxKind.IsExpression);

                        nextValue = SimpleAbstractValue.Access
                            .WithAccessLocation(accessLocation, instance, localReference.Local, instance.Syntax, isTypeChecking)
                            .WithPreviousLocations(new List<SimpleAbstractValue>(1) { lastLocationValue });
                    }
                    else
                    {
                        // This is the first type check for the (variable, target type) combination.
                        nextValue = SimpleAbstractValue.Access.WithAccessLocation(accessLocation, instance, localReference.Local, instance.Syntax, isTypeChecking: false);
                    }

                    if (nextValue != null)
                    {
                        // Update the last access, and set the current access.
                        this.SetAbstractValue(lastLocation, nextValue);
                        this.SetAbstractValue(nextLocation, nextValue);
                    }
                }
            }

            /// <summary>
            /// Sets an abstract value.
            /// </summary>
            /// <param name="location">The location used as key.</param>
            /// <param name="value">The abstract value to set.</param>
            private void SetAbstractValue(ExtraTypeCheckingAbstractLocation location, SimpleAbstractValue value)
            {
                if (!location.Location.IsNull)
                {
                    this.CurrentAnalysisData.Data[location] = value;
                }
            }

            protected override void SetValueForParameterOnEntry(IParameterSymbol parameter, AnalysisEntity analysisEntity, ArgumentInfo<SimpleAbstractValue>? assignedValue)
            {
                throw new NotImplementedException();
            }

            protected override void SetAbstractValueForArrayElementInitializer(IArrayCreationOperation arrayCreation, ImmutableArray<AbstractIndex> indices, ITypeSymbol elementType, IOperation initializer, SimpleAbstractValue value)
            {
                throw new NotImplementedException();
            }

            protected override void SetAbstractValueForAssignment(IOperation target, IOperation? assignedValueOperation, SimpleAbstractValue assignedValue, bool mayBeAssignment = false)
            {
                throw new NotImplementedException();
            }

            protected override void SetAbstractValueForTupleElementAssignment(AnalysisEntity tupleElementEntity, IOperation assignedValueOperation, SimpleAbstractValue assignedValue)
            {
                throw new NotImplementedException();
            }
        }
    }
}
