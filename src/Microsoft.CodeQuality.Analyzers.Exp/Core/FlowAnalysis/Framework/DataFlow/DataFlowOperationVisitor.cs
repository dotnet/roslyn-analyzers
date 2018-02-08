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

        protected abstract TAbstractAnalysisValue UnknownOrMayBeValue { get; }
        protected abstract void SetAbstractValue(AnalysisEntity analysisEntity, TAbstractAnalysisValue value);
        protected abstract TAbstractAnalysisValue GetAbstractValue(AnalysisEntity analysisEntity);
        protected abstract bool HasAbstractValue(AnalysisEntity analysisEntity);
        protected abstract TAbstractAnalysisValue GetAbstractDefaultValue(ITypeSymbol type);
        protected abstract void ResetCurrentAnalysisData(TAnalysisData newAnalysisDataOpt = default(TAnalysisData));
        protected virtual bool HasPointsToAnalysisResult => _pointsToAnalysisResultOpt != null;

        protected AbstractDomain<TAbstractAnalysisValue> ValueDomain { get; }
        protected TAnalysisData CurrentAnalysisData { get; private set; }
        protected BasicBlock CurrentBasicBlock { get; private set; }
        protected IOperation CurrentStatement { get; private set; }
        protected PointsToAbstractValue ThisOrMePointsToAbstractValue { get; }
        protected AnalysisEntityFactory AnalysisEntityFactory { get; }
        
        protected DataFlowOperationVisitor(
            AbstractDomain<TAbstractAnalysisValue> valueDomain,
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
            AnalysisEntityFactory = new AnalysisEntityFactory(
                HasPointsToAnalysisResult ? GetPointsToAbstractValue : (Func<IOperation, PointsToAbstractValue>)null,
                containingTypeSymbol);
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
            if (AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity analysisEntity))
            {
                if (!HasAbstractValue(analysisEntity))
                {
                    SetAbstractValue(analysisEntity, defaultValue);
                }

                return GetAbstractValue(analysisEntity);
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper method to reset analysis data for analysis entities.
        /// If <paramref name="newAnalysisDataOpt"/> is null, all the analysis values in <paramref name="currentAnalysisDataOpt"/> are set to <see cref="UnknownOrMayBeValue"/>.
        /// Otherwise, all the key-value paris in <paramref name="newAnalysisDataOpt"/> are transfered to <paramref name="currentAnalysisDataOpt"/> and keys in <paramref name="currentAnalysisDataOpt"/> which
        /// are not present in <paramref name="newAnalysisDataOpt"/> are set to <see cref="UnknownOrMayBeValue"/>.
        /// </summary>
        /// <param name="currentAnalysisDataOpt"></param>
        /// <param name="newAnalysisDataOpt"></param>
        protected void ResetAnalysisData(IDictionary<AnalysisEntity, TAbstractAnalysisValue> currentAnalysisDataOpt, IDictionary<AnalysisEntity, TAbstractAnalysisValue> newAnalysisDataOpt)
        {
            // Reset the current analysis data, while ensuring that we don't violate the monotonicity, i.e. we cannot remove any existing key from currentAnalysisData.
            if (newAnalysisDataOpt == null)
            {
                // Just set the values for existing keys to UnknownOrMayBeValue.
                foreach (var key in currentAnalysisDataOpt?.Keys.ToArray())
                {
                    SetAbstractValue(key, UnknownOrMayBeValue);
                }
            }
            else
            {
                // Merge the values from current and new analysis data.
                var keys = currentAnalysisDataOpt?.Keys.Concat(newAnalysisDataOpt.Keys).ToArray();
                foreach (var key in keys)
                {
                    var value1 = currentAnalysisDataOpt != null && currentAnalysisDataOpt.TryGetValue(key, out var currentValue) ? currentValue : ValueDomain.Bottom;
                    var value2 = newAnalysisDataOpt.TryGetValue(key, out var newValue) ? newValue : ValueDomain.Bottom;
                    var mergedValue = ValueDomain.Merge(value1, value2);
                    SetAbstractValue(key, mergedValue);
                }
            }
        }

        #region Helper methods to handle initialization/assignment operations
        private void SetAbstractValueForSymbolDeclaration(ISymbol symbol, IOperation initializer, TAbstractAnalysisValue initializerValue)
        {
            if (AnalysisEntityFactory.TryCreateForSymbolDeclaration(symbol, out AnalysisEntity analysisEntity))
            {
                SetAbstractValueForAssignment(analysisEntity, initializer, initializerValue);
            }
        }

        private void SetAbstractValueForElementInitializer(IOperation instance, ImmutableArray<AbstractIndex> indices, ITypeSymbol elementType, IOperation initializer, TAbstractAnalysisValue value)
        {
            if (AnalysisEntityFactory.TryCreateForElementInitializer(instance, indices, elementType, out AnalysisEntity analysisEntity))
            {
                SetAbstractValueForAssignment(analysisEntity, initializer, value);
            }
        }

        protected void SetAbstractValueForAssignment(IOperation target, IOperation assignedValueOperation, TAbstractAnalysisValue assignedValue)
        {
            if (AnalysisEntityFactory.TryCreate(target, out AnalysisEntity targetAnalysisEntity))
            {
                SetAbstractValueForAssignment(targetAnalysisEntity, assignedValueOperation, assignedValue);
            }
        }

        protected void SetAbstractValueForAssignment(AnalysisEntity targetAnalysisEntity, IOperation assignedValueOperation, TAbstractAnalysisValue assignedValue)
        {
            // Value type and string type assignment has copy semantics.
            if (HasPointsToAnalysisResult &&
                targetAnalysisEntity.Type.HasValueCopySemantics())
            {
                if (HasAbstractValue(targetAnalysisEntity))
                {
                    // Reset the analysis values for analysis entities within the target instance.
                    ResetValueTypeInstanceAnalysisData(targetAnalysisEntity);
                }

                // Transfer the values of symbols from the assigned instance to the analysis entities in the target instance.
                TransferValueTypeInstanceAnalysisDataForAssignment(targetAnalysisEntity, assignedValueOperation);
            }

            SetAbstractValue(targetAnalysisEntity, assignedValue);
        }

        #endregion

        #region Helper methods for reseting/transfer instance analysis data when PointsTo analysis results are available

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
                if (AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity analysisEntity))
                {
                    ResetValueTypeInstanceAnalysisData(analysisEntity);
                }
            }
            else
            {
                ResetReferenceTypeInstanceAnalysisData(operation);
            }
        }

        /// <summary>
        /// Resets all the analysis data for all <see cref="AnalysisEntity"/> instances that share the same <see cref="AnalysisEntity.InstanceLocation"/>
        /// as the given <paramref name="analysisEntity"/>.
        /// </summary>
        /// <param name="analysisEntity"></param>
        private void ResetValueTypeInstanceAnalysisData(AnalysisEntity analysisEntity)
        {
            Debug.Assert(HasPointsToAnalysisResult);
            Debug.Assert(analysisEntity.Type.HasValueCopySemantics());

            IEnumerable<AnalysisEntity> dependantAnalysisEntities = GetChildAnalysisEntities(analysisEntity);
            ResetInstanceAnalysisDataCore(dependantAnalysisEntities);
        }

        /// <summary>
        /// Resets all the analysis data for all <see cref="AnalysisEntity"/> instances that share the same <see cref="AnalysisEntity.InstanceLocation"/>
        /// as pointed to by given reference type <paramref name="operation"/>.
        /// </summary>
        /// <param name="operation"></param>
        private void ResetReferenceTypeInstanceAnalysisData(IOperation operation)
        {
            Debug.Assert(HasPointsToAnalysisResult);
            Debug.Assert(!operation.Type.HasValueCopySemantics());

            var pointsToValue = GetPointsToAbstractValue(operation);
            if (pointsToValue.Kind != PointsToAbstractValueKind.Known)
            {
                return;
            }

            IEnumerable<AnalysisEntity> dependantAnalysisEntities = GetChildAnalysisEntities(pointsToValue);
            ResetInstanceAnalysisDataCore(dependantAnalysisEntities);
        }

        /// <summary>
        /// Resets the analysis data for the given <paramref name="dependantAnalysisEntities"/>.
        /// </summary>
        /// <param name="dependantAnalysisEntities"></param>
        private void ResetInstanceAnalysisDataCore(IEnumerable<AnalysisEntity> dependantAnalysisEntities)
        {
            foreach (var dependentAnalysisEntity in dependantAnalysisEntities)
            {
                // Reset value.
                SetAbstractValue(dependentAnalysisEntity, UnknownOrMayBeValue);
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
                SetAbstractValueForAssignment(operation.Value, operation.Value, UnknownOrMayBeValue);
            }
        }

        /// <summary>
        /// Transfers the analysis data rooted from <paramref name="assignedValueOperation"/> to <paramref name="targetAnalysisEntity"/>, for a value type assignment operation.
        /// This involves transfer of data for of all <see cref="AnalysisEntity"/> instances that share the same <see cref="AnalysisEntity.InstanceLocation"/> as the valueAnalysisEntity for the <paramref name="assignedValueOperation"/>
        /// to all <see cref="AnalysisEntity"/> instances that share the same <see cref="AnalysisEntity.InstanceLocation"/> as <paramref name="targetAnalysisEntity"/>.
        /// </summary>
        private void TransferValueTypeInstanceAnalysisDataForAssignment(AnalysisEntity targetAnalysisEntity, IOperation assignedValueOperation)
        {
            Debug.Assert(HasPointsToAnalysisResult);
            Debug.Assert(targetAnalysisEntity.Type.HasValueCopySemantics());

            IEnumerable<AnalysisEntity> dependentAnalysisEntities;
            if (AnalysisEntityFactory.TryCreate(assignedValueOperation, out AnalysisEntity valueAnalysisEntity))
            {
                dependentAnalysisEntities = GetChildAnalysisEntities(valueAnalysisEntity);
            }
            else
            {
                // For allocations.
                PointsToAbstractValue newValueLocation = GetPointsToAbstractValue(assignedValueOperation);
                if (newValueLocation.Kind == PointsToAbstractValueKind.NoLocation)
                {
                    return;
                }

                dependentAnalysisEntities = GetChildAnalysisEntities(newValueLocation);
            }

            foreach (AnalysisEntity dependentInstance in dependentAnalysisEntities)
            {
                // Clone the dependent instance but with with target as the root.
                AnalysisEntity newAnalysisEntity = AnalysisEntityFactory.CreateWithNewInstanceRoot(dependentInstance, targetAnalysisEntity);
                var dependentValue = GetAbstractValue(dependentInstance);
                SetAbstractValue(newAnalysisEntity, dependentValue);
            }
        }

        private IEnumerable<AnalysisEntity> GetChildAnalysisEntities(AnalysisEntity analysisEntity)
        {
            IEnumerable<AnalysisEntity> dependentAnalysisEntities = GetChildAnalysisEntities(analysisEntity.InstanceLocation);
            if (analysisEntity.Type.HasValueCopySemantics())
            {
                dependentAnalysisEntities = dependentAnalysisEntities.Where(info => info.HasAncestorOrSelf(analysisEntity));
            }

            return dependentAnalysisEntities;
        }

        private IEnumerable<AnalysisEntity> GetChildAnalysisEntities(PointsToAbstractValue instanceLocationOpt)
        {
            // We are interested only in dependent child/member infos, not the root info.
            return instanceLocationOpt != null ?
                AnalysisEntityFactory.GetAnalysisEntitiesCreatedFromInstance(instanceLocationOpt).Where(info => info.IsChildOrInstanceMember) :
                ImmutableHashSet<AnalysisEntity>.Empty;
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

            return UnknownOrMayBeValue;
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

            return UnknownOrMayBeValue;
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

            return UnknownOrMayBeValue;
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
            return operation.OperatorMethod == null ? operandValue : UnknownOrMayBeValue;
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
                if (AnalysisEntityFactory.TryCreateForSymbolDeclaration(operation.Symbol, out AnalysisEntity analysisEntity))
                {
                    SetAbstractValue(analysisEntity, value);
                }
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