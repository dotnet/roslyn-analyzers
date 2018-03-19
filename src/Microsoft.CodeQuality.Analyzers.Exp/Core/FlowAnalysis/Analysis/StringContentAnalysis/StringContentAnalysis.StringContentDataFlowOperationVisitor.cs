// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;

    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the string content values across a given statement in a basic block.
        /// </summary>
        private sealed class StringContentDataFlowOperationVisitor : AnalysisEntityDataFlowOperationVisitor<StringContentAnalysisData, StringContentAbstractValue>
        {
            public StringContentDataFlowOperationVisitor(
                StringContentAbstractValueDomain valueDomain,
                ISymbol owningSymbol,
                WellKnownTypeProvider wellKnownTypeProvider,
                bool pessimisticAnalysis,
                DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue> copyAnalysisResultOpt,
                DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt,
                DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt)
                : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis: true,
                      nullAnalysisResultOpt: nullAnalysisResultOpt, copyAnalysisResultOpt: copyAnalysisResultOpt, pointsToAnalysisResultOpt: pointsToAnalysisResultOpt)
            {
            }

            protected override IEnumerable<AnalysisEntity> TrackedEntities => CurrentAnalysisData.Keys;

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, StringContentAbstractValue value) => SetAbstractValue(CurrentAnalysisData, analysisEntity, value);

            private static void SetAbstractValue(StringContentAnalysisData analysisData, AnalysisEntity analysisEntity, StringContentAbstractValue value) => analysisData[analysisEntity] = value;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override StringContentAbstractValue GetAbstractValue(AnalysisEntity analysisEntity) => CurrentAnalysisData.TryGetValue(analysisEntity, out var value) ? value : ValueDomain.UnknownOrMayBeValue;

            protected override StringContentAbstractValue GetAbstractDefaultValue(ITypeSymbol type) => StringContentAbstractValue.DoesNotContainLiteralOrNonLiteralState;

            protected override void ResetCurrentAnalysisData(StringContentAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

            #region Predicate analysis
            protected override PredicateValueKind SetValueForEqualsOrNotEqualsComparisonOperator(IBinaryOperation operation, StringContentAnalysisData negatedCurrentAnalysisData, bool equals)
            {
                Debug.Assert(operation.IsComparisonOperator());

                var predicateValueKind = PredicateValueKind.Unknown;

                // Handle 'a == "SomeString"' and 'a != "SomeString"'
                SetValueForComparisonOperator(operation.LeftOperand, operation.RightOperand, negatedCurrentAnalysisData, equals, ref predicateValueKind);

                // Handle '"SomeString" == a' and '"SomeString" != a'
                SetValueForComparisonOperator(operation.RightOperand, operation.LeftOperand, negatedCurrentAnalysisData, equals, ref predicateValueKind);

                return predicateValueKind;
            }

            private void SetValueForComparisonOperator(IOperation target, IOperation assignedValue, StringContentAnalysisData negatedCurrentAnalysisData, bool equals, ref PredicateValueKind predicateValueKind)
            {
                var analysisData = equals ? CurrentAnalysisData : negatedCurrentAnalysisData;
                StringContentAbstractValue stringContentValue = GetCachedAbstractValue(assignedValue);
                if (stringContentValue.IsLiteralState &&
                    AnalysisEntityFactory.TryCreate(target, out AnalysisEntity targetEntity))
                {
                    if (analysisData.TryGetValue(targetEntity, out StringContentAbstractValue existingValue) &&
                        existingValue.IsLiteralState)
                    {
                        var newStringContentValue = stringContentValue.IntersectLiteralValues(existingValue);
                        if (newStringContentValue.NonLiteralState == StringContainsNonLiteralState.Invalid)
                        {
                            predicateValueKind = equals ? PredicateValueKind.AlwaysFalse : PredicateValueKind.AlwaysTrue;
                        }
                        else if (predicateValueKind != PredicateValueKind.AlwaysFalse &&
                            newStringContentValue.IsLiteralState &&
                            newStringContentValue.LiteralValues.Count == 1 &&
                            stringContentValue.LiteralValues.Count == 1 &&
                            existingValue.LiteralValues.Count == 1)
                        {
                            predicateValueKind = equals ? PredicateValueKind.AlwaysTrue : PredicateValueKind.AlwaysFalse;
                        }

                        stringContentValue = newStringContentValue;
                    }

                    CopyAbstractValue copyValue = GetCopyAbstractValue(target);
                    if (copyValue.Kind == CopyAbstractValueKind.Known)
                    {
                        Debug.Assert(copyValue.AnalysisEntities.Contains(targetEntity));
                        foreach (var analysisEntity in copyValue.AnalysisEntities)
                        {
                            SetAbstractValue(analysisData, analysisEntity, stringContentValue);
                        }
                    }
                    else
                    {
                        SetAbstractValue(analysisData, targetEntity, stringContentValue);
                    }                    
                }
            }

            #endregion

            // TODO: Remove these temporary methods once we move to compiler's CFG
            // https://github.com/dotnet/roslyn-analyzers/issues/1567
            #region Temporary methods to workaround lack of *real* CFG
            protected override StringContentAnalysisData MergeAnalysisData(StringContentAnalysisData value1, StringContentAnalysisData value2)
                => StringContentAnalysisDomainInstance.Merge(value1, value2);
            protected override StringContentAnalysisData GetClonedAnalysisData(StringContentAnalysisData analysisData)
                => GetClonedAnalysisDataHelper(analysisData);
            protected override bool Equals(StringContentAnalysisData value1, StringContentAnalysisData value2)
                => EqualsHelper(value1, value2);
            #endregion

            #region Visitor methods
            public override StringContentAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var _ = base.DefaultVisit(operation, argument);
                if (operation.Type == null)
                {
                    return StringContentAbstractValue.DoesNotContainLiteralOrNonLiteralState;
                }

                if (operation.Type.SpecialType == SpecialType.System_String)
                {
                    if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is string value)
                    {
                        return StringContentAbstractValue.Create(value);
                    }
                    else
                    {
                        return StringContentAbstractValue.MayBeContainsNonLiteralState;
                    }
                }

                return ValueDomain.UnknownOrMayBeValue;
            }

            public override StringContentAbstractValue VisitBinaryOperatorCore(IBinaryOperation operation, object argument)
            {
                switch (operation.OperatorKind)
                {
                    case BinaryOperatorKind.Add:
                    case BinaryOperatorKind.Concatenate:
                        var leftValue = Visit(operation.LeftOperand, argument);
                        var rightValue = Visit(operation.RightOperand, argument);
                        return leftValue.MergeBinaryAdd(rightValue);

                    default:
                        return base.VisitBinaryOperatorCore(operation, argument);
                }
            }

            public override StringContentAbstractValue VisitCompoundAssignment(ICompoundAssignmentOperation operation, object argument)
            {
                StringContentAbstractValue value;
                switch (operation.OperatorKind)
                {
                    case BinaryOperatorKind.Add:
                    case BinaryOperatorKind.Concatenate:
                        var leftValue = Visit(operation.Target, argument);
                        var rightValue = Visit(operation.Value, argument);
                        value = leftValue.MergeBinaryAdd(rightValue);
                        break;

                    default:
                        value = base.VisitCompoundAssignment(operation, argument);
                        break;
                }

                SetAbstractValueForAssignment(operation.Target, operation.Value, value);
                return value;
            }

            public override StringContentAbstractValue VisitNameOf(INameOfOperation operation, object argument)
            {
                var nameofValue = base.VisitNameOf(operation, argument);
                if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is string value)
                {
                    return StringContentAbstractValue.Create(value);
                }

                return nameofValue;
            }

            public override StringContentAbstractValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
            {
                // TODO: Analyze string constructor
                // https://github.com/dotnet/roslyn-analyzers/issues/1547
                return base.VisitObjectCreation(operation, argument);
            }

            public override StringContentAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                var value = base.VisitFieldReference(operation, argument);

                // Handle "string.Empty"
                if (operation.Field.Name.Equals("Empty", StringComparison.Ordinal) &&
                    operation.Field.ContainingType.SpecialType == SpecialType.System_String)
                {
                    return StringContentAbstractValue.Create(string.Empty);
                }

                return value;
            }

            public override StringContentAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                // TODO: Handle invocations of string methods (Format, SubString, Replace, Concat, etc.)
                // https://github.com/dotnet/roslyn-analyzers/issues/1547
                return base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
            }

            public override StringContentAbstractValue VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                if (operation.Parts.IsEmpty)
                {
                    return StringContentAbstractValue.Create(string.Empty);
                }

                StringContentAbstractValue mergedValue = Visit(operation.Parts[0], argument);
                for (int i = 1; i < operation.Parts.Length; i++)
                {
                    var newValue = Visit(operation.Parts[i], argument);
                    mergedValue = mergedValue.MergeBinaryAdd(newValue);
                }

                return mergedValue;
            }

            #endregion
        }
    }
}
