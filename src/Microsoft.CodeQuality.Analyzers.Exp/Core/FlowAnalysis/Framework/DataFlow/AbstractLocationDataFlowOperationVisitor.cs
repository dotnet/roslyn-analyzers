// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
            INamedTypeSymbol containingTypeSymbol,
            DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt,
            DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResultOpt)
            : base(valueDomain, containingTypeSymbol, nullAnalysisResultOpt, pointsToAnalysisResultOpt)
        {
        }

        protected abstract TAbstractAnalysisValue GetAbstractValue(AbstractLocation location);
        protected abstract void SetAbstractValue(AbstractLocation location, TAbstractAnalysisValue value);
        protected abstract void SetAbstractValue(PointsToAbstractValue location, TAbstractAnalysisValue value);

        protected override void ResetValueTypeInstanceAnalysisData(IOperation operation)
        {
        }

        protected override void ResetReferenceTypeInstanceAnalysisData(IOperation operation)
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

        protected virtual TAbstractAnalysisValue HandleInstanceCreation(IOperation operation, PointsToAbstractValue instanceLocation, TAbstractAnalysisValue defaultValue)
        {
            SetAbstractValue(instanceLocation, defaultValue);
            return defaultValue;
        }

        #region Visitor methods

        public override TAbstractAnalysisValue VisitObjectCreation(IObjectCreationOperation operation, object argument)
        {
            var value = base.VisitObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, object argument)
        {
            var value = base.VisitTypeParameterObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, object argument)
        {
            var value = base.VisitDynamicObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, object argument)
        {
            var value = base.VisitAnonymousObjectCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitArrayCreation(IArrayCreationOperation operation, object argument)
        {
            var value = base.VisitArrayCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation, instanceLocation, value);
        }

        public override TAbstractAnalysisValue VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
        {
            var value = base.VisitDelegateCreation(operation, argument);
            PointsToAbstractValue instanceLocation = GetPointsToAbstractValue(operation);
            return HandleInstanceCreation(operation, instanceLocation, value);
        }

        #endregion
    }
}
