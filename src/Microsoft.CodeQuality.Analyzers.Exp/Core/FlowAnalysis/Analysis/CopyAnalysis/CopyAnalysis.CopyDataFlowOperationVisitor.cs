// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis
{
    using CopyAnalysisData = IDictionary<AnalysisEntity, CopyAbstractValue>;

    internal partial class CopyAnalysis : ForwardDataFlowAnalysis<CopyAnalysisData, CopyBlockAnalysisResult, CopyAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the copy values across a given statement in a basic block.
        /// </summary>
        private sealed class CopyDataFlowOperationVisitor : AnalysisEntityDataFlowOperationVisitor<CopyAnalysisData, CopyAbstractValue>
        {
            public CopyDataFlowOperationVisitor(
                CopyAbstractValueDomain valueDomain,
                ISymbol owningSymbol,
                WellKnownTypeProvider wellKnownTypeProvider,
                bool pessimisticAnalysis,
                DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt,
                DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt)
                : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis: true, nullAnalysisResultOpt: nullAnalysisResultOpt, copyAnalysisResultOpt: null, pointsToAnalysisResultOpt: pointsToAnalysisResultOpt)
            {
            }

            protected override IEnumerable<AnalysisEntity> TrackedEntities => CurrentAnalysisData.Keys;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override CopyAbstractValue GetAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.TryGetValue(analysisEntity, out var value) ? value : CopyAbstractValue.Unknown;

            protected override CopyAbstractValue GetCopyAbstractValue(IOperation operation) => base.GetCachedAbstractValue(operation);
            
            protected override CopyAbstractValue GetAbstractDefaultValue(ITypeSymbol type) => CopyAbstractValue.NotApplicable;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, CopyAbstractValue value)
            {
                SetAbstractValue(CurrentAnalysisData, analysisEntity, value, fromPredicate: false);
            }

            private static void SetAbstractValue(CopyAnalysisData copyAnalysisData, AnalysisEntity analysisEntity, CopyAbstractValue value, bool fromPredicate)
            {
                // Handle updating the existing value if not setting the value from predicate analysis.
                if (!fromPredicate &&
                    copyAnalysisData.TryGetValue(analysisEntity, out CopyAbstractValue existingValue))
                {
                    if (existingValue == value)
                    {
                        // Assigning the same value to the entity.
                        Debug.Assert(existingValue.AnalysisEntities.Contains(analysisEntity));
                        return;
                    }

                    if (existingValue.AnalysisEntities.Count > 1)
                    {
                        var newValueForEntitiesInOldSet = existingValue.WithEntityRemoved(analysisEntity);
                        foreach (var entityToUpdate in newValueForEntitiesInOldSet.AnalysisEntities)
                        {
                            Debug.Assert(copyAnalysisData[entityToUpdate] == existingValue);
                            copyAnalysisData[entityToUpdate] = newValueForEntitiesInOldSet;
                        }
                    }
                }

                // Handle setting the new value.
                var newAnalysisEntities = value.AnalysisEntities.Add(analysisEntity);
                if (fromPredicate)
                {
                    // Also include the existing values for the analysis entity.
                    if (copyAnalysisData.TryGetValue(analysisEntity, out existingValue))
                    {
                        if (existingValue.Kind == CopyAbstractValueKind.Invalid)
                        {
                            return;
                        }

                        newAnalysisEntities = newAnalysisEntities.Union(existingValue.AnalysisEntities);
                    }
                }

                var newValue = new CopyAbstractValue(newAnalysisEntities);
                foreach (var entityToUpdate in newAnalysisEntities)
                {
                    copyAnalysisData[entityToUpdate] = newValue;
                }
            }

            protected override void SetValueForParameterOnEntry(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                // Create a dummy copy value for each parameter.
                SetAbstractValue(analysisEntity, new CopyAbstractValue(analysisEntity));
            }

            protected override void SetValueForParameterOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                // Do not escape the copy value for parameter at exit.
            }

            protected override void ResetCurrentAnalysisData(CopyAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

            protected override CopyAbstractValue ComputeAnalysisValueForReferenceOperation(IOperation operation, CopyAbstractValue defaultValue)
            {
                if (AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity analysisEntity))
                {
                    return CurrentAnalysisData.TryGetValue(analysisEntity, out CopyAbstractValue value) ? value : new CopyAbstractValue(analysisEntity);
                }
                else
                {
                    return defaultValue;
                }
            }

            protected override CopyAbstractValue ComputeAnalysisValueForOutArgument(AnalysisEntity analysisEntity, IArgumentOperation operation, CopyAbstractValue defaultValue)
            {
                var value = new CopyAbstractValue(analysisEntity);
                SetAbstractValue(analysisEntity, value);
                return value;
            }

            #region Predicate analysis
            protected override PredicateValueKind SetValueForEqualsOrNotEqualsComparisonOperator(IBinaryOperation operation, CopyAnalysisData negatedCurrentAnalysisData, bool equals)
            {
                Debug.Assert(operation.IsComparisonOperator());

                if (AnalysisEntityFactory.TryCreate(operation.LeftOperand, out AnalysisEntity leftEntity) &&
                    AnalysisEntityFactory.TryCreate(operation.RightOperand, out AnalysisEntity rightEntity))
                {
                    var predicateKind = PredicateValueKind.Unknown;
                    if (!CurrentAnalysisData.TryGetValue(rightEntity, out CopyAbstractValue rightValue))
                    {
                        rightValue = new CopyAbstractValue(rightEntity);
                    }
                    else if (rightValue.AnalysisEntities.Contains(leftEntity))
                    {
                        // We have "a == b && a == b" or "a == b && a != b"
                        // For both cases, condition on right is always true or always false and redundant.
                        predicateKind = equals ? PredicateValueKind.AlwaysTrue : PredicateValueKind.AlwaysFalse;
                    }
                    else if (negatedCurrentAnalysisData.TryGetValue(rightEntity, out rightValue) &&
                        rightValue.AnalysisEntities.Contains(leftEntity))
                    {
                        // We have "a == b || a == b" or "a == b || a != b"
                        // For both cases, condition on right is always true or always false and redundant.
                        predicateKind = equals ? PredicateValueKind.AlwaysFalse : PredicateValueKind.AlwaysTrue;                        
                    }

                    if (predicateKind != PredicateValueKind.Unknown)
                    {
                        if (!equals)
                        {
                            // "a == b && a != b" or "a == b || a != b"
                            // CurrentAnalysisData and negatedCurrentAnalysisData are both unknown values.
                            foreach (var entity in rightValue.AnalysisEntities)
                            {
                                SetAbstractValue(CurrentAnalysisData, entity, CopyAbstractValue.Invalid, fromPredicate: true);
                                SetAbstractValue(negatedCurrentAnalysisData, entity, CopyAbstractValue.Invalid, fromPredicate: true);
                            }
                        }

                        return predicateKind;
                    }

                    var analysisData = equals ? CurrentAnalysisData : negatedCurrentAnalysisData;
                    SetAbstractValue(analysisData, leftEntity, rightValue, fromPredicate: true);
                }

                return PredicateValueKind.Unknown;
            }

            #endregion

            // TODO: Remove these temporary methods once we move to compiler's CFG
            // https://github.com/dotnet/roslyn-analyzers/issues/1567
            #region Temporary methods to workaround lack of *real* CFG
            protected override CopyAnalysisData MergeAnalysisData(CopyAnalysisData value1, CopyAnalysisData value2)
                => CopyAnalysisDomain.Instance.Merge(value1, value2);
            protected override CopyAnalysisData GetClonedAnalysisData(CopyAnalysisData analysisData)
                => GetClonedAnalysisDataHelper(analysisData);
            protected override bool Equals(CopyAnalysisData value1, CopyAnalysisData value2)
                => EqualsHelper(value1, value2);
            #endregion

            #region Visitor overrides
            public override CopyAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var _ = base.DefaultVisit(operation, argument);
                return CopyAbstractValue.Unknown;
            }
            #endregion
        }
    }
}
