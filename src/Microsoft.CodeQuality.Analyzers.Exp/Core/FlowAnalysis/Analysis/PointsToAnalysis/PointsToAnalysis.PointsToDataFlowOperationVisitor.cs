// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using Microsoft.CodeAnalysis.Operations.ControlFlow;
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
                DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt)
                : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis: false, nullAnalysisResultOpt: nullAnalysisResultOpt, copyAnalysisResultOpt: null, pointsToAnalysisResultOpt: null)
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

            protected override IEnumerable<AnalysisEntity> TrackedEntities => CurrentAnalysisData.Keys;

            protected override bool IsPointsToAnalysis => true;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override PointsToAbstractValue GetAbstractValue(AnalysisEntity analysisEntity)
            {
                if (!analysisEntity.Type.IsReferenceType)
                {
                    return PointsToAbstractValue.NoLocation;
                }

                if (!CurrentAnalysisData.TryGetValue(analysisEntity, out var value))
                {
                    value = _defaultPointsToValueGenerator.GetOrCreateDefaultValue(analysisEntity);
                    CurrentAnalysisData.Add(analysisEntity, value);
                }

                return value;
            }

            protected override PointsToAbstractValue GetPointsToAbstractValue(IOperation operation) => base.GetCachedAbstractValue(operation);
            
            protected override PointsToAbstractValue GetAbstractDefaultValue(ITypeSymbol type) => type != null && !type.IsReferenceType ? PointsToAbstractValue.NoLocation : PointsToAbstractValue.NullLocation;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, PointsToAbstractValue value)
            {
                if (analysisEntity.Type.IsReferenceType)
                {
                    CurrentAnalysisData[analysisEntity] = value;
                }
            }

            protected override void SetValueForParameterOnEntry(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                // Create a dummy PointsTo value for each reference type parameter.
                if (parameter.Type.IsReferenceType)
                {
                    var value = new PointsToAbstractValue(AbstractLocation.CreateSymbolLocation(parameter));
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
                if (operation.Type != null &&
                    operation.Type.IsReferenceType &&
                    AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity analysisEntity))
                {
                    return GetAbstractValue(analysisEntity);
                }
                else
                {
                    Debug.Assert(operation.Type == null || !operation.Type.IsValueType || defaultValue == PointsToAbstractValue.NoLocation);
                    return defaultValue;
                }
            }

            protected override PointsToAbstractValue ComputeAnalysisValueForOutArgument(AnalysisEntity analysisEntity, IArgumentOperation operation, PointsToAbstractValue defaultValue)
            {
                if (!analysisEntity.Type.IsReferenceType)
                {
                    return PointsToAbstractValue.NoLocation;
                }

                var location = AbstractLocation.CreateAllocationLocation(operation, analysisEntity.Type);
                return new PointsToAbstractValue(location);
            }

            // TODO: Remove these temporary methods once we move to compiler's CFG
            // https://github.com/dotnet/roslyn-analyzers/issues/1567
            #region Temporary methods to workaround lack of *real* CFG
            protected override PointsToAnalysisData MergeAnalysisData(PointsToAnalysisData value1, PointsToAnalysisData value2)
                => _pointsToAnalysisDomain.Merge(value1, value2);
            protected override PointsToAnalysisData MergeAnalysisDataForBackEdge(PointsToAnalysisData value1, PointsToAnalysisData value2)
                => _pointsToAnalysisDomain.MergeAnalysisDataForBackEdge(value1, value2);
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
                if (GetNullAbstractValue(operation) == NullAnalysis.NullAbstractValue.Null &&
                    (operation.Type == null || operation.Type.IsReferenceType))
                {
                    return PointsToAbstractValue.NullLocation;
                }
                else if (operation.ConstantValue.HasValue ||
                    (operation.Type != null && !operation.Type.IsReferenceType))
                {
                    return PointsToAbstractValue.NoLocation;
                }

                return ValueDomain.UnknownOrMayBeValue;
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

                // We should have created a new PointsTo value for the associated creation operation.
                return GetCachedAbstractValue(operation.Parent);
            }

            public override PointsToAbstractValue VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
            {
                var _ = base.VisitArrayInitializer(operation, argument);

                // We should have created a new PointsTo value for the associated array creation operation.
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

            private static PointsToAbstractValue VisitInvocationCommon(IOperation operation)
            {
                if (operation.Type.IsReferenceType)
                {
                    AbstractLocation location = AbstractLocation.CreateAllocationLocation(operation, operation.Type);
                    return new PointsToAbstractValue(location);
                }
                else
                {
                    return PointsToAbstractValue.NoLocation;
                }
            }

            public override PointsToAbstractValue VisitInvocation_LambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var _ = base.VisitInvocation_LambdaOrDelegateOrLocalFunction(operation, argument);
                return VisitInvocationCommon(operation);
            }

            public override PointsToAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                var _ = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
                return VisitInvocationCommon(operation);
            }

            public override PointsToAbstractValue VisitDynamicInvocation(IDynamicInvocationOperation operation, object argument)
            {
                var _ = base.VisitDynamicInvocation(operation, argument);
                return VisitInvocationCommon(operation);
            }

            #endregion
        }
    }
}
