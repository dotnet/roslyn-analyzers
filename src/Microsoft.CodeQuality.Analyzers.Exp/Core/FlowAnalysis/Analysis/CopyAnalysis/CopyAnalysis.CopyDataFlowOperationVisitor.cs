// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities.Extensions;

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
                DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt)
                : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis: true, copyAnalysisResultOpt: null, pointsToAnalysisResultOpt: pointsToAnalysisResultOpt)
            {
            }

            protected override void AddTrackedEntities(ImmutableArray<AnalysisEntity>.Builder builder) => builder.AddRange(CurrentAnalysisData.Keys);

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override CopyAbstractValue GetAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.TryGetValue(analysisEntity, out var value) ? value : CopyAbstractValue.Unknown;

            protected override CopyAbstractValue GetCopyAbstractValue(IOperation operation) => base.GetCachedAbstractValue(operation);
            
            protected override CopyAbstractValue GetAbstractDefaultValue(ITypeSymbol type) => CopyAbstractValue.NotApplicable;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, CopyAbstractValue value)
            {
                Debug.Assert(analysisEntity != null);
                Debug.Assert(value != null);

                SetAbstractValue(CurrentAnalysisData, analysisEntity, value, fromPredicate: false);

                if (IsCurrentlyPerformingPredicateAnalysis)
                {
                    SetAbstractValue(NegatedCurrentAnalysisDataStack.Peek(), analysisEntity, value, fromPredicate: false);
                }
            }

            private static void SetAbstractValue(CopyAnalysisData copyAnalysisData, AnalysisEntity analysisEntity, CopyAbstractValue value, bool fromPredicate)
            {
                AssertValidCopyAnalysisData(copyAnalysisData);

                // Don't track entities if do not know about it's instance location.
                if (analysisEntity.HasUnknownInstanceLocation)
                {
                    return;
                }

                if (value.AnalysisEntities.Count > 0)
                {
                    if (copyAnalysisData.TryGetValue(value.AnalysisEntities.First(), out var fixedUpValue))
                    {
                        value = fixedUpValue;
                    }

                    var validEntities = value.AnalysisEntities.Where(entity => !entity.HasUnknownInstanceLocation).ToImmutableHashSet();
                    if (validEntities.Count < value.AnalysisEntities.Count)
                    {
                        value = validEntities.Count > 0 ? new CopyAbstractValue(validEntities) : CopyAbstractValue.Unknown;
                    }
                }

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
                            Debug.Assert(newValueForEntitiesInOldSet.AnalysisEntities.Contains(entityToUpdate));
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
                    Debug.Assert(newValue.AnalysisEntities.Count > 0);
                    Debug.Assert(newValue.AnalysisEntities.Contains(entityToUpdate));
                    copyAnalysisData[entityToUpdate] = newValue;
                }

                AssertValidCopyAnalysisData(copyAnalysisData);
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
                SetAbstractValue(analysisEntity, ValueDomain.UnknownOrMayBeValue);
                return GetAbstractValue(analysisEntity);
            }

            #region Predicate analysis
            protected override PredicateValueKind SetValueForEqualsOrNotEqualsComparisonOperator(
                IOperation leftOperand,
                IOperation rightOperand,
                CopyAnalysisData negatedCurrentAnalysisData,
                bool equals,
                bool isReferenceEquality)
            {
                if (GetCopyAbstractValue(leftOperand).Kind != CopyAbstractValueKind.Unknown &&
                    GetCopyAbstractValue(rightOperand).Kind != CopyAbstractValueKind.Unknown &&
                    AnalysisEntityFactory.TryCreate(leftOperand, out AnalysisEntity leftEntity) &&
                    AnalysisEntityFactory.TryCreate(rightOperand, out AnalysisEntity rightEntity))
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
                        // NOTE: CopyAnalysis only tracks value equal entities
                        if (!isReferenceEquality)
                        {
                            predicateKind = equals ? PredicateValueKind.AlwaysTrue : PredicateValueKind.AlwaysFalse;
                        }
                    }
                    else if (negatedCurrentAnalysisData.TryGetValue(rightEntity, out var negatedRightValue) &&
                        negatedRightValue.AnalysisEntities.Contains(leftEntity))
                    {
                        // We have "a == b || a == b" or "a == b || a != b"
                        // For both cases, condition on right is always true or always false and redundant.
                        // NOTE: CopyAnalysis only tracks value equal entities
                        if (!isReferenceEquality)
                        {
                            predicateKind = equals ? PredicateValueKind.AlwaysFalse : PredicateValueKind.AlwaysTrue;
                        }
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

            public override CopyAbstractValue VisitConversion(IConversionOperation operation, object argument)
            {
                var operandValue = Visit(operation.Operand, argument);

                if (TryInferConversion(operation, out bool alwaysSucceed, out bool alwaysFail))
                {
                    Debug.Assert(!alwaysSucceed || !alwaysFail);
                    
                    // Flow the copy value of the operand to the converted operation if conversion may succeed.
                    if (!alwaysFail)
                    {
                        // For try cast, also ensure conversion always succeeds before flowing copy value.
                        // TODO: For direct cast, we should check if conversion is implicit.
                        // For now, we only flow values for reference type direct cast conversions.
                        if (operation.IsTryCast && alwaysSucceed ||
                            !operation.IsTryCast && operation.Type.IsReferenceType)
                        {
                            return operandValue;
                        }
                    }
                }

                return CopyAbstractValue.Unknown;
            }
            #endregion
        }
    }
}
