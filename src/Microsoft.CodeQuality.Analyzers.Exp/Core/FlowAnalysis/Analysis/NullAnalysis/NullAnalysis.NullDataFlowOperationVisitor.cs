// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    using NullAnalysisData = IDictionary<AnalysisEntity, NullAbstractValue>;

    internal partial class NullAnalysis : ForwardDataFlowAnalysis<NullAnalysisData, NullBlockAnalysisResult, NullAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the null values across a given statement in a basic block.
        /// </summary>
        private sealed class NullDataFlowOperationVisitor : AnalysisEntityDataFlowOperationVisitor<NullAnalysisData, NullAbstractValue>
        {
            public NullDataFlowOperationVisitor(
                NullAbstractValueDomain valueDomain,
                ISymbol owningSymbol,
                WellKnownTypeProvider wellKnownTypeProvider,
                bool pessimisticAnalysis,
                DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue> copyAnalysisResultOpt,
                DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt)
                : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis: true, nullAnalysisResultOpt: null, copyAnalysisResultOpt: copyAnalysisResultOpt, pointsToAnalysisResultOpt: pointsToAnalysisResultOpt)
            {
            }

            protected override IEnumerable<AnalysisEntity> TrackedEntities => CurrentAnalysisData.Keys;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, NullAbstractValue value) => SetAbstractValue(CurrentAnalysisData, analysisEntity, value);

            private static void SetAbstractValue(NullAnalysisData analysisData, AnalysisEntity analysisEntity, NullAbstractValue value)
            {
                if (analysisEntity.Type.IsReferenceType)
                {
                    analysisData[analysisEntity] = value;
                }
            }

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override NullAbstractValue GetAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.TryGetValue(analysisEntity, out var value) ? value : ValueDomain.UnknownOrMayBeValue;

            protected override NullAbstractValue GetAbstractDefaultValue(ITypeSymbol type)
            {
                if (type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                return NullAbstractValue.Null;
            }

            protected override NullAbstractValue GetNullAbstractValue(IOperation operation) => GetCachedAbstractValue(operation);

            protected override void ResetCurrentAnalysisData(NullAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

            protected override NullAbstractValue GetDefaultValueForParameterOnEntry(ITypeSymbol parameterType)
                => parameterType.IsValueType ? NullAbstractValue.NotNull : NullAbstractValue.MaybeNull;

            protected override NullAbstractValue GetDefaultValueForParameterOnExit(ITypeSymbol parameterType)
                => parameterType.IsValueType ? NullAbstractValue.NotNull : NullAbstractValue.MaybeNull;

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

            protected override PredicateValueKind SetValueForEqualsOrNotEqualsComparisonOperator(IBinaryOperation operation, NullAnalysisData negatedCurrentAnalysisData, bool equals)
            {
                Debug.Assert(operation.IsComparisonOperator());
                var predicateValueKind = PredicateValueKind.Unknown;

                // Handle "a == null" and "a != null"
                if (SetValueForComparisonOperator(operation.LeftOperand, operation.RightOperand, negatedCurrentAnalysisData, equals, ref predicateValueKind))
                {
                    return predicateValueKind;
                }

                // Otherwise, handle "null == a" and "null != a"
                SetValueForComparisonOperator(operation.RightOperand, operation.LeftOperand, negatedCurrentAnalysisData, equals, ref predicateValueKind);
                return predicateValueKind;
            }

            private bool SetValueForComparisonOperator(IOperation target, IOperation assignedValue, NullAnalysisData negatedCurrentAnalysisData, bool equals, ref PredicateValueKind predicateValueKind)
            {
                NullAbstractValue nullValue = GetNullAbstractValue(assignedValue);
                if (IsValidValueForPredicateAnalysis(nullValue) &&
                    AnalysisEntityFactory.TryCreate(target, out AnalysisEntity targetEntity))
                {
                    bool inferInCurrentAnalysisData = true;
                    bool inferInNegatedCurrentAnalysisData = true;
                    if (nullValue == NullAbstractValue.NotNull)
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
                            SetValueFromPredicate(analysisEntity, nullValue, negatedCurrentAnalysisData, equals, inferInCurrentAnalysisData, inferInNegatedCurrentAnalysisData, ref predicateValueKind);
                        }
                    }
                    else
                    {
                        SetValueFromPredicate(targetEntity, nullValue, negatedCurrentAnalysisData, equals, inferInCurrentAnalysisData, inferInNegatedCurrentAnalysisData, ref predicateValueKind);
                    }

                    return true;
                }

                return false;
            }

            private void SetValueFromPredicate(
                AnalysisEntity key,
                NullAbstractValue value,
                NullAnalysisData negatedCurrentAnalysisData,
                bool equals,
                bool inferInCurrentAnalysisData,
                bool inferInNegatedCurrentAnalysisData,
                ref PredicateValueKind predicateValueKind)
            {
                var negatedValue = NegatePredicateValue(value);
                if (CurrentAnalysisData.TryGetValue(key, out NullAbstractValue existingValue) &&
                    IsValidValueForPredicateAnalysis(existingValue) &&
                    (existingValue == NullAbstractValue.Null || value == NullAbstractValue.Null))
                {
                    if (value == existingValue && equals ||
                        negatedValue == existingValue && !equals)
                    {
                        predicateValueKind = PredicateValueKind.AlwaysTrue;
                        negatedValue = NullAbstractValue.Invalid;
                        inferInCurrentAnalysisData = false;
                    }

                    if (negatedValue == existingValue && equals ||
                        value == existingValue && !equals)
                    {
                        predicateValueKind = PredicateValueKind.AlwaysFalse;
                        value = NullAbstractValue.Invalid;
                        inferInNegatedCurrentAnalysisData = false;
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
                    SetAbstractValue(CurrentAnalysisData, key, value);
                }

                if (inferInNegatedCurrentAnalysisData)
                {
                    // Set negated value for the NegatedCurrentAnalysisData.
                    SetAbstractValue(negatedCurrentAnalysisData, key, negatedValue);
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
            protected override NullAnalysisData MergeAnalysisData(NullAnalysisData value1, NullAnalysisData value2)
                => NullAnalysisDomainInstance.Merge(value1, value2);
            protected override NullAnalysisData GetClonedAnalysisData(NullAnalysisData analysisData)
                => GetClonedAnalysisDataHelper(analysisData);
            protected override bool Equals(NullAnalysisData value1, NullAnalysisData value2)
                => EqualsHelper(value1, value2);


            #endregion

            #region Visitor methods
            public override NullAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var value = base.DefaultVisit(operation, argument);
                if (operation.ConstantValue.HasValue)
                {
                    return operation.ConstantValue.Value == null ?
                        NullAbstractValue.Null :
                        NullAbstractValue.NotNull;
                }
                else if (operation.Type != null && operation.Type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                return value;
            }

            protected override NullAbstractValue VisitAssignmentOperation(IAssignmentOperation operation, object argument)
            {
                var value = base.VisitAssignmentOperation(operation, argument);

                if (operation.Target.Type?.IsValueType == true)
                {
                    return NullAbstractValue.NotNull;
                }

                return value;
            }

            public override NullAbstractValue VisitCoalesce(ICoalesceOperation operation, object argument)
            {
                var leftValue = Visit(operation.Value, argument);
                var rightValue = Visit(operation.WhenNull, argument);

                if (operation.Type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                if (rightValue == NullAbstractValue.NotNull)
                {
                    return NullAbstractValue.NotNull;
                }

                switch (leftValue)
                {
                    case NullAbstractValue.NotNull:
                        return NullAbstractValue.NotNull;

                    case NullAbstractValue.Null:
                        return rightValue;

                    default:
                        return NullAbstractValue.MaybeNull;
                }
            }

            public override NullAbstractValue VisitConditionalAccess(IConditionalAccessOperation operation, object argument)
            {
                var leftValue = Visit(operation.Operation, argument);
                var rightValue = Visit(operation.WhenNotNull, argument);

                if (operation.Type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                switch (leftValue)
                {
                    case NullAbstractValue.Null:
                        return GetAbstractDefaultValue(operation.WhenNotNull.Type);

                    case NullAbstractValue.NotNull:
                        return rightValue;

                    default:
                        return NullAbstractValue.MaybeNull;
                }
            }

            public override NullAbstractValue VisitAddressOf(IAddressOfOperation operation, object argument)
            {
                return Visit(operation.Reference, argument) == NullAbstractValue.Null ? NullAbstractValue.Null : NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDeclarationExpression(IDeclarationExpressionOperation operation, object argument)
            {
                var _ = base.VisitDeclarationExpression(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitAwait(IAwaitOperation operation, object argument)
            {
                var _ = base.VisitAwait(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitNameOf(INameOfOperation operation, object argument)
            {
                var _ = base.VisitNameOf(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitIsType(IIsTypeOperation operation, object argument)
            {
                var _ = base.VisitIsType(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitInstanceReference(IInstanceReferenceOperation operation, object argument)
            {
                var _ = base.VisitInstanceReference(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
            {
                var _ = base.VisitObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, object argument)
            {
                var _ = base.VisitAnonymousObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitArrayCreation(IArrayCreationOperation operation, object argument)
            {
                var _ = base.VisitArrayCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitAnonymousFunction(IAnonymousFunctionOperation operation, object argument)
            {
                var _ = base.VisitAnonymousFunction(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
            {
                var _ = base.VisitDelegateCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, object argument)
            {
                var _ = base.VisitDynamicObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, object argument)
            {
                var _ = base.VisitTypeParameterObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitCollectionElementInitializer(ICollectionElementInitializerOperation operation, object argument)
            {
                var _ = base.VisitCollectionElementInitializer(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
            {
                var _ = base.VisitArrayInitializer(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitIsPattern(IIsPatternOperation operation, object argument)
            {
                var _ = base.VisitIsPattern(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDeclarationPattern(IDeclarationPatternOperation operation, object argument)
            {
                var _ = base.VisitDeclarationPattern(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, object argument)
            {
                var _ = base.VisitObjectOrCollectionInitializer(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                var _ = base.VisitInterpolatedString(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitSizeOf(ISizeOfOperation operation, object argument)
            {
                var _ = base.VisitSizeOf(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTypeOf(ITypeOfOperation operation, object argument)
            {
                var _ = base.VisitTypeOf(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitThrowCore(IThrowOperation operation, object argument)
            {
                var _ = base.VisitThrowCore(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTuple(ITupleOperation operation, object argument)
            {
                var _ = base.VisitTuple(operation, argument);
                return NullAbstractValue.NotNull;
            }

            protected override NullAbstractValue VisitReturnCore(IReturnOperation operation, object argument)
            {
                var _ = base.VisitReturnCore(operation, argument);
                return NullAbstractValue.NotNull;
            }

            private NullAbstractValue GetValueBasedOnInstanceOrReferenceValue(IOperation referenceOrInstance, ITypeSymbol operationType, NullAbstractValue defaultValue)
            {
                if (operationType != null && operationType.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                NullAbstractValue referenceOrInstanceValue = referenceOrInstance != null ? GetCachedAbstractValue(referenceOrInstance) : NullAbstractValue.NotNull;
                return referenceOrInstanceValue == NullAbstractValue.Null ? NullAbstractValue.Null : defaultValue;
            }

            public override NullAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var value = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation.Type, value);
            }

            public override NullAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                var value = base.VisitFieldReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation.Type, value);
            }

            public override NullAbstractValue VisitPropertyReference(IPropertyReferenceOperation operation, object argument)
            {
                var value = base.VisitPropertyReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation.Type, value);
            }

            public override NullAbstractValue VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, object argument)
            {
                var value = base.VisitDynamicMemberReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation.Type, value);
            }

            public override NullAbstractValue VisitMethodReference(IMethodReferenceOperation operation, object argument)
            {
                var value = base.VisitMethodReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation.Type, value);
            }

            public override NullAbstractValue VisitEventReference(IEventReferenceOperation operation, object argument)
            {
                var value = base.VisitEventReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance, operation.Type, value);
            }

            public override NullAbstractValue VisitArrayElementReference(IArrayElementReferenceOperation operation, object argument)
            {
                var value = base.VisitArrayElementReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.ArrayReference, operation.Type, value);
            }

            public override NullAbstractValue VisitDynamicInvocation(IDynamicInvocationOperation operation, object argument)
            {
                var value = base.VisitDynamicInvocation(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation, operation.Type, value);
            }

            public override NullAbstractValue VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, object argument)
            {
                var value = base.VisitDynamicIndexerAccess(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation, operation.Type, value);
            }

            #endregion
        }
    }
}
