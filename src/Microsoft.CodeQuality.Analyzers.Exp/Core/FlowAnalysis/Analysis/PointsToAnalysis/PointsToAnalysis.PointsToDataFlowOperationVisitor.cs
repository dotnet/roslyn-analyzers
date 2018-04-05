// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using PointsToAnalysisData = IDictionary<AnalysisEntity, PointsToAbstractValue>;

    internal partial class PointsToAnalysis : ForwardDataFlowAnalysis<PointsToAnalysisData, PointsToBlockAnalysisResult, PointsToAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the PointsTo values across a given statement in a basic block.
        /// </summary>
        private sealed class PointsToDataFlowOperationVisitor : AnalysisEntityDataFlowOperationVisitor<PointsToAnalysisData, PointsToAbstractValue>
        {
            private readonly DefaultPointsToValueGenerator _defaultPointsToValueGenerator;
            private readonly PointsToAnalysisDomain _pointsToAnalysisDomain;

            public PointsToDataFlowOperationVisitor(
                DefaultPointsToValueGenerator defaultPointsToValueGenerator,
                PointsToAnalysisDomain pointsToAnalysisDomain,
                PointsToAbstractValueDomain valueDomain,
                ISymbol owningSymbol,
                WellKnownTypeProvider wellKnownTypeProvider,
                bool pessimisticAnalysis,
                DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue> copyAnalysisResultOpt)
                : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis: true, copyAnalysisResultOpt: copyAnalysisResultOpt, pointsToAnalysisResultOpt: null)
            {
                _defaultPointsToValueGenerator = defaultPointsToValueGenerator;
                _pointsToAnalysisDomain = pointsToAnalysisDomain;
            }

            public override PointsToAnalysisData Flow(IOperation statement, BasicBlock block, PointsToAnalysisData input)
            {
                if (input != null)
                {
                    // Always set the PointsTo value for the "this" or "Me" instance.
                    input[AnalysisEntityFactory.ThisOrMeInstance] = ThisOrMePointsToAbstractValue;
                }

                return base.Flow(statement, block, input);
            }

            private static bool ShouldBeTracked(ITypeSymbol typeSymbol) => typeSymbol.IsReferenceTypeOrNullableValueType();

            protected override void AddTrackedEntities(ImmutableArray<AnalysisEntity>.Builder builder)
            {
                _defaultPointsToValueGenerator.AddTrackedEntities(builder);

                // Ensure we skip duplicates.
                var defaultPointsToEntities = builder.ToSet();
                foreach (var key in CurrentAnalysisData.Keys)
                {
                    if (!defaultPointsToEntities.Contains(key))
                    {
                        builder.Add(key);
                    }
                }

            }
            protected override bool IsPointsToAnalysis => true;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override PointsToAbstractValue GetAbstractValue(AnalysisEntity analysisEntity)
            {
                if (!ShouldBeTracked(analysisEntity.Type))
                {
                    return PointsToAbstractValue.NoLocation;
                }

                if (!CurrentAnalysisData.TryGetValue(analysisEntity, out var value))
                {
                    value = _defaultPointsToValueGenerator.GetOrCreateDefaultValue(analysisEntity);
                }

                return value;
            }

            protected override PointsToAbstractValue GetPointsToAbstractValue(IOperation operation) => base.GetCachedAbstractValue(operation);

            protected override PointsToAbstractValue GetAbstractDefaultValue(ITypeSymbol type) => !ShouldBeTracked(type) ? PointsToAbstractValue.NoLocation : PointsToAbstractValue.NullLocation;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, PointsToAbstractValue value)
            {
                SetAbstractValue(CurrentAnalysisData, analysisEntity, value);

                if (IsCurrentlyPerformingPredicateAnalysis)
                {
                    SetAbstractValue(NegatedCurrentAnalysisDataStack.Peek(), analysisEntity, value);
                }
            }

            private static void SetAbstractValue(PointsToAnalysisData analysisData, AnalysisEntity analysisEntity, PointsToAbstractValue value)
            {
                if (ShouldBeTracked(analysisEntity.Type))
                {
                    analysisData[analysisEntity] = value;
                }
            }

            private static void SetAbstractValueFromPredicate(PointsToAnalysisData analysisData, AnalysisEntity analysisEntity, IOperation operation, NullAbstractValue nullState)
            {
                Debug.Assert(IsValidValueForPredicateAnalysis(nullState) || nullState == NullAbstractValue.Invalid);
                if (analysisData.TryGetValue(analysisEntity, out PointsToAbstractValue existingValue))
                {
                    PointsToAbstractValue newPointsToValue;
                    switch (nullState)
                    {
                        case NullAbstractValue.Null:
                            newPointsToValue = existingValue.MakeNull();
                            break;

                        case NullAbstractValue.NotNull:
                            newPointsToValue = existingValue.MakeNonNull(operation);
                            break;

                        case NullAbstractValue.Invalid:
                            newPointsToValue = PointsToAbstractValue.Invalid;
                            break;

                        default:
                            throw new InvalidProgramException();
                    }

                    analysisData[analysisEntity] = newPointsToValue;
                }
            }

            protected override void SetValueForParameterOnEntry(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                // Create a dummy PointsTo value for each reference type parameter.
                if (ShouldBeTracked(parameter.Type))
                {
                    var value = PointsToAbstractValue.Create(AbstractLocation.CreateSymbolLocation(parameter), mayBeNull: true);
                    SetAbstractValue(analysisEntity, value);
                }
            }

            protected override void SetValueForParameterOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                // Do not escape the PointsTo value for parameter at exit.
            }

            protected override void ResetCurrentAnalysisData(PointsToAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

            protected override PointsToAbstractValue ComputeAnalysisValueForReferenceOperation(IOperation operation, PointsToAbstractValue defaultValue)
            {
                if (ShouldBeTracked(operation.Type) &&
                    AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity analysisEntity))
                {
                    return GetAbstractValue(analysisEntity);
                }
                else
                {
                    Debug.Assert(operation.Type == null || !operation.Type.IsNonNullableValueType() || defaultValue == PointsToAbstractValue.NoLocation);
                    return defaultValue;
                }
            }

            protected override PointsToAbstractValue ComputeAnalysisValueForOutArgument(AnalysisEntity analysisEntity, IArgumentOperation operation, PointsToAbstractValue defaultValue)
            {
                if (!ShouldBeTracked(analysisEntity.Type))
                {
                    return PointsToAbstractValue.NoLocation;
                }

                var location = AbstractLocation.CreateAllocationLocation(operation, analysisEntity.Type);
                return PointsToAbstractValue.Create(location, mayBeNull: true);
            }

            #region Predicate analysis
            private static bool IsValidValueForPredicateAnalysis(NullAbstractValue value)
            {
                switch (value)
                {
                    case NullAbstractValue.Null:
                    case NullAbstractValue.NotNull:
                        return true;

                    default:
                        return false;
                }
            }

            protected override PredicateValueKind SetValueForEqualsOrNotEqualsComparisonOperator(
                IOperation leftOperand,
                IOperation rightOperand,
                PointsToAnalysisData negatedCurrentAnalysisData,
                bool equals,
                bool isReferenceEquality)
            {
                var predicateValueKind = PredicateValueKind.Unknown;

                // Handle "a == null" and "a != null"
                if (SetValueForComparisonOperator(leftOperand, rightOperand, negatedCurrentAnalysisData, equals, ref predicateValueKind))
                {
                    return predicateValueKind;
                }

                // Otherwise, handle "null == a" and "null != a"
                SetValueForComparisonOperator(rightOperand, leftOperand, negatedCurrentAnalysisData, equals, ref predicateValueKind);
                return predicateValueKind;
            }

            private bool SetValueForComparisonOperator(IOperation target, IOperation assignedValue, PointsToAnalysisData negatedCurrentAnalysisData, bool equals, ref PredicateValueKind predicateValueKind)
            {
                NullAbstractValue value = GetNullAbstractValue(assignedValue);
                if (IsValidValueForPredicateAnalysis(value) &&
                    AnalysisEntityFactory.TryCreate(target, out AnalysisEntity targetEntity))
                {
                    bool inferInCurrentAnalysisData = true;
                    bool inferInNegatedCurrentAnalysisData = true;
                    if (value == NullAbstractValue.NotNull)
                    {
                        // Comparison with a non-null value guarantees that we can infer result in only one of the branches.
                        // For example, predicate "a == c", where we know 'c' is non-null, guarantees 'a' is non-null in CurrentAnalysisData,
                        // but we cannot infer anything about nullness of 'a' in NegatedCurrentAnalysisData.
                        if (equals)
                        {
                            inferInNegatedCurrentAnalysisData = false;
                        }
                        else
                        {
                            inferInCurrentAnalysisData = false;
                        }
                    }

                    CopyAbstractValue copyValue = GetCopyAbstractValue(target);
                    if (copyValue.Kind == CopyAbstractValueKind.Known)
                    {
                        Debug.Assert(copyValue.AnalysisEntities.Contains(targetEntity));
                        foreach (var analysisEntity in copyValue.AnalysisEntities)
                        {
                            SetValueFromPredicate(analysisEntity, value, negatedCurrentAnalysisData, equals,
                                inferInCurrentAnalysisData, inferInNegatedCurrentAnalysisData, target, ref predicateValueKind);
                        }
                    }
                    else
                    {
                        SetValueFromPredicate(targetEntity, value, negatedCurrentAnalysisData, equals,
                            inferInCurrentAnalysisData, inferInNegatedCurrentAnalysisData, target, ref predicateValueKind);
                    }

                    return true;
                }

                return false;
            }

            private void SetValueFromPredicate(
                AnalysisEntity key,
                NullAbstractValue value,
                PointsToAnalysisData negatedCurrentAnalysisData,
                bool equals,
                bool inferInCurrentAnalysisData,
                bool inferInNegatedCurrentAnalysisData,
                IOperation target,
                ref PredicateValueKind predicateValueKind)
            {
                NullAbstractValue negatedValue = NegatePredicateValue(value);
                if (CurrentAnalysisData.TryGetValue(key, out PointsToAbstractValue existingPointsToValue))
                {
                    NullAbstractValue existingNullValue = existingPointsToValue.NullState;
                    if (IsValidValueForPredicateAnalysis(existingNullValue) &&
                        (existingNullValue == NullAbstractValue.Null || value == NullAbstractValue.Null))
                    {
                        if (value == existingNullValue && equals ||
                            negatedValue == existingNullValue && !equals)
                        {
                            predicateValueKind = PredicateValueKind.AlwaysTrue;
                            negatedValue = NullAbstractValue.Invalid;
                            inferInCurrentAnalysisData = false;
                        }

                        if (negatedValue == existingNullValue && equals ||
                            value == existingNullValue && !equals)
                        {
                            predicateValueKind = PredicateValueKind.AlwaysFalse;
                            value = NullAbstractValue.Invalid;
                            inferInNegatedCurrentAnalysisData = false;
                        }
                    }
                }

                if (!equals)
                {
                    if (value != NullAbstractValue.Invalid && negatedValue != NullAbstractValue.Invalid)
                    {
                        var temp = value;
                        value = negatedValue;
                        negatedValue = temp;
                    }
                }

                if (inferInCurrentAnalysisData)
                {
                    // Set value for the CurrentAnalysisData.
                    SetAbstractValueFromPredicate(CurrentAnalysisData, key, target, value);
                }

                if (inferInNegatedCurrentAnalysisData)
                {
                    // Set negated value for the NegatedCurrentAnalysisData.
                    SetAbstractValueFromPredicate(negatedCurrentAnalysisData, key, target, negatedValue);
                }
            }

            private static NullAbstractValue NegatePredicateValue(NullAbstractValue value)
            {
                Debug.Assert(IsValidValueForPredicateAnalysis(value));

                switch (value)
                {
                    case NullAbstractValue.Null:
                        return NullAbstractValue.NotNull;

                    case NullAbstractValue.NotNull:
                        return NullAbstractValue.Null;

                    default:
                        throw new InvalidProgramException();
                }
            }
            #endregion

            // TODO: Remove these temporary methods once we move to compiler's CFG
            // https://github.com/dotnet/roslyn-analyzers/issues/1567
            #region Temporary methods to workaround lack of *real* CFG
            protected override PointsToAnalysisData MergeAnalysisData(PointsToAnalysisData value1, PointsToAnalysisData value2)
                => _pointsToAnalysisDomain.Merge(value1, value2);
            protected override PointsToAnalysisData MergeAnalysisDataForBackEdge(PointsToAnalysisData forwardEdgeAnalysisData, PointsToAnalysisData backEdgeAnalysisData)
                => _pointsToAnalysisDomain.MergeAnalysisDataForBackEdge(forwardEdgeAnalysisData, backEdgeAnalysisData, GetChildAnalysisEntities);
            protected override PointsToAnalysisData GetClonedAnalysisData(PointsToAnalysisData analysisData)
                => GetClonedAnalysisDataHelper(analysisData);
            protected override bool Equals(PointsToAnalysisData value1, PointsToAnalysisData value2)
                => EqualsHelper(value1, value2);
            #endregion

            #region Visitor methods

            public override PointsToAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var value = base.DefaultVisit(operation, argument);

                // Special handling for:
                //  1. Null value: NullLocation
                //  2. Constants and value types do not point to any location.
                if (operation.ConstantValue.HasValue)
                {
                    if (operation.Type == null ||
                        operation.ConstantValue.Value == null)
                    {
                        return PointsToAbstractValue.NullLocation;
                    }
                    else
                    {
                        return PointsToAbstractValue.NoLocation;
                    }
                }
                else if (operation.Type.IsNonNullableValueType())
                {
                    return PointsToAbstractValue.NoLocation;
                }

                return ValueDomain.UnknownOrMayBeValue;
            }

            public override PointsToAbstractValue VisitCoalesce(ICoalesceOperation operation, object argument)
            {
                var value = base.VisitCoalesce(operation, argument);
                var rightNullValue = GetNullAbstractValue(operation.WhenNull);
                if (rightNullValue == NullAbstractValue.NotNull)
                {
                    value = value.MakeNonNull(operation);
                }

                return value;
            }

            public override PointsToAbstractValue VisitAwait(IAwaitOperation operation, object argument)
            {
                var _ = base.VisitAwait(operation, argument);
                return PointsToAbstractValue.Unknown;
            }

            public override PointsToAbstractValue VisitNameOf(INameOfOperation operation, object argument)
            {
                var _ = base.VisitNameOf(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitIsType(IIsTypeOperation operation, object argument)
            {
                var _ = base.VisitIsType(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitInstanceReference(IInstanceReferenceOperation operation, object argument)
            {
                var _ = base.VisitInstanceReference(operation, argument);
                IOperation currentInstanceOperation = operation.GetInstance(IsInsideObjectInitializer);
                var value = currentInstanceOperation != null ?
                    GetCachedAbstractValue(currentInstanceOperation) :
                    ThisOrMePointsToAbstractValue;
                Debug.Assert(value.NullState == NullAbstractValue.NotNull);
                return value;
            }

            private PointsToAbstractValue VisitTypeCreationWithArgumentsAndInitializer(IEnumerable<IOperation> arguments, IObjectOrCollectionInitializerOperation initializer, IOperation operation, object argument)
            {
                AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                var pointsToAbstractValue = PointsToAbstractValue.Create(location, mayBeNull: false);
                CacheAbstractValue(operation, pointsToAbstractValue);

                var unusedArray = VisitArray(arguments, argument);
                var initializerValue = Visit(initializer, argument);
                Debug.Assert(initializer == null || initializerValue == pointsToAbstractValue);
                return pointsToAbstractValue;
            }

            public override PointsToAbstractValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
            {
                return VisitTypeCreationWithArgumentsAndInitializer(operation.Arguments, operation.Initializer, operation, argument);
            }

            public override PointsToAbstractValue VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, object argument)
            {
                return VisitTypeCreationWithArgumentsAndInitializer(operation.Arguments, operation.Initializer, operation, argument);
            }

            public override PointsToAbstractValue VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, object argument)
            {
                AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                var pointsToAbstractValue = PointsToAbstractValue.Create(location, mayBeNull: false);
                CacheAbstractValue(operation, pointsToAbstractValue);

                var _ = base.VisitAnonymousObjectCreation(operation, argument);
                return pointsToAbstractValue;
            }

            public override PointsToAbstractValue VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
            {
                var _ = base.VisitDelegateCreation(operation, argument);
                AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                return PointsToAbstractValue.Create(location, mayBeNull: false);
            }

            public override PointsToAbstractValue VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, object argument)
            {
                var arguments = ImmutableArray<IOperation>.Empty;
                return VisitTypeCreationWithArgumentsAndInitializer(arguments, operation.Initializer, operation, argument);
            }

            public override PointsToAbstractValue VisitMemberInitializer(IMemberInitializerOperation operation, object argument)
            {
                if (operation.InitializedMember is IMemberReferenceOperation memberReference)
                {
                    IOperation objectCreation = operation.GetCreation();
                    PointsToAbstractValue objectCreationLocation = GetCachedAbstractValue(objectCreation);
                    Debug.Assert(objectCreationLocation.Kind == PointsToAbstractValueKind.Known);
                    Debug.Assert(objectCreationLocation.Locations.Count == 1);

                    PointsToAbstractValue memberInstanceLocation = PointsToAbstractValue.Create(AbstractLocation.CreateAllocationLocation(operation, memberReference.Type), mayBeNull: false);
                    CacheAbstractValue(operation, memberInstanceLocation);
                    CacheAbstractValue(operation.Initializer, memberInstanceLocation);

                    var unusedInitializedMemberValue = Visit(memberReference, argument);
                    var initializerValue = Visit(operation.Initializer, argument);
                    Debug.Assert(operation.Initializer == null || initializerValue == memberInstanceLocation);
                    SetAbstractValueForAssignment(memberReference, operation, memberInstanceLocation);

                    return memberInstanceLocation;
                }

                var _ = base.Visit(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, object argument)
            {
                var _ = base.VisitObjectOrCollectionInitializer(operation, argument);

                // We should have created a new PointsTo value for the associated creation operation.
                return GetCachedAbstractValue(operation.Parent);
            }

            public override PointsToAbstractValue VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
            {
                var _ = base.VisitArrayInitializer(operation, argument);

                // We should have created a new PointsTo value for the associated array creation operation.
                return GetCachedAbstractValue(operation.GetAncestor<IArrayCreationOperation>(OperationKind.ArrayCreation));
            }

            public override PointsToAbstractValue VisitArrayCreation(IArrayCreationOperation operation, object argument)
            {
                var pointsToAbstractValue = PointsToAbstractValue.Create(AbstractLocation.CreateAllocationLocation(operation, operation.Type), mayBeNull: false);
                CacheAbstractValue(operation, pointsToAbstractValue);

                var unusedDimensionsValue = VisitArray(operation.DimensionSizes, argument);
                var initializerValue = Visit(operation.Initializer, argument);
                Debug.Assert(operation.Initializer == null || initializerValue == pointsToAbstractValue);
                return pointsToAbstractValue;
            }

            public override PointsToAbstractValue VisitIsPattern(IIsPatternOperation operation, object argument)
            {
                // TODO: Handle patterns
                // https://github.com/dotnet/roslyn-analyzers/issues/1571
                return base.VisitIsPattern(operation, argument);
            }

            public override PointsToAbstractValue VisitDeclarationPattern(IDeclarationPatternOperation operation, object argument)
            {
                // TODO: Handle patterns
                // https://github.com/dotnet/roslyn-analyzers/issues/1571
                return base.VisitDeclarationPattern(operation, argument);
            }

            public override PointsToAbstractValue VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                var _ = base.VisitInterpolatedString(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitBinaryOperator_NonConditional(IBinaryOperation operation, object argument)
            {
                var _ = base.VisitBinaryOperator_NonConditional(operation, argument);
                return PointsToAbstractValue.Unknown;
            }

            public override PointsToAbstractValue VisitSizeOf(ISizeOfOperation operation, object argument)
            {
                var _ = base.VisitSizeOf(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitTypeOf(ITypeOfOperation operation, object argument)
            {
                var _ = base.VisitTypeOf(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitThrowCore(IThrowOperation operation, object argument)
            {
                var _ = base.VisitThrowCore(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            private PointsToAbstractValue VisitInvocationCommon(IOperation operation, IOperation instance)
            {
                if (ShouldBeTracked(operation.Type))
                {
                    AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                    var pointsToAbstractValue = PointsToAbstractValue.Create(location, mayBeNull: true);
                    return GetValueBasedOnInstanceOrReferenceValue(referenceOrInstance: instance, operation: operation, defaultValue: pointsToAbstractValue);
                }
                else
                {
                    return PointsToAbstractValue.NoLocation;
                }
            }

            public override PointsToAbstractValue VisitInvocation_LambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var _ = base.VisitInvocation_LambdaOrDelegateOrLocalFunction(operation, argument);
                return VisitInvocationCommon(operation, operation.Instance);
            }

            public override PointsToAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var _ = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
                return VisitInvocationCommon(operation, operation.Instance);
            }

            public override PointsToAbstractValue VisitDynamicInvocation(IDynamicInvocationOperation operation, object argument)
            {
                var _ = base.VisitDynamicInvocation(operation, argument);
                return VisitInvocationCommon(operation, operation.Operation);
            }

            private NullAbstractValue GetNullStateBasedOnInstanceOrReferenceValue(IOperation referenceOrInstance, ITypeSymbol operationType, NullAbstractValue defaultValue)
            {
                if (operationType.IsNonNullableValueType())
                {
                    return NullAbstractValue.NotNull;
                }

                NullAbstractValue referenceOrInstanceValue = referenceOrInstance != null ? GetNullAbstractValue(referenceOrInstance) : NullAbstractValue.NotNull;
                return referenceOrInstanceValue == NullAbstractValue.Null ? NullAbstractValue.Null : defaultValue;
            }

            private PointsToAbstractValue GetValueBasedOnInstanceOrReferenceValue(IOperation referenceOrInstance, IOperation operation, PointsToAbstractValue defaultValue)
            {
                NullAbstractValue nullState = GetNullStateBasedOnInstanceOrReferenceValue(referenceOrInstance, operation.Type, defaultValue.NullState);
                switch (nullState)
                {
                    case NullAbstractValue.NotNull:
                        return defaultValue.MakeNonNull(operation);

                    case NullAbstractValue.Null:
                        return defaultValue.MakeNull();

                    default:
                        return defaultValue;
                }
            }

            public override PointsToAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                var value = base.VisitFieldReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation, value);
            }

            public override PointsToAbstractValue VisitPropertyReference(IPropertyReferenceOperation operation, object argument)
            {
                var value = base.VisitPropertyReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation, value);
            }

            public override PointsToAbstractValue VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, object argument)
            {
                var value = base.VisitDynamicMemberReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation, value);
            }

            public override PointsToAbstractValue VisitMethodReferenceCore(IMethodReferenceOperation operation, object argument)
            {
                var value = base.VisitMethodReferenceCore(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation, value);
            }

            public override PointsToAbstractValue VisitEventReference(IEventReferenceOperation operation, object argument)
            {
                var value = base.VisitEventReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation, value);
            }

            public override PointsToAbstractValue VisitArrayElementReference(IArrayElementReferenceOperation operation, object argument)
            {
                var value = base.VisitArrayElementReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.ArrayReference, operation, value);
            }

            public override PointsToAbstractValue VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, object argument)
            {
                var value = base.VisitDynamicIndexerAccess(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation, operation, value);
            }

            public override PointsToAbstractValue VisitConversion(IConversionOperation operation, object argument)
            {
                var value = base.VisitConversion(operation, argument);
                if (value.NullState == NullAbstractValue.NotNull)
                {
                    if (TryInferConversion(operation, out bool alwaysSucceed, out bool alwaysFail))
                    {
                        Debug.Assert(!alwaysSucceed || !alwaysFail);
                        if (alwaysFail)
                        {
                            value = value.MakeNull();
                        }
                        else if (operation.IsTryCast && !alwaysSucceed)
                        {
                            // TryCast which may or may not succeed.
                            value = value.MakeMayBeNull();
                        }
                    }
                    else
                    {
                        value = value.MakeMayBeNull();
                    }
                }

                return value;
            }

            #endregion
        }
    }
}
