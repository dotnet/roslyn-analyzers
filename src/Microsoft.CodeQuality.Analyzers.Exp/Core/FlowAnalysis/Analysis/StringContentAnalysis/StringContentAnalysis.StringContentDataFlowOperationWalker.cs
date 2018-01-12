// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<ISymbol, StringContentAbstractValue>;

    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        /// <summary>
        /// Operation visitor to flow the string content values across a given statement in a basic block.
        /// </summary>
        private sealed class StringContentDataFlowOperationWalker : DataFlowOperationWalker<StringContentAnalysisData, StringContentAbstractValue>
        {
            public StringContentDataFlowOperationWalker(AbstractDomain<StringContentAbstractValue> valueDomain, DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt)
                : base(valueDomain, nullAnalysisResultOpt)
            {
            }

            protected override StringContentAbstractValue UninitializedValue => StringContentAbstractValue.DefaultNo;
            protected override StringContentAbstractValue DefaultValue => StringContentAbstractValue.DefaultMaybe;
            protected override void SetAbstractValue(ISymbol symbol, StringContentAbstractValue value)
                => CurrentAnalysisData[symbol] = value;

            protected override StringContentAbstractValue GetAbstractValue(ISymbol symbol) =>
                CurrentAnalysisData.TryGetValue(symbol, out var value) ? value : DefaultValue;

            protected override void ResetCurrentAnalysisData(StringContentAnalysisData newAnalysisDataOpt = null) =>
                CurrentAnalysisData.Reset(newAnalysisDataOpt);

            protected override StringContentAbstractValue GetAbstractDefaultValue(ITypeSymbol type) =>
                StringContentAbstractValue.DefaultNo;

            #region Visitor methods
            public override StringContentAbstractValue DefaultVisit(IOperation operation, object argument)
            {
                var unused = base.DefaultVisit(operation, argument);
                if (operation.Type == null)
                {
                    return StringContentAbstractValue.DefaultNo;
                }

                if (operation.Type.SpecialType == SpecialType.System_String)
                {
                    if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is string value)
                    {
                        return new StringContentAbstractValue(literal: value);
                    }
                    else
                    {
                        return new StringContentAbstractValue(nonLiteral: operation);
                    }
                }

                return DefaultValue;
            }

            public override StringContentAbstractValue VisitBinaryOperator(IBinaryOperation operation, object argument)
            {
                switch (operation.OperatorKind)
                {
                    case BinaryOperatorKind.Add:
                    case BinaryOperatorKind.Concatenate:
                        var leftValue = Visit(operation.LeftOperand, argument);
                        var rightValue = Visit(operation.RightOperand, argument);
                        return leftValue.MergeBinaryAdd(rightValue, operation);

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
                        value = leftValue.MergeBinaryAdd(rightValue, operation);
                        break;

                    default:
                        value = base.VisitCompoundAssignment(operation, argument);
                        break;
                }

                SetAbstractValueForAssignment(operation.Target, value);
                return value;
            }

            public override StringContentAbstractValue VisitNameOf(INameOfOperation operation, object argument)
            {
                var nameofValue = base.VisitNameOf(operation, argument);
                if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is string value)
                {
                    return new StringContentAbstractValue(literal: value);
                }

                return nameofValue;
            }

            public override StringContentAbstractValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
            {
                // TODO: Analyze string constructor
                return base.VisitObjectCreation(operation, argument);
            }

            public override StringContentAbstractValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                var value = base.VisitFieldReference(operation, argument);

                // Handle "string.Empty"
                if (operation.Field.Name.Equals("Empty", StringComparison.Ordinal) &&
                    operation.Field.ContainingType.SpecialType == SpecialType.System_String)
                {
                    return new StringContentAbstractValue(literal: string.Empty);
                }

                return value;
            }

            public override StringContentAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
            {
                // TODO: Handle invocations of string methods (Format, SubString, Replace, etc.)
                return base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
            }

            public override StringContentAbstractValue VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                if (operation.Parts.IsEmpty)
                {
                    return new StringContentAbstractValue(literal: string.Empty);
                }

                StringContentAbstractValue mergedValue = Visit(operation.Parts[0], argument);
                for (int i = 1; i < operation.Parts.Length; i++)
                {
                    var newValue = Visit(operation.Parts[i], argument);
                    mergedValue = mergedValue.MergeBinaryAdd(newValue, operation);
                }

                return mergedValue;
            }

            #endregion
        }
    }
}
