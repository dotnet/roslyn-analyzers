// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Operation visitor to flow the abstract dataflow analysis values across a given statement in a basic block.
    /// </summary>
    internal abstract class DataFlowOperationVisitor<TAnalysisData, TAbstractAnalysisValue> : OperationVisitor<object, TAbstractAnalysisValue>
    {
        private readonly DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> _nullAnalysisResultOpt;
        private readonly DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> _pointsToAnalysisResultOpt;
        private readonly ImmutableDictionary<IOperation, TAbstractAnalysisValue>.Builder _valueCacheBuilder;
        private readonly List<IArgumentOperation> _pendingArgumentsToReset;

        private int _recursionDepth;

        protected abstract TAbstractAnalysisValue GetAbstractDefaultValue(ITypeSymbol type);
        protected abstract void ResetCurrentAnalysisData(TAnalysisData newAnalysisDataOpt = default(TAnalysisData));
        protected bool HasPointsToAnalysisResult => _pointsToAnalysisResultOpt != null || IsPointsToAnalysis;
        protected virtual bool IsPointsToAnalysis => false;

        protected AbstractValueDomain<TAbstractAnalysisValue> ValueDomain { get; }
        protected TAnalysisData CurrentAnalysisData { get; private set; }
        protected BasicBlock CurrentBasicBlock { get; private set; }
        protected IOperation CurrentStatement { get; private set; }
        protected PointsToAbstractValue ThisOrMePointsToAbstractValue { get; }

        protected DataFlowOperationVisitor(
            AbstractValueDomain<TAbstractAnalysisValue> valueDomain,
            INamedTypeSymbol containingTypeSymbol,
            DataFlowAnalysisResult<NullBlockAnalysisResult, NullAbstractValue> nullAnalysisResultOpt,
            DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResultOpt)
        {
            ValueDomain = valueDomain;
            _nullAnalysisResultOpt = nullAnalysisResultOpt;
            _pointsToAnalysisResultOpt = pointsToAnalysisResultOpt;
            _valueCacheBuilder = ImmutableDictionary.CreateBuilder<IOperation, TAbstractAnalysisValue>();
            _pendingArgumentsToReset = new List<IArgumentOperation>();
            ThisOrMePointsToAbstractValue = GetThisOrMeInstancePointsToValue(containingTypeSymbol);
        }

        private static PointsToAbstractValue GetThisOrMeInstancePointsToValue(INamedTypeSymbol containingTypeSymbol)
        {
            if (!containingTypeSymbol.HasValueCopySemantics())
            {
                var thisOrMeLocation = AbstractLocation.CreateThisOrMeLocation(containingTypeSymbol);
                return new PointsToAbstractValue(thisOrMeLocation);
            }
            else
            {
                return PointsToAbstractValue.NoLocation;
            }
        }

        /// <summary>
        /// Primary method that flows analysis data through the given statement.
        /// </summary>
        public virtual TAnalysisData Flow(IOperation statement, BasicBlock block, TAnalysisData input)
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
                var _ = GetCachedAbstractValue(operation);
            }
#endif

            return CurrentAnalysisData;
        }

        #region Helper methods to get or cache analysis data for visited operations.

        public ImmutableDictionary<IOperation, TAbstractAnalysisValue> GetStateMap() => _valueCacheBuilder.ToImmutable();

        public TAbstractAnalysisValue GetCachedAbstractValue(IOperation operation)
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

        protected void CacheAbstractValue(IOperation operation, TAbstractAnalysisValue value)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            _valueCacheBuilder[operation] = value;
        }

        protected virtual NullAbstractValue GetNullAbstractValue(IOperation operation)
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

        protected virtual PointsToAbstractValue GetPointsToAbstractValue(IOperation operation)
        {
            if (_pointsToAnalysisResultOpt == null)
            {
                return PointsToAbstractValue.Unknown;
            }
            else
            {
                return _pointsToAnalysisResultOpt[operation];
            }
        }

        #endregion region

        protected virtual TAbstractAnalysisValue ComputeAnalysisValueForReferenceOperation(IOperation operation, TAbstractAnalysisValue defaultValue)
        {
            return defaultValue;
        }

        #region Helper methods to handle initialization/assignment operations
        protected abstract void SetAbstractValueForSymbolDeclaration(ISymbol symbol, IOperation initializer, TAbstractAnalysisValue initializerValue);
        protected abstract void SetAbstractValueForElementInitializer(IOperation instance, ImmutableArray<AbstractIndex> indices, ITypeSymbol elementType, IOperation initializer, TAbstractAnalysisValue value);
        protected abstract void SetAbstractValueForAssignment(IOperation target, IOperation assignedValueOperation, TAbstractAnalysisValue assignedValue);
        #endregion

        #region Helper methods for reseting/transfer instance analysis data when PointsTo analysis results are available

        protected abstract void ResetValueTypeInstanceAnalysisData(IOperation operation);
        protected abstract void ResetReferenceTypeInstanceAnalysisData(IOperation operation);

        /// <summary>
        /// Reset all the instance analysis data if <see cref="HasPointsToAnalysisResult"/> is true.
        /// If we are using or performing points to analysis, certain operations can invalidate all the analysis data off the containing instance.
        /// </summary>
        private void ResetInstanceAnalysisData(IOperation operation)
        {
            if (operation == null || !HasPointsToAnalysisResult)
            {
                return;
            }

            if (operation.Type.HasValueCopySemantics())
            {
                ResetValueTypeInstanceAnalysisData(operation);
            }
            else
            {
                ResetReferenceTypeInstanceAnalysisData(operation);
            }
        }

        /// <summary>
        /// Resets the analysis data for an object instance passed around as an <see cref="IArgumentOperation"/>.
        /// </summary>
        private void ResetInstanceAnalysisDataForArgument(IArgumentOperation operation)
        {
            // For reference types passed as arguments, 
            // reset all analysis data for the instance members as the content might change for them.
            if (HasPointsToAnalysisResult &&
                !operation.Value.Type.HasValueCopySemantics())
            {
                ResetReferenceTypeInstanceAnalysisData(operation.Value);
            }

            // Handle ref/out arguments as escapes.
            if (operation.Parameter.RefKind != RefKind.None)
            {
                SetAbstractValueForAssignment(operation.Value, operation.Value, ValueDomain.UnknownOrMayBeValue);
            }
        }

        #endregion

        // TODO: Remove these temporary methods once we move to compiler's CFG
        // https://github.com/dotnet/roslyn-analyzers/issues/1567
        #region Temporary methods to workaround lack of *real* CFG
        protected abstract TAnalysisData MergeAnalysisData(TAnalysisData value1, TAnalysisData value2);
        protected abstract TAnalysisData GetClonedAnalysisData();
        protected abstract bool Equals(TAnalysisData value1, TAnalysisData value2);
        protected static bool EqualsHelper<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
            => dict1.Count == dict2.Count &&
               dict1.Keys.All(key => dict2.TryGetValue(key, out TValue value2) && EqualityComparer<TValue>.Default.Equals(dict1[key], value2));

        public override TAbstractAnalysisValue VisitCoalesce(ICoalesceOperation operation, object argument)
        {
            var leftValue = Visit(operation.Value, argument);
            var rightValue = Visit(operation.WhenNull, argument);
            var leftNullValue = GetNullAbstractValue(operation.Value);
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
            var leftNullValue = GetNullAbstractValue(operation.Operation);
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

        public override TAbstractAnalysisValue VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation, object argument)
        {
            IConditionalAccessOperation conditionalAccess = operation.GetConditionalAccess();
            return GetCachedAbstractValue(conditionalAccess.Operation);
        }

        public override TAbstractAnalysisValue VisitConditional(IConditionalOperation operation, object argument)
        {
            var unusedConditionValue = Visit(operation.Condition, argument);
            var whenFalseBranchAnalysisData = GetClonedAnalysisData();
            var whenTrue = Visit(operation.WhenTrue, argument);
            var whenTrueBranchAnalysisData = CurrentAnalysisData;
            CurrentAnalysisData = whenFalseBranchAnalysisData;
            var whenFalse = Visit(operation.WhenFalse, argument);
            whenFalseBranchAnalysisData = CurrentAnalysisData;

            if (operation.Condition.ConstantValue.HasValue &&
                operation.Condition.ConstantValue.Value is bool condition)
            {
                CurrentAnalysisData = condition ? whenTrueBranchAnalysisData : whenFalseBranchAnalysisData;
                return condition ? whenTrue : whenFalse;
            }

            CurrentAnalysisData = MergeAnalysisData(whenTrueBranchAnalysisData, whenFalseBranchAnalysisData);
            return ValueDomain.Merge(whenTrue, whenFalse);
        }

        public override TAbstractAnalysisValue VisitWhileLoop(IWhileLoopOperation operation, object argument)
        {
            var previousAnalysisData = GetClonedAnalysisData();
            var fixedPointReached = false;
            do
            {
                if (operation.ConditionIsTop)
                {
                    var _ = Visit(operation.Condition, argument);
                }

                var unusedBodyValue = Visit(operation.Body, argument);
                if (!operation.ConditionIsTop)
                {
                    var _ = Visit(operation.Condition, argument);
                }

                var mergedAnalysisData = MergeAnalysisData(previousAnalysisData, CurrentAnalysisData);
                fixedPointReached = Equals(previousAnalysisData, mergedAnalysisData);
                previousAnalysisData = CurrentAnalysisData;
                CurrentAnalysisData = mergedAnalysisData;
            }
            while (!fixedPointReached);

            var unusedIgnoredCondition = Visit(operation.IgnoredCondition, argument);
            return ValueDomain.Bottom;
        }

        public override TAbstractAnalysisValue VisitForLoop(IForLoopOperation operation, object argument)
        {
            var unusedBeforeValue = VisitArray(operation.Before, argument);
            var previousAnalysisData = GetClonedAnalysisData();
            var fixedPointReached = false;
            do
            {
                var unusedConditionValue = Visit(operation.Condition, argument);
                var unusedBodyValue = Visit(operation.Body, argument);
                var unusedLoopBottomValue = VisitArray(operation.AtLoopBottom, argument);

                var mergedAnalysisData = MergeAnalysisData(previousAnalysisData, CurrentAnalysisData);
                fixedPointReached = Equals(previousAnalysisData, mergedAnalysisData);
                previousAnalysisData = CurrentAnalysisData;
                CurrentAnalysisData = mergedAnalysisData;
            }
            while (!fixedPointReached);

            return ValueDomain.Bottom;
        }

        public override TAbstractAnalysisValue VisitForEachLoop(IForEachLoopOperation operation, object argument)
        {
            var unusedLoopControlVariableValue = Visit(operation.LoopControlVariable, argument);
            var unusedCollectionValue = Visit(operation.Collection, argument);

            var previousAnalysisData = GetClonedAnalysisData();
            var fixedPointReached = false;
            do
            {
                var unusedBodyValue = Visit(operation.Body, argument);

                var mergedAnalysisData = MergeAnalysisData(previousAnalysisData, CurrentAnalysisData);
                fixedPointReached = Equals(previousAnalysisData, mergedAnalysisData);
                previousAnalysisData = CurrentAnalysisData;
                CurrentAnalysisData = mergedAnalysisData;
            }
            while (!fixedPointReached);

            return ValueDomain.Bottom;
        }

        public override TAbstractAnalysisValue VisitForToLoop(IForToLoopOperation operation, object argument)
        {
            var loopControlVariableValue = Visit(operation.LoopControlVariable, argument);
            var initialValue = Visit(operation.InitialValue, argument);
            SetAbstractValueForAssignment(operation.LoopControlVariable, operation.InitialValue, initialValue);

            var previousAnalysisData = GetClonedAnalysisData();
            var fixedPointReached = false;
            do
            {
                var unusedLimitValue = Visit(operation.LimitValue, argument);
                var unusedBodyValue = Visit(operation.Body, argument);
                var unusedStepValue = Visit(operation.StepValue, argument);
                var unusedNextVariablesValue = VisitArray(operation.NextVariables, argument);

                var mergedAnalysisData = MergeAnalysisData(previousAnalysisData, CurrentAnalysisData);
                fixedPointReached = Equals(previousAnalysisData, mergedAnalysisData);
                previousAnalysisData = CurrentAnalysisData;
                CurrentAnalysisData = mergedAnalysisData;
            }
            while (!fixedPointReached);

            return ValueDomain.Bottom;
        }

        #endregion

        #region Visitor methods

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
                CacheAbstractValue(operation, value);

                if (_pendingArgumentsToReset.Any(arg => arg.Parent == operation))
                {
                    var pendingArguments = _pendingArgumentsToReset.Where(arg => arg.Parent == operation).ToImmutableArray();
                    foreach (IArgumentOperation argumentOperation in pendingArguments)
                    {
                        ResetInstanceAnalysisDataForArgument(argumentOperation);
                        _pendingArgumentsToReset.Remove(argumentOperation);
                    }
                }

                return value;
            }

            return ValueDomain.UnknownOrMayBeValue;
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
            TAbstractAnalysisValue assignedValue = Visit(operation.Value, argument);
            SetAbstractValueForAssignment(operation.Target, operation.Value, assignedValue);

            return assignedValue;
        }

        public override TAbstractAnalysisValue VisitMemberInitializer(IMemberInitializerOperation operation, object argument)
        {
            TAbstractAnalysisValue _ = Visit(operation.InitializedMember, argument);
            TAbstractAnalysisValue assignedValue = Visit(operation.Initializer, argument);
            SetAbstractValueForAssignment(operation.InitializedMember, operation.Initializer, assignedValue);
            return assignedValue;
        }

        public override TAbstractAnalysisValue VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation, object argument)
        {
            // Special handling for collection initializers as we need to track indices.
            uint collectionElementInitializerIndex = 0;
            foreach (var elementInitializer in operation.Initializers)
            {
                if (elementInitializer is ICollectionElementInitializerOperation collectionElementInitializer)
                {
                    var _ = Visit(elementInitializer, argument: collectionElementInitializerIndex);
                    collectionElementInitializerIndex += (uint)collectionElementInitializer.Arguments.Length;
                }
                else
                {
                    var _ = Visit(elementInitializer, argument);
                }
            }

            return ValueDomain.UnknownOrMayBeValue;
        }

        public override TAbstractAnalysisValue VisitCollectionElementInitializer(ICollectionElementInitializerOperation operation, object argument)
        {
            var objectCreation = operation.GetAncestor<IObjectCreationOperation>(OperationKind.ObjectCreation);
            ITypeSymbol collectionElementType = operation.AddMethod?.Parameters.FirstOrDefault()?.Type;
            if (collectionElementType != null)
            {
                var index = (uint)argument;
                for (int i = 0; i < operation.Arguments.Length; i++, index++)
                {
                    AbstractIndex abstractIndex = AbstractIndex.Create(index);
                    IOperation elementInitializer = operation.Arguments[i];
                    TAbstractAnalysisValue argumentValue = Visit(elementInitializer, argument: null);
                    SetAbstractValueForElementInitializer(objectCreation, ImmutableArray.Create(abstractIndex), collectionElementType, elementInitializer, argumentValue);
                }
            }
            else
            {
                var _ = base.VisitCollectionElementInitializer(operation, argument: null);
            }

            return ValueDomain.UnknownOrMayBeValue;
        }

        public override TAbstractAnalysisValue VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
        {
            var arrayCreation = (IArrayCreationOperation)operation.Parent;
            var elementType = ((IArrayTypeSymbol)arrayCreation.Type).ElementType;
            for (int index = 0; index < operation.ElementValues.Length; index++)
            {
                AbstractIndex abstractIndex = AbstractIndex.Create((uint)index);
                IOperation elementInitializer = operation.ElementValues[index];
                TAbstractAnalysisValue initializerValue = Visit(elementInitializer, argument);
                SetAbstractValueForElementInitializer(arrayCreation, ImmutableArray.Create(abstractIndex), elementType, elementInitializer, initializerValue);
            }

            return ValueDomain.UnknownOrMayBeValue;
        }

        public override TAbstractAnalysisValue VisitLocalReference(ILocalReferenceOperation operation, object argument)
        {
            var value = base.VisitLocalReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitParameterReference(IParameterReferenceOperation operation, object argument)
        {
            var value = base.VisitParameterReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitArrayElementReference(IArrayElementReferenceOperation operation, object argument)
        {
            var value = base.VisitArrayElementReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, object argument)
        {
            var value = base.VisitDynamicMemberReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitEventReference(IEventReferenceOperation operation, object argument)
        {
            var value = base.VisitEventReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitFieldReference(IFieldReferenceOperation operation, object argument)
        {
            var value = base.VisitFieldReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitMethodReference(IMethodReferenceOperation operation, object argument)
        {
            var value = base.VisitMethodReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitPropertyReference(IPropertyReferenceOperation operation, object argument)
        {
            var value = base.VisitPropertyReference(operation, argument);
            return ComputeAnalysisValueForReferenceOperation(operation, value);
        }

        public override TAbstractAnalysisValue VisitDefaultValue(IDefaultValueOperation operation, object argument)
        {
            return GetAbstractDefaultValue(operation.Type);
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
            _pendingArgumentsToReset.Add(operation);
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
            return operation.OperatorMethod == null ? operandValue : ValueDomain.UnknownOrMayBeValue;
        }

        protected virtual TAbstractAnalysisValue VisitSymbolInitializer(ISymbolInitializerOperation operation, ISymbol initializedSymbol, object argument)
        {
            var value = Visit(operation.Value, argument);
            SetAbstractValueForSymbolDeclaration(initializedSymbol, operation.Value, value);
            return value;
        }

        private TAbstractAnalysisValue VisitSymbolInitializer(ISymbolInitializerOperation operation, IEnumerable<ISymbol> initializedSymbols, object argument)
        {
            var value = Visit(operation.Value, argument);
            foreach (var initializedSymbol in initializedSymbols)
            {
                SetAbstractValueForSymbolDeclaration(initializedSymbol, operation.Value, value);
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
                SetAbstractValueForSymbolDeclaration(operation.Symbol, initializer: null, initializerValue: value);
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
            TAbstractAnalysisValue value;
            switch (operation.TargetMethod.MethodKind)
            {
                case MethodKind.LambdaMethod:
                case MethodKind.LocalFunction:
                case MethodKind.DelegateInvoke:
                    // Invocation of a lambda or local function.
                    value = VisitInvocation_LambdaOrDelegateOrLocalFunction(operation, argument);
                    break;

                default:
                    value = VisitInvocation_NonLambdaOrDelegateOrLocalFunction(operation, argument);
                    break;
            }

            // Invocation might invalidate all the analysis data on the invoked instance.
            // Conservatively reset all the instance analysis data.
            ResetInstanceAnalysisData(operation.Instance);

            return value;
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

        #endregion
    }
}