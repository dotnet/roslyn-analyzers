// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    using NullAnalysisData = IDictionary<AnalysisEntity, NullAbstractValue>;

    internal partial class NullAnalysis : ForwardDataFlowAnalysis<NullAnalysisData, NullBlockAnalysisResult, NullAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the null values across a given statement in a basic block.
        /// </summary>
        private sealed class NullDataFlowOperationVisitor : DataFlowOperationVisitor<NullAnalysisData, NullAbstractValue>
        {
            public NullDataFlowOperationVisitor(
                AbstractDomain<NullAbstractValue> valueDomain,
                INamedTypeSymbol containingTypeSymbol,
                DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt)
                : base(valueDomain, containingTypeSymbol, nullAnalysisResultOpt: null, pointsToAnalysisResultOpt: pointsToAnalysisResultOpt)
            {
            }

            protected override NullAbstractValue UnknownOrMayBeValue => NullAbstractValue.MaybeNull;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, NullAbstractValue value) =>
                CurrentAnalysisData[analysisEntity] = value;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) =>
                CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override NullAbstractValue GetAbstractValue(AnalysisEntity analysisEntity) =>
                CurrentAnalysisData.TryGetValue(analysisEntity, out var value) ? value : UnknownOrMayBeValue;

            protected override NullAbstractValue GetAbstractDefaultValue(ITypeSymbol type)
            {
                if (type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                return NullAbstractValue.Null;
            }

            protected override void ResetCurrentAnalysisData(NullAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

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

                if (operation.Target.Type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }

                return value;
            }

            public override NullAbstractValue VisitCoalesce(ICoalesceOperation operation, object argument)
            {
                var leftValue = Visit(operation.Value, argument);
                var rightValue = Visit(operation.WhenNull, argument);
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

            public override NullAbstractValue VisitThrow(IThrowOperation operation, object argument)
            {
                var _ = base.VisitThrow(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTuple(ITupleOperation operation, object argument)
            {
                var _ = base.VisitTuple(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitReturn(IReturnOperation operation, object argument)
            {
                var _ = base.VisitReturn(operation, argument);
                return NullAbstractValue.NotNull;
            }

            private NullAbstractValue GetValueBasedOnInstanceOrReferenceValue(IOperation referenceOrInstance)
            {
                NullAbstractValue referenceOrInstanceValue = referenceOrInstance != null ? GetCachedAbstractValue(referenceOrInstance) : NullAbstractValue.NotNull;
                return referenceOrInstanceValue == NullAbstractValue.Null ? NullAbstractValue.Null : NullAbstractValue.MaybeNull;
            }

            public override NullAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var _ = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                var _ = base.VisitFieldReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitPropertyReference(IPropertyReferenceOperation operation, object argument)
            {
                var _ = base.VisitPropertyReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, object argument)
            {
                var _ = base.VisitDynamicMemberReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitMethodReference(IMethodReferenceOperation operation, object argument)
            {
                var _ = base.VisitMethodReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitEventReference(IEventReferenceOperation operation, object argument)
            {
                var _ = base.VisitEventReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitArrayElementReference(IArrayElementReferenceOperation operation, object argument)
            {
                var _ = base.VisitArrayElementReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.ArrayReference);
            }

            public override NullAbstractValue VisitDynamicInvocation(IDynamicInvocationOperation operation, object argument)
            {
                var _ = base.VisitDynamicInvocation(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation);
            }

            public override NullAbstractValue VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, object argument)
            {
                var _ = base.VisitDynamicIndexerAccess(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation);
            }

            #endregion
        }
    }
}
