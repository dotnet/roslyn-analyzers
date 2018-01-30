// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;

    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the string content values across a given statement in a basic block.
        /// </summary>
        private sealed class StringContentDataFlowOperationVisitor : DataFlowOperationVisitor<StringContentAnalysisData, StringContentAbstractValue>
        {
            public StringContentDataFlowOperationVisitor(
                AbstractDomain<StringContentAbstractValue> valueDomain,
                INamedTypeSymbol containingTypeSymbol,
                DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt,
                DataFlowAnalysisResult<PointsToAnalysis.PointsToBlockAnalysisResult, PointsToAnalysis.PointsToAbstractValue> pointsToAnalysisResultOpt)
                : base(valueDomain, containingTypeSymbol, nullAnalysisResultOpt, pointsToAnalysisResultOpt)
            {
            }

            protected override StringContentAbstractValue UnknownOrMayBeValue => StringContentAbstractValue.MayBeContainsNonLiteralState;
            protected override void SetAbstractValue(AnalysisEntity analysisEntity, StringContentAbstractValue value)
                => CurrentAnalysisData[analysisEntity] = value;

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity) =>
                CurrentAnalysisData.ContainsKey(analysisEntity);

            protected override StringContentAbstractValue GetAbstractValue(AnalysisEntity analysisEntity) =>
                CurrentAnalysisData.TryGetValue(analysisEntity, out var value) ? value : UnknownOrMayBeValue;

            protected override StringContentAbstractValue GetAbstractDefaultValue(ITypeSymbol type) =>
                StringContentAbstractValue.DoesNotContainNonLiteralState;

            protected override void ResetCurrentAnalysisData(StringContentAnalysisData newAnalysisDataOpt = null) => ResetAnalysisData(CurrentAnalysisData, newAnalysisDataOpt);

            #region Visitor methods
            public override StringContentAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var _ = base.DefaultVisit(operation, argument);
                if (operation.Type == null)
                {
                    return StringContentAbstractValue.DoesNotContainNonLiteralState;
                }

                if (operation.Type.SpecialType == SpecialType.System_String)
                {
                    if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is string value)
                    {
                        return StringContentAbstractValue.Create(value);
                    }
                    else
                    {
                        return StringContentAbstractValue.ContainsNonLiteralState;
                    }
                }

                return UnknownOrMayBeValue;
            }

            public override StringContentAbstractValue VisitBinaryOperator(IBinaryOperation operation, object argument)
            {
                switch (operation.OperatorKind)
                {
                    case BinaryOperatorKind.Add:
                    case BinaryOperatorKind.Concatenate:
                        var leftValue = Visit(operation.LeftOperand, argument);
                        var rightValue = Visit(operation.RightOperand, argument);
                        return leftValue.MergeBinaryAdd(rightValue);

                    default:
                        return base.VisitBinaryOperator(operation, argument);
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
