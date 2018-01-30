// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using Microsoft.CodeAnalysis.Operations.ControlFlow;
    using PointsToAnalysisData = IDictionary<AnalysisEntity, PointsToAbstractValue>;

    internal partial class PointsToAnalysis : ForwardDataFlowAnalysis<PointsToAnalysisData, PointsToBlockAnalysisResult, PointsToAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the points to values across a given statement in a basic block.
        /// </summary>
        private sealed class PointsToDataFlowOperationVisitor : DataFlowOperationVisitor<PointsToAnalysisData, PointsToAbstractValue>
        {
            public PointsToDataFlowOperationVisitor(
                AbstractDomain<PointsToAbstractValue> valueDomain,
                INamedTypeSymbol containingTypeSymbol,
                DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt)
                : base(valueDomain, containingTypeSymbol, nullAnalysisResultOpt: nullAnalysisResultOpt, pointsToAnalysisResultOpt: null)
            {
            }

            protected override PointsToAbstractValue UnknownOrMayBeValue => PointsToAbstractValue.Unknown;
            protected override bool HasPointsToAnalysisResult => true;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) =>
                CurrentAnalysisData.ContainsKey(analysisEntity);

            public override PointsToAnalysisData Flow(IOperation statement, BasicBlock block, PointsToAnalysisData input)
            {
                if (input != null)
                {
                    // Always set the points to value for the "this" or "Me" instance.
                    input[AnalysisEntityFactory.ThisOrMeInstance] = ThisOrMePointsToAbstractValue;
                }

                return base.Flow(statement, block, input);
            }

            protected override PointsToAbstractValue GetAbstractValue(AnalysisEntity analysisEntity)
            {
                if (analysisEntity.Type.HasValueCopySemantics())
                {
                    return PointsToAbstractValue.NoLocation;
                }

                if (!CurrentAnalysisData.TryGetValue(analysisEntity, out var value))
                {
                    value = analysisEntity.SymbolOpt?.Kind == SymbolKind.Local ?
                        PointsToAbstractValue.Undefined :
                        UnknownOrMayBeValue;
                }

                return value;
            }

            protected override PointsToAbstractValue GetPointsToAbstractValue(IOperation operation)
            {
                return base.GetCachedAbstractValue(operation);
            }

            protected override PointsToAbstractValue GetAbstractDefaultValue(ITypeSymbol type)
                => PointsToAbstractValue.NoLocation;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, PointsToAbstractValue value)
            {
                if (!analysisEntity.Type.HasValueCopySemantics())
                {
                    CurrentAnalysisData[analysisEntity] = value;
                }
            }

            protected override void ResetCurrentAnalysisData(PointsToAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

            protected override PointsToAbstractValue ComputeAnalysisValueForReferenceOperation(IOperation operation, PointsToAbstractValue defaultValue)
            {
                if (!operation.Type.HasValueCopySemantics() &&
                    AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity analysisEntity))
                {
                    if (!HasAbstractValue(analysisEntity))
                    {
                        var value = new PointsToAbstractValue(AbstractLocation.CreateAllocationLocation(operation, operation.Type));
                        SetAbstractValue(analysisEntity, value);
                        return value;
                    }

                    return GetAbstractValue(analysisEntity);
                }
                else
                {
                    Debug.Assert(!operation.Type.HasValueCopySemantics() || defaultValue == PointsToAbstractValue.NoLocation);
                    return defaultValue;
                }
            }

            #region Visitor methods

            public override PointsToAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var value = base.DefaultVisit(operation, argument);

                // Constants, operations with NullAbstractValue.Null and operations with value copy semantics (value type and strings)
                // do not point to any location.
                if (operation.ConstantValue.HasValue ||
                    GetNullAbstractValue(operation) == NullAnalysis.NullAbstractValue.Null ||
                    (operation.Type != null && operation.Type.HasValueCopySemantics()))
                {
                    return PointsToAbstractValue.NoLocation;
                }

                return UnknownOrMayBeValue;
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
                IOperation currentInstanceOperation = operation.GetInstance();
                return currentInstanceOperation != null ?
                    GetCachedAbstractValue(currentInstanceOperation) :
                    ThisOrMePointsToAbstractValue;
            }

            private PointsToAbstractValue VisitTypeCreationWithArgumentsAndInitializer(IEnumerable<IOperation> arguments, IObjectOrCollectionInitializerOperation initializer, IOperation operation, object argument)
            {
                AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                var pointsToAbstractValue = new PointsToAbstractValue(location);
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
                var pointsToAbstractValue = new PointsToAbstractValue(location);
                CacheAbstractValue(operation, pointsToAbstractValue);

                var _ = VisitArray(operation.Initializers, argument);
                return pointsToAbstractValue;
            }

            public override PointsToAbstractValue VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
            {
                var _ = base.VisitDelegateCreation(operation, argument);
                AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                return new PointsToAbstractValue(location);
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

                    PointsToAbstractValue memberInstanceLocation = new PointsToAbstractValue(AbstractLocation.CreateAllocationLocation(operation, memberReference.Type));
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

                // We should have created and created a new points to value for the associated creation operation.
                return GetCachedAbstractValue(operation.Parent);
            }

            public override PointsToAbstractValue VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
            {
                var _ = base.VisitArrayInitializer(operation, argument);

                // We should have created and created a new points to value for the associated array creation operation.
                return GetCachedAbstractValue((IArrayCreationOperation)operation.Parent);
            }

            public override PointsToAbstractValue VisitArrayCreation(IArrayCreationOperation operation, object argument)
            {
                var pointsToAbstractValue = new PointsToAbstractValue(AbstractLocation.CreateAllocationLocation(operation, operation.Type));
                CacheAbstractValue(operation, pointsToAbstractValue);

                var unusedDimensionsValue = VisitArray(operation.DimensionSizes, argument);
                var initializerValue = Visit(operation.Initializer, argument);
                Debug.Assert(operation.Initializer == null || initializerValue == pointsToAbstractValue);
                return pointsToAbstractValue;
            }

            public override PointsToAbstractValue VisitParameterReference(IParameterReferenceOperation operation, object argument)
            {
                // Create a dummy points to value for each reference type parameter.
                if (!operation.Type.HasValueCopySemantics())
                {
                    var result = AnalysisEntityFactory.TryCreateForSymbolDeclaration(operation.Parameter, out AnalysisEntity analysisEntity);
                    Debug.Assert(result);
                    if (!HasAbstractValue(analysisEntity))
                    {
                        var value = new PointsToAbstractValue(AbstractLocation.CreateAllocationLocation(operation, operation.Parameter.Type));
                        SetAbstractValue(analysisEntity, value);
                        return value;
                    }

                    return GetAbstractValue(analysisEntity);
                }
                else
                {
                    return PointsToAbstractValue.NoLocation;
                }
            }

            public override PointsToAbstractValue VisitIsPattern(IIsPatternOperation operation, object argument)
            {
                // TODO: Handle patterns
                return base.VisitIsPattern(operation, argument);
            }

            public override PointsToAbstractValue VisitDeclarationPattern(IDeclarationPatternOperation operation, object argument)
            {
                // TODO: Handle patterns
                return base.VisitDeclarationPattern(operation, argument);
            }

            public override PointsToAbstractValue VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                var _ = base.VisitInterpolatedString(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitBinaryOperator(IBinaryOperation operation, object argument)
            {
                var _ = base.VisitBinaryOperator(operation, argument);
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

            public override PointsToAbstractValue VisitThrow(IThrowOperation operation, object argument)
            {
                var _ = base.VisitThrow(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            public override PointsToAbstractValue VisitTuple(ITupleOperation operation, object argument)
            {
                // TODO: Handle tuples.
                return base.VisitTuple(operation, argument);
            }

            public override PointsToAbstractValue VisitReturn(IReturnOperation operation, object argument)
            {
                var _ = base.VisitReturn(operation, argument);
                return PointsToAbstractValue.NoLocation;
            }

            #endregion
        }
    }
}
