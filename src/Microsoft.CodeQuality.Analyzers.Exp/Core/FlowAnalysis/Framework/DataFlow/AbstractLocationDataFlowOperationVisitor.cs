// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Operation visitor to flow the abstract dataflow analysis values for <see cref="AbstractLocation"/>s across a given statement in a basic block.
    /// </summary>
    internal abstract class AbstractLocationDataFlowOperationVisitor<TAnalysisData, TAbstractAnalysisValue> : DataFlowOperationVisitor<TAnalysisData, TAbstractAnalysisValue>
    {
        protected AbstractLocationDataFlowOperationVisitor(
            AbstractValueDomain<TAbstractAnalysisValue> valueDomain,
            ISymbol owningSymbol,
            WellKnownTypeProvider wellKnownTypeProvider,
            bool pessimisticAnalysis,
            bool predicateAnalysis,
            DataFlowAnalysisResult<CopyAnalysis.CopyBlockAnalysisResult, CopyAnalysis.CopyAbstractValue> copyAnalysisResultOpt,
            DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResultOpt)
            : base(valueDomain, owningSymbol, wellKnownTypeProvider, pessimisticAnalysis, predicateAnalysis,
                  copyAnalysisResultOpt, pointsToAnalysisResultOpt)
        {
            Debug.Assert(pointsToAnalysisResultOpt != null);
        }

        protected abstract TAbstractAnalysisValue GetAbstractValue(AbstractLocation location);
        protected abstract void SetAbstractValue(AbstractLocation location, TAbstractAnalysisValue value);
        protected virtual void SetAbstractValue(PointsToAbstractValue instanceLocation, TAbstractAnalysisValue value)
        {
            foreach (var location in instanceLocation.Locations)
            {
                SetAbstractValue(location, value);
            }
        }

        protected override void ResetValueTypeInstanceAnalysisData(AnalysisEntity analysisEntity)
        {
        }

        protected override void ResetReferenceTypeInstanceAnalysisData(PointsToAbstractValue pointsToAbstractValue)
        {
        }

        protected void ResetCurrentAnalysisData(IDictionary<AbstractLocation, TAbstractAnalysisValue> currentAnalysisData, IDictionary<AbstractLocation, TAbstractAnalysisValue> newAnalysisDataOpt)
        {
            // Reset the current analysis data, while ensuring that we don't violate the monotonicity, i.e. we cannot remove any existing key from currentAnalysisData.
            if (newAnalysisDataOpt == null)
            {
                // Just set the values for existing keys to ValueDomain.UnknownOrMayBeValue.
                foreach (var key in currentAnalysisData.Keys)
                {
                    SetAbstractValue(key, ValueDomain.UnknownOrMayBeValue);
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

        protected virtual TAbstractAnalysisValue HandleInstanceCreation(ITypeSymbol instanceType, PointsToAbstractValue instanceLocation, TAbstractAnalysisValue defaultValue)
        {
            SetAbstractValue(instanceLocation, defaultValue);
            return defaultValue;
        }

        protected override TAbstractAnalysisValue ComputeAnalysisValueForOutArgument(IArgumentOperation operation, TAbstractAnalysisValue defaultValue)
        {
            if (operation.Value.Type != null)
            {
                PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
                return HandleInstanceCreation(operation.Value.Type, instanceLocation, defaultValue);
            }

            return defaultValue;
        }

        protected abstract void SetValueForParameterPointsToLocationOnEntry(IParameterSymbol parameter, PointsToAbstractValue pointsToAbstractValue);
        protected abstract void SetValueForParameterPointsToLocationOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity, PointsToAbstractValue pointsToAbstractValue);

        protected override void SetValueForParameterOnEntry(IParameterSymbol parameter, AnalysisEntity analysisEntity)
        {
            Debug.Assert(analysisEntity.SymbolOpt == parameter);
            if (TryGetPointsToAbstractValueAtCurrentBlockExit(analysisEntity, out PointsToAbstractValue pointsToAbstractValue))
            {
                SetValueForParameterPointsToLocationOnEntry(parameter, pointsToAbstractValue);
            }
        }

        protected override void SetValueForParameterOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity)
        {
            Debug.Assert(analysisEntity.SymbolOpt == parameter);
            if (TryGetPointsToAbstractValueAtCurrentBlockEntry(analysisEntity, out PointsToAbstractValue pointsToAbstractValue))
            {
                SetValueForParameterPointsToLocationOnExit(parameter, analysisEntity, pointsToAbstractValue);
            }
        }

        /// <summary>
        /// Helper method to reset analysis data for analysis locations.
        /// If <paramref name="newAnalysisDataOpt"/> is null, all the analysis values in <paramref name="currentAnalysisDataOpt"/> are set to <see cref="ValueDomain.UnknownOrMayBeValue"/>.
        /// Otherwise, all the key-value paris in <paramref name="newAnalysisDataOpt"/> are transfered to <paramref name="currentAnalysisDataOpt"/> and keys in <paramref name="currentAnalysisDataOpt"/> which
        /// are not present in <paramref name="newAnalysisDataOpt"/> are set to <see cref="ValueDomain.UnknownOrMayBeValue"/>.
        /// </summary>
        protected void ResetAnalysisData(IDictionary<AbstractLocation, TAbstractAnalysisValue> currentAnalysisDataOpt, IDictionary<AbstractLocation, TAbstractAnalysisValue> newAnalysisDataOpt)
        {
            // Reset the current analysis data, while ensuring that we don't violate the monotonicity, i.e. we cannot remove any existing key from currentAnalysisData.
            if (newAnalysisDataOpt == null)
            {
                // Just set the values for existing keys to ValueDomain.UnknownOrMayBeValue.
                var keys = currentAnalysisDataOpt?.Keys.ToImmutableArray();
                foreach (var key in keys)
                {
                    SetAbstractValue(key, ValueDomain.UnknownOrMayBeValue);
                }
            }
            else
            {
                // Merge the values from current and new analysis data.
                var keys = currentAnalysisDataOpt?.Keys.Concat(newAnalysisDataOpt.Keys).ToImmutableHashSet();
                foreach (var key in keys)
                {
                    var value1 = currentAnalysisDataOpt != null && currentAnalysisDataOpt.TryGetValue(key, out var currentValue) ? currentValue : ValueDomain.Bottom;
                    var value2 = newAnalysisDataOpt.TryGetValue(key, out var newValue) ? newValue : ValueDomain.Bottom;
                    var mergedValue = ValueDomain.Merge(value1, value2);
                    SetAbstractValue(key, mergedValue);
                }
            }
        }

        // TODO: Remove these temporary methods once we move to compiler's CFG
        // https://github.com/dotnet/roslyn-analyzers/issues/1567
        #region Temporary methods to workaround lack of *real* CFG
        protected IDictionary<AbstractLocation, TAbstractAnalysisValue> GetClonedAnalysisDataHelper(IDictionary<AbstractLocation, TAbstractAnalysisValue> analysisData)
            => new Dictionary<AbstractLocation, TAbstractAnalysisValue>(analysisData);
        #endregion

        #region Visitor methods

        public override TAbstractAnalysisValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
        {
            var value = base.VisitObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation.Type, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, object argument)
        {
            var value = base.VisitTypeParameterObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation.Type, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, object argument)
        {
            var value = base.VisitDynamicObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation.Type, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, object argument)
        {
            var value = base.VisitAnonymousObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation.Type, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitArrayCreation(IArrayCreationOperation operation, object argument)
        {
            var value = base.VisitArrayCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation.Type, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
        {
            var value = base.VisitDelegateCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation.Type, instanceLocation, value);
        }

        #endregion
    }
}
