// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    internal partial class ThreadDependencyAnalysis
    {
        internal new class OperationVisitor : DataFlowOperationVisitor<Data, Context, Result, Value>
        {
            public OperationVisitor(Context analysisContext)
                : base(analysisContext)
            {
            }

            public override Data GetEmptyAnalysisData()
            {
                throw new NotImplementedException();
            }

            protected override void ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(Data dataAtException, ThrownExceptionInfo throwBranchWithExceptionType)
            {
                throw new NotImplementedException();
            }

            protected override bool Equals(Data value1, Data value2)
            {
                throw new NotImplementedException();
            }

            protected override void EscapeValueForParameterOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                throw new NotImplementedException();
            }

            protected override Value GetAbstractDefaultValue(ITypeSymbol type)
            {
                throw new NotImplementedException();
            }

            protected override Data GetClonedAnalysisData(Data analysisData)
            {
                throw new NotImplementedException();
            }

            protected override Data GetExitBlockOutputData(Result analysisResult)
            {
                throw new NotImplementedException();
            }

            protected override bool HasAnyAbstractValue(Data data)
            {
                throw new NotImplementedException();
            }

            protected override Data MergeAnalysisData(Data value1, Data value2)
            {
                throw new NotImplementedException();
            }

            protected override void ResetCurrentAnalysisData()
            {
                throw new NotImplementedException();
            }

            protected override void ResetReferenceTypeInstanceAnalysisData(PointsToAbstractValue pointsToAbstractValue)
            {
                throw new NotImplementedException();
            }

            protected override void ResetValueTypeInstanceAnalysisData(AnalysisEntity analysisEntity)
            {
                throw new NotImplementedException();
            }

            protected override void SetAbstractValueForArrayElementInitializer(IArrayCreationOperation arrayCreation, ImmutableArray<AbstractIndex> indices, ITypeSymbol elementType, IOperation initializer, Value value)
            {
                throw new NotImplementedException();
            }

            protected override void SetAbstractValueForAssignment(IOperation target, IOperation assignedValueOperation, Value assignedValue, bool mayBeAssignment = false)
            {
                throw new NotImplementedException();
            }

            protected override void SetAbstractValueForTupleElementAssignment(AnalysisEntity tupleElementEntity, IOperation assignedValueOperation, Value assignedValue)
            {
                throw new NotImplementedException();
            }

            protected override void SetValueForParameterOnEntry(IParameterSymbol parameter, AnalysisEntity analysisEntity, ArgumentInfo<Value> assignedValueOpt)
            {
                throw new NotImplementedException();
            }

            protected override void StopTrackingDataForParameter(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                throw new NotImplementedException();
            }

            protected override void UpdateValuesForAnalysisData(Data targetAnalysisData)
            {
                throw new NotImplementedException();
            }
        }
    }
}
