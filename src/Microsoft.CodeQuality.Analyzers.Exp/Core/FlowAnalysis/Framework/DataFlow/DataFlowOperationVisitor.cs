// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Operation visitor to flow the abstract dataflow analysis values across a given statement in a basic block.
    /// </summary>
    internal abstract class DataFlowOperationVisitor<TAnalysisData, TAbstractAnalysisValue> : OperationVisitor<object, TAbstractAnalysisValue>
    {
        private readonly DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> _nullAnalysisResultOpt;
        private readonly ImmutableDictionary<IOperation, TAbstractAnalysisValue>.Builder _valueCacheBuilder;

        private int _recursionDepth;

        protected AbstractDomain<TAbstractAnalysisValue> ValueDomain { get; }
        protected abstract TAbstractAnalysisValue UnknownOrMayBeValue { get; }

        protected abstract void SetAbstractValue(ISymbol symbol, TAbstractAnalysisValue value);
        protected abstract TAbstractAnalysisValue GetAbstractValue(ISymbol symbol);
        protected abstract TAbstractAnalysisValue GetAbstractDefaultValue(ITypeSymbol type);
        protected abstract void ResetCurrentAnalysisData(TAnalysisData newAnalysisDataOpt = default(TAnalysisData));
        protected TAnalysisData CurrentAnalysisData { get; private set; }
        protected BasicBlock CurrentBasicBlock { get; private set; }
        protected IOperation CurrentStatement { get; private set; }

        protected DataFlowOperationVisitor(AbstractDomain<TAbstractAnalysisValue> valueDomain, DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> nullAnalysisResultOpt)
        {
            ValueDomain = valueDomain;
            _nullAnalysisResultOpt = nullAnalysisResultOpt;
            _valueCacheBuilder = ImmutableDictionary.CreateBuilder<IOperation, TAbstractAnalysisValue>();
        }

        public ImmutableDictionary<IOperation, TAbstractAnalysisValue> GetStateMap() => _valueCacheBuilder.ToImmutable();

        public TAbstractAnalysisValue GetState(IOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            TAbstractAnalysisValue state;
            if (!_valueCacheBuilder.TryGetValue(operation, out state))
            {
                throw new InvalidOperationException();
            }

            return state;
        }

        protected NullAbstractValue GetNullState(IOperation operation)
        {
            if (_nullAnalysisResultOpt == null)
            {
                return NullAbstractValue.MaybeNull;
            }
            else
            {
                return _nullAnalysisResultOpt[operation];
            }
        }

        public TAnalysisData Flow(IOperation statement, BasicBlock block, TAnalysisData input)
        {
            CurrentStatement = statement;
            CurrentBasicBlock = block;
            CurrentAnalysisData = input;
            Visit(statement, null);

#if DEBUG
            // Ensure that we visited and cached values for all operation descendants.
            foreach (var operation in statement.DescendantsAndSelf())
            {
                // GetState will throw an InvalidOperationException if the visitor did not visit the operation or cache it's abstract value.
                var _ = GetState(operation);
            }
#endif

            return CurrentAnalysisData;
        }

        internal TAbstractAnalysisValue VisitArray(IEnumerable<IOperation> operations, object argument)
        {
            var values = new List<TAbstractAnalysisValue>();
            foreach (var operation in operations)
            {
                var result = VisitOperationArrayElement(operation, argument);
                values.Add(result);
            }

            return ValueDomain.Merge(values);
        }

        internal TAbstractAnalysisValue VisitOperationArrayElement(IOperation operation, object argument)
        {
            return Visit(operation, argument);
        }

        public override TAbstractAnalysisValue Visit(IOperation operation, object argument)
        {
            if (operation != null)
            {
                var value = VisitCore(operation, argument);
                _valueCacheBuilder[operation] = value;
                return value;
            }

            return UnknownOrMayBeValue;
        }

        private TAbstractAnalysisValue VisitCore(IOperation operation, object argument)
        {
            if (operation.Kind == OperationKind.None)
            {
                return DefaultVisit(operation, argument);
            }

            _recursionDepth++;
            try
            {
                StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
                return operation.Accept(this, argument);
            }
            finally
            {
                _recursionDepth--;
            }
        }

        public override TAbstractAnalysisValue DefaultVisit(IOperation operation, object argument)
        {
            return VisitArray(operation.Children, argument);
        }

        public override TAbstractAnalysisValue VisitSimpleAssignment(ISimpleAssignmentOperation operation, object argument)
        {
            return VisitAssignmentOperation(operation, argument);
        }

        public override TAbstractAnalysisValue VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, object argument)
        {
            return VisitAssignmentOperation(operation, argument);
        }

        protected virtual TAbstractAnalysisValue VisitAssignmentOperation(IAssignmentOperation operation, object argument)
        {
            TAbstractAnalysisValue _ = Visit(operation.Target, argument);
            TAbstractAnalysisValue value = Visit(operation.Value, argument);
            SetAbstractValueForAssignment(operation.Target, value);

            return value;
        }

        protected void SetAbstractValueForAssignment(IOperation target, TAbstractAnalysisValue value)
        {
            if (target is ILocalReferenceOperation localReference)
            {
                SetAbstractValue(localReference.Local, value);
            }
            else if (target is IParameterReferenceOperation parameterReference)
            {
                SetAbstractValue(parameterReference.Parameter, value);
            }
        }

        public override TAbstractAnalysisValue VisitLocalReference(ILocalReferenceOperation operation, object argument)
        {
            return GetAbstractValue(operation.Local);
        }

        public override TAbstractAnalysisValue VisitParameterReference(IParameterReferenceOperation operation, object argument)
        {
            return GetAbstractValue(operation.Parameter);
        }

        public override TAbstractAnalysisValue VisitDefaultValue(IDefaultValueOperation operation, object argument)
        {
            return GetAbstractDefaultValue(operation.Type);
        }

        public override TAbstractAnalysisValue VisitCoalesce(ICoalesceOperation operation, object argument)
        {
            var leftValue = Visit(operation.Value, argument);
            var rightValue = Visit(operation.WhenNull, argument);
            var leftNullValue = GetNullState(operation.Value);
            switch (leftNullValue)
            {
                case NullAbstractValue.Null:
                    return rightValue;

                case NullAbstractValue.NotNull:
                    return leftValue;

                default:
                    return ValueDomain.Merge(leftValue, rightValue);
            }
        }

        public override TAbstractAnalysisValue VisitConditionalAccess(IConditionalAccessOperation operation, object argument)
        {
            var leftValue = Visit(operation.Operation, argument);
            var whenNullValue = Visit(operation.WhenNotNull, argument);
            var leftNullValue = GetNullState(operation.Operation);
            switch (leftNullValue)
            {
                case NullAbstractValue.Null:
                    return GetAbstractDefaultValue(operation.WhenNotNull.Type);

                case NullAbstractValue.NotNull:
                    return whenNullValue;

                default:
                    var value1 = GetAbstractDefaultValue(operation.WhenNotNull.Type);
                    return ValueDomain.Merge(value1, whenNullValue);
            }
        }

        public override TAbstractAnalysisValue VisitConditional(IConditionalOperation operation, object argument)
        {
            var _ = Visit(operation.Condition, argument);
            var whenTrue = Visit(operation.WhenTrue, argument);
            var whenFalse = Visit(operation.WhenFalse, argument);

            if (operation.Condition.ConstantValue.HasValue &&
                operation.Condition.ConstantValue.Value is bool condition)
            {
                return condition ? whenTrue : whenFalse;
            }

            return ValueDomain.Merge(whenTrue, whenFalse);
        }

        public override TAbstractAnalysisValue VisitInterpolation(IInterpolationOperation operation, object argument)
        {
            var expressionValue = Visit(operation.Expression, argument);
            var formatValue = Visit(operation.FormatString, argument);
            var alignmentValue = Visit(operation.Alignment, argument);
            return expressionValue;
        }

        public override TAbstractAnalysisValue VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, object argument)
        {
            return Visit(operation.Text, argument);
        }

        public override TAbstractAnalysisValue VisitArgument(IArgumentOperation operation, object argument)
        {
            var value = Visit(operation.Value, argument);

            // Handle ref/out arguments as escapes.
            if (operation.Parameter.RefKind != RefKind.None)
            {
                ISymbol symbol;
                switch (operation.Value)
                {
                    case ILocalReferenceOperation localReference:
                        symbol = localReference.Local;
                        break;

                    case IParameterReferenceOperation parameterReference:
                        symbol = parameterReference.Parameter;
                        break;

                    case IMemberReferenceOperation memberReference:
                        symbol = memberReference.Member;
                        break;

                    default:
                        symbol = null;
                        break;
                }

                if (symbol != null)
                {
                    SetAbstractValue(symbol, UnknownOrMayBeValue);
                }

                return UnknownOrMayBeValue;
            }

            return value;
        }

        public override TAbstractAnalysisValue VisitConstantPattern(IConstantPatternOperation operation, object argument)
        {
            return Visit(operation.Value, argument);
        }

        public override TAbstractAnalysisValue VisitParenthesized(IParenthesizedOperation operation, object argument)
        {
            return Visit(operation.Operand, argument);
        }

        public override TAbstractAnalysisValue VisitTranslatedQuery(ITranslatedQueryOperation operation, object argument)
        {
            return Visit(operation.Operation, argument);
        }

        public override TAbstractAnalysisValue VisitConversion(IConversionOperation operation, object argument)
        {
            var operandValue = Visit(operation.Operand, argument);

            // Conservative for user defined operator.
            return operation.OperatorMethod == null ? operandValue : UnknownOrMayBeValue;
        }

        protected virtual TAbstractAnalysisValue VisitSymbolInitializer(ISymbolInitializerOperation operation, ISymbol initializedSymbol, object argument)
        {
            var value = Visit(operation.Value, argument);
            SetAbstractValue(initializedSymbol, value);
            return value;
        }

        private TAbstractAnalysisValue VisitSymbolInitializer(ISymbolInitializerOperation operation, IEnumerable<ISymbol> initializedSymbols, object argument)
        {
            var value = Visit(operation.Value, argument);
            foreach (var initializedSymbol in initializedSymbols)
            {
                SetAbstractValue(initializedSymbol, value);
            }

            return value;
        }

        public override TAbstractAnalysisValue VisitVariableDeclarator(IVariableDeclaratorOperation operation, object argument)
        {
            var value = base.VisitVariableDeclarator(operation, argument);

            // Handle variable declarations without initializer (IVariableInitializerOperation). 
            var initializer = operation.GetVariableInitializer();
            if (initializer == null)
            {
                value = ValueDomain.Bottom;
                SetAbstractValue(operation.Symbol, value);
            }

            return value;
        }

        public override TAbstractAnalysisValue VisitVariableInitializer(IVariableInitializerOperation operation, object argument)
        {
            if (operation.Parent is IVariableDeclaratorOperation declarator)
            {
                return VisitSymbolInitializer(operation, declarator.Symbol, argument);
            }
            else if (operation.Parent is IVariableDeclarationOperation declaration)
            {
                var symbols = declaration.Declarators.Select(d => d.Symbol);
                return VisitSymbolInitializer(operation, symbols, argument);
            }

            return base.VisitVariableInitializer(operation, argument);
        }

        public override TAbstractAnalysisValue VisitFieldInitializer(IFieldInitializerOperation operation, object argument)
        {
            return VisitSymbolInitializer(operation, operation.InitializedFields, argument);
        }

        public override TAbstractAnalysisValue VisitParameterInitializer(IParameterInitializerOperation operation, object argument)
        {
            return VisitSymbolInitializer(operation, operation.Parameter, argument);
        }

        public override TAbstractAnalysisValue VisitPropertyInitializer(IPropertyInitializerOperation operation, object argument)
        {
            return VisitSymbolInitializer(operation, operation.InitializedProperties, argument);
        }

        public sealed override TAbstractAnalysisValue VisitInvocation(IInvocationOperation operation, object argument)
        {
            switch (operation.TargetMethod.MethodKind)
            {
                case MethodKind.LambdaMethod:
                case MethodKind.LocalFunction:
                case MethodKind.DelegateInvoke:
                    // Invocation of a lambda or local function.
                    return VisitInvocation_LambdaOrDelegateOrLocalFunction(operation, argument);

                default:
                    return VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
            }
        }

        public virtual TAbstractAnalysisValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
        {
            return base.VisitInvocation(operation, argument);
        }

        public virtual TAbstractAnalysisValue VisitInvocation_LambdaOrDelegateOrLocalFunction(IInvocationOperation operation, object argument)
        {
            var value = base.VisitInvocation(operation, argument);

            // Currently, we are not performing flow analysis for invocations of lambda or delegate or local function.
            // Pessimistically assume that all the current state could change and reset all our current analysis data.
            // TODO: Analyze lambda and local functions and flow the values from it's exit block to CurrentAnalysisData.
            // https://github.com/dotnet/roslyn-analyzers/issues/1547
            ResetCurrentAnalysisData();
            return value;
        }

        protected void ResetAnalysisData(IDictionary<ISymbol, TAbstractAnalysisValue> currentAnalysisData, IDictionary<ISymbol, TAbstractAnalysisValue> newAnalysisDataOpt)
        {
            // Reset the current analysis data, while ensuring that we don't violate the monotonicity, i.e. we cannot remove any existing key from currentAnalysisData.
            if (newAnalysisDataOpt == null)
            {
                // Just set the values for existing keys to UnknownOrMayBeValue.
                foreach (var key in currentAnalysisData.Keys.ToArray())
                {
                    SetAbstractValue(key, UnknownOrMayBeValue);
                }
            }
            else
            {
                // Merge the values from current and new analysis data.
                var keys = currentAnalysisData.Keys.Concat(newAnalysisDataOpt.Keys).ToArray();
                foreach (var key in keys)
                {
                    var value1 = currentAnalysisData.TryGetValue(key, out var currentValue) ? currentValue : ValueDomain.Bottom;
                    var value2 = newAnalysisDataOpt.TryGetValue(key, out var newValue) ? newValue : ValueDomain.Bottom;
                    var mergedValue = ValueDomain.Merge(value1, value2);
                    SetAbstractValue(key, mergedValue);
                }
            }
        }
    }
}