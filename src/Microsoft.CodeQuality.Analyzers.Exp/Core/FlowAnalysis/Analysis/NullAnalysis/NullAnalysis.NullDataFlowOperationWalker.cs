// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis
{
    using NullAnalysisData = IDictionary<ISymbol, NullAbstractValue>;

    internal partial class NullAnalysis : ForwardDataFlowAnalysis<NullAnalysisData, NullBlockAnalysisResult, NullAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the null values across a given statement in a basic block.
        /// </summary>
        private sealed class NullDataFlowOperationWalker : DataFlowOperationWalker<NullAnalysisData, NullAbstractValue>
        {
            public NullDataFlowOperationWalker(AbstractDomain<NullAbstractValue> valueDomain) : base(valueDomain, nullAnalysisResultOpt: null)
            {
            }

            protected override NullAbstractValue UninitializedValue => NullAbstractValue.Undefined;
            protected override NullAbstractValue DefaultValue => NullAbstractValue.MaybeNull;

            protected override void SetAbstractValue(ISymbol symbol, NullAbstractValue value) =>
                CurrentAnalysisData[symbol] = value;

            protected override NullAbstractValue GetAbstractValue(ISymbol symbol) =>
                CurrentAnalysisData.TryGetValue(symbol, out var value) ? value : DefaultValue;

            protected override NullAbstractValue GetAbstractDefaultValue(ITypeSymbol type)
            {
                if (type.IsReferenceType)
                {
                    return NullAbstractValue.Null;
                }
                else if (type.IsValueType)
                {
                    return NullAbstractValue.NotNull;
                }
                else
                {
                    return DefaultValue;
                }
            }

            protected override void ResetCurrentAnalysisData(NullAnalysisData newAnalysisDataOpt = null) =>
                CurrentAnalysisData.Reset(newAnalysisDataOpt);

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

            public override NullAbstractValue VisitDeclarationExpression(IDeclarationExpressionOperation operation, object argument)
            {
                var unused = base.VisitDeclarationExpression(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitAwait(IAwaitOperation operation, object argument)
            {
                var unused = base.VisitDeclarationExpression(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitNameOf(INameOfOperation operation, object argument)
            {
                var unused = base.VisitNameOf(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitIsType(IIsTypeOperation operation, object argument)
            {
                var unused = base.VisitIsType(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitInstanceReference(IInstanceReferenceOperation operation, object argument)
            {
                var unused = base.VisitInstanceReference(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
            {
                var unused = base.VisitObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, object argument)
            {
                var unused = base.VisitAnonymousObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitArrayCreation(IArrayCreationOperation operation, object argument)
            {
                var unused = base.VisitArrayCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitAnonymousFunction(IAnonymousFunctionOperation operation, object argument)
            {
                var unused = base.VisitAnonymousFunction(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
            {
                var unused = base.VisitDelegateCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, object argument)
            {
                var unused = base.VisitDynamicObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, object argument)
            {
                var unused = base.VisitTypeParameterObjectCreation(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitCollectionElementInitializer(ICollectionElementInitializerOperation operation, object argument)
            {
                var unused = base.VisitCollectionElementInitializer(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
            {
                var unused = base.VisitArrayInitializer(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitIsPattern(IIsPatternOperation operation, object argument)
            {
                var unused = base.VisitIsPattern(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitDeclarationPattern(IDeclarationPatternOperation operation, object argument)
            {
                var unused = base.VisitDeclarationPattern(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, object argument)
            {
                var unused = base.VisitObjectOrCollectionInitializer(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                var unused = base.VisitInterpolatedString(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitSizeOf(ISizeOfOperation operation, object argument)
            {
                var unused = base.VisitSizeOf(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTypeOf(ITypeOfOperation operation, object argument)
            {
                var unused = base.VisitTypeOf(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitThrow(IThrowOperation operation, object argument)
            {
                var unused = base.VisitThrow(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitTuple(ITupleOperation operation, object argument)
            {
                var unused = base.VisitTuple(operation, argument);
                return NullAbstractValue.NotNull;
            }

            public override NullAbstractValue VisitReturn(IReturnOperation operation, object argument)
            {
                var unused = base.VisitReturn(operation, argument);
                return NullAbstractValue.NotNull;
            }

            private NullAbstractValue GetValueBasedOnInstanceOrReferenceValue(IOperation referenceOrInstance)
            {
                var referenceOrInstanceValue = GetState(referenceOrInstance);
                return referenceOrInstanceValue == NullAbstractValue.Null ? NullAbstractValue.Null : NullAbstractValue.MaybeNull;
            }

            public override NullAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var unused = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                var unused = base.VisitFieldReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitPropertyReference(IPropertyReferenceOperation operation, object argument)
            {
                var unused = base.VisitPropertyReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, object argument)
            {
                var unused = base.VisitDynamicMemberReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitMethodReference(IMethodReferenceOperation operation, object argument)
            {
                var unused = base.VisitMethodReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitEventReference(IEventReferenceOperation operation, object argument)
            {
                var unused = base.VisitEventReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Instance);
            }

            public override NullAbstractValue VisitArrayElementReference(IArrayElementReferenceOperation operation, object argument)
            {
                var unused = base.VisitArrayElementReference(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.ArrayReference);
            }

            public override NullAbstractValue VisitDynamicInvocation(IDynamicInvocationOperation operation, object argument)
            {
                var unused = base.VisitDynamicInvocation(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation);
            }

            public override NullAbstractValue VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, object argument)
            {
                var unused = base.VisitDynamicIndexerAccess(operation, argument);
                return GetValueBasedOnInstanceOrReferenceValue(operation.Operation);
            }

            public override NullAbstractValue VisitAddressOf(IAddressOfOperation operation, object argument)
            {
                return Visit(operation.Reference, argument) == NullAbstractValue.Null ? NullAbstractValue.Null : NullAbstractValue.NotNull;
            }

            #endregion
        }
    }
}
