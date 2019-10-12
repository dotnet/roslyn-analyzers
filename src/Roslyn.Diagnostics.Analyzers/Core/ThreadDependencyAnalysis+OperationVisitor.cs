// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Roslyn.Diagnostics.Analyzers
{
    internal partial class ThreadDependencyAnalysis
    {
        internal new class OperationVisitor : DataFlowOperationVisitor<Data, Context, Result, Value>
        {
            protected override bool IsPointsToAnalysis { get => base.IsPointsToAnalysis; }

            public OperationVisitor(Context analysisContext)
                : base(analysisContext)
            {
            }

            public override Data GetEmptyAnalysisData()
            {
                return new Data(YieldKind.NotYielded);
            }

            protected override void ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(Data dataAtException, ThrownExceptionInfo throwBranchWithExceptionType)
            {
                throw new NotImplementedException();
            }

            protected override bool Equals(Data value1, Data value2)
            {
                return value1.Equals(value2);
            }

            protected override void UpdateReachability(BasicBlock basicBlock, Data analysisData, bool isReachable)
            {
                // TODO: What is this method supposed to do?
            }

            protected override bool IsReachableBlockData(Data analysisData)
            {
                // TODO: What is this method supposed to do?
                return true;
            }

            protected override void EscapeValueForParameterOnExit(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                // TODO: What is this method supposed to do?
            }

            protected override Value GetAbstractDefaultValue(ITypeSymbol type)
            {
                // TODO: What is this method supposed to do?
                return ValueDomain.UnknownOrMayBeValue;
            }

            protected override Data GetClonedAnalysisData(Data analysisData)
            {
                return new Data(analysisData);
            }

            protected override Data GetExitBlockOutputData(Result analysisResult)
            {
                throw new NotImplementedException();
            }

            protected override bool HasAnyAbstractValue(Data data)
            {
                // TODO: What is this method supposed to do?
                return data.YieldKind != YieldKind.Unknown;
            }

            protected override Data MergeAnalysisData(Data value1, Data value2)
            {
                return new Data(ThreadDependencyAnalysis.ValueDomain.Merge(value1.YieldKind, value2.YieldKind));
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
                // TODO: What is this method supposed to do?
            }

            protected override void StopTrackingDataForParameter(IParameterSymbol parameter, AnalysisEntity analysisEntity)
            {
                throw new NotImplementedException();
            }

            protected override void UpdateValuesForAnalysisData(Data targetAnalysisData)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return base.ToString();
            }

            public override Value VisitExpressionStatement(IExpressionStatementOperation operation, object argument)
            {
                return base.VisitExpressionStatement(operation, argument);
            }

            public override Value VisitStop(IStopOperation operation, object argument)
            {
                return base.VisitStop(operation, argument);
            }

            public override Value VisitOmittedArgument(IOmittedArgumentOperation operation, object argument)
            {
                return base.VisitOmittedArgument(operation, argument);
            }

            public override Value VisitInstanceReference(IInstanceReferenceOperation operation, object argument)
            {
                return base.VisitInstanceReference(operation, argument);
            }

            public override Value VisitEventAssignment(IEventAssignmentOperation operation, object argument)
            {
                return base.VisitEventAssignment(operation, argument);
            }

            public override Value VisitTupleBinaryOperator(ITupleBinaryOperation operation, object argument)
            {
                return base.VisitTupleBinaryOperator(operation, argument);
            }

            public override Value VisitCoalesceAssignment(ICoalesceAssignmentOperation operation, object argument)
            {
                return base.VisitCoalesceAssignment(operation, argument);
            }

            public override Value VisitIsType(IIsTypeOperation operation, object argument)
            {
                return base.VisitIsType(operation, argument);
            }

            public override Value VisitSizeOf(ISizeOfOperation operation, object argument)
            {
                return base.VisitSizeOf(operation, argument);
            }

            public override Value VisitTypeOf(ITypeOfOperation operation, object argument)
            {
                return base.VisitTypeOf(operation, argument);
            }

            public override Value VisitDelegateCreation(IDelegateCreationOperation operation, object argument)
            {
                return base.VisitDelegateCreation(operation, argument);
            }

            public override Value VisitLiteral(ILiteralOperation operation, object argument)
            {
                return base.VisitLiteral(operation, argument);
            }

            public override Value VisitAddressOf(IAddressOfOperation operation, object argument)
            {
                return base.VisitAddressOf(operation, argument);
            }

            public override Value VisitDynamicObjectCreation(IDynamicObjectCreationOperation operation, object argument)
            {
                return base.VisitDynamicObjectCreation(operation, argument);
            }

            public override Value VisitDynamicInvocation(IDynamicInvocationOperation operation, object argument)
            {
                return base.VisitDynamicInvocation(operation, argument);
            }

            public override Value VisitDynamicIndexerAccess(IDynamicIndexerAccessOperation operation, object argument)
            {
                return base.VisitDynamicIndexerAccess(operation, argument);
            }

            public override Value VisitArrayCreation(IArrayCreationOperation operation, object argument)
            {
                return base.VisitArrayCreation(operation, argument);
            }

            public override Value VisitDeclarationExpression(IDeclarationExpressionOperation operation, object argument)
            {
                return base.VisitDeclarationExpression(operation, argument);
            }

            public override Value VisitTypeParameterObjectCreation(ITypeParameterObjectCreationOperation operation, object argument)
            {
                return base.VisitTypeParameterObjectCreation(operation, argument);
            }

            public override Value VisitInvalid(IInvalidOperation operation, object argument)
            {
                return base.VisitInvalid(operation, argument);
            }

            public override Value VisitInterpolatedString(IInterpolatedStringOperation operation, object argument)
            {
                return base.VisitInterpolatedString(operation, argument);
            }

            public override Value VisitDeclarationPattern(IDeclarationPatternOperation operation, object argument)
            {
                return base.VisitDeclarationPattern(operation, argument);
            }

            public override Value VisitRaiseEvent(IRaiseEventOperation operation, object argument)
            {
                return base.VisitRaiseEvent(operation, argument);
            }

            public override Value VisitMethodBodyOperation(IMethodBodyOperation operation, object argument)
            {
                return base.VisitMethodBodyOperation(operation, argument);
            }

            public override Value VisitConstructorBodyOperation(IConstructorBodyOperation operation, object argument)
            {
                return base.VisitConstructorBodyOperation(operation, argument);
            }

            public override Value VisitDiscardOperation(IDiscardOperation operation, object argument)
            {
                return base.VisitDiscardOperation(operation, argument);
            }

            public override Value VisitDiscardPattern(IDiscardPatternOperation operation, object argument)
            {
                return base.VisitDiscardPattern(operation, argument);
            }

            public override Value VisitSwitchExpression(ISwitchExpressionOperation operation, object argument)
            {
                return base.VisitSwitchExpression(operation, argument);
            }

            public override Value VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation, object argument)
            {
                return base.VisitSwitchExpressionArm(operation, argument);
            }

            public override Value VisitRangeOperation(IRangeOperation operation, object argument)
            {
                return base.VisitRangeOperation(operation, argument);
            }

            public override Value VisitReDim(IReDimOperation operation, object argument)
            {
                return base.VisitReDim(operation, argument);
            }

            public override Value VisitReDimClause(IReDimClauseOperation operation, object argument)
            {
                return base.VisitReDimClause(operation, argument);
            }

            protected override Value GetAbstractDefaultValueForCatchVariable(ICatchClauseOperation catchClause)
            {
                return base.GetAbstractDefaultValueForCatchVariable(catchClause);
            }

            public override (Value Value, PredicateValueKind PredicateValueKind)? GetReturnValueAndPredicateKind()
            {
                return base.GetReturnValueAndPredicateKind();
            }

            public override Data Flow(IOperation statement, BasicBlock block, Data input)
            {
                return base.Flow(statement, block, input);
            }

            protected override void StopTrackingDataForParameters(ImmutableDictionary<IParameterSymbol, AnalysisEntity> parameterEntities)
            {
                base.StopTrackingDataForParameters(parameterEntities);
            }

            public override (Data output, bool isFeasibleBranch) FlowBranch(BasicBlock fromBlock, BranchWithInfo branch, Data input)
            {
                return base.FlowBranch(fromBlock, branch, input);
            }

            private protected override Value GetAbstractValueForImplicitWrappingTaskCreation(IOperation returnValueOperation, Value returnValue, PointsToAbstractValue implicitTaskPointsToValue)
            {
                return base.GetAbstractValueForImplicitWrappingTaskCreation(returnValueOperation, returnValue, implicitTaskPointsToValue);
            }

            protected override void ProcessReturnValue(IOperation returnValueOperation)
            {
                base.ProcessReturnValue(returnValueOperation);
            }

            protected override void HandlePossibleThrowingOperation(IOperation operation)
            {
                base.HandlePossibleThrowingOperation(operation);
            }

            protected override Data GetMergedAnalysisDataForPossibleThrowingOperation(Data existingDataOpt, IOperation operation)
            {
                return base.GetMergedAnalysisDataForPossibleThrowingOperation(existingDataOpt, operation);
            }

            protected override void ProcessOutOfScopeLocalsAndFlowCaptures(IEnumerable<ILocalSymbol> locals, IEnumerable<CaptureId> flowCaptures)
            {
                base.ProcessOutOfScopeLocalsAndFlowCaptures(locals, flowCaptures);
            }

            public override Data GetMergedDataForUnhandledThrowOperations()
            {
                return base.GetMergedDataForUnhandledThrowOperations();
            }

            protected override CopyAbstractValue GetCopyAbstractValue(IOperation operation)
            {
                return base.GetCopyAbstractValue(operation);
            }

            protected override PointsToAbstractValue GetPointsToAbstractValue(IOperation operation)
            {
                return base.GetPointsToAbstractValue(operation);
            }

            protected override ValueContentAbstractValue GetValueContentAbstractValue(IOperation operation)
            {
                return base.GetValueContentAbstractValue(operation);
            }

            protected override Value ComputeAnalysisValueForReferenceOperation(IOperation operation, Value defaultValue)
            {
                return base.ComputeAnalysisValueForReferenceOperation(operation, defaultValue);
            }

            protected override Value ComputeAnalysisValueForEscapedRefOrOutArgument(IArgumentOperation operation, Value defaultValue)
            {
                return base.ComputeAnalysisValueForEscapedRefOrOutArgument(operation, defaultValue);
            }

            protected override void SetPredicateValueKind(IOperation operation, Data analysisData, PredicateValueKind predicateValueKind)
            {
                base.SetPredicateValueKind(operation, analysisData, predicateValueKind);
            }

            protected override PredicateValueKind SetValueForComparisonOperator(IBinaryOperation operation, Data targetAnalysisData)
            {
                return base.SetValueForComparisonOperator(operation, targetAnalysisData);
            }

            protected override PredicateValueKind SetValueForEqualsOrNotEqualsComparisonOperator(IOperation leftOperand, IOperation rightOperand, bool equals, bool isReferenceEquality, Data targetAnalysisData)
            {
                return base.SetValueForEqualsOrNotEqualsComparisonOperator(leftOperand, rightOperand, equals, isReferenceEquality, targetAnalysisData);
            }

            protected override PredicateValueKind SetValueForIsNullComparisonOperator(IOperation leftOperand, bool equals, Data targetAnalysisData)
            {
                return base.SetValueForIsNullComparisonOperator(leftOperand, equals, targetAnalysisData);
            }

            protected override void StartTrackingPredicatedData(AnalysisEntity predicatedEntity, Data truePredicateData, Data falsePredicateData)
            {
                base.StartTrackingPredicatedData(predicatedEntity, truePredicateData, falsePredicateData);
            }

            protected override void StopTrackingPredicatedData(AnalysisEntity predicatedEntity)
            {
                base.StopTrackingPredicatedData(predicatedEntity);
            }

            protected override bool HasPredicatedDataForEntity(Data analysisData, AnalysisEntity predicatedEntity)
            {
                return base.HasPredicatedDataForEntity(analysisData, predicatedEntity);
            }

            protected override void TransferPredicatedData(AnalysisEntity fromEntity, AnalysisEntity toEntity)
            {
                base.TransferPredicatedData(fromEntity, toEntity);
            }

            protected override PredicateValueKind ApplyPredicatedDataForEntity(Data analysisData, AnalysisEntity predicatedEntity, bool trueData)
            {
                return base.ApplyPredicatedDataForEntity(analysisData, predicatedEntity, trueData);
            }

            protected override void ProcessThrowValue(IOperation thrownValueOpt)
            {
                base.ProcessThrowValue(thrownValueOpt);
            }

            protected override Data MergeAnalysisDataForBackEdge(Data value1, Data value2)
            {
                return base.MergeAnalysisDataForBackEdge(value1, value2);
            }

            protected override void AssertValidAnalysisData(Data analysisData)
            {
                base.AssertValidAnalysisData(analysisData);
            }

            protected override Data GetInitialInterproceduralAnalysisData(IMethodSymbol invokedMethod, (AnalysisEntity InstanceOpt, PointsToAbstractValue PointsToValue)? invocationInstanceOpt, (AnalysisEntity Instance, PointsToAbstractValue PointsToValue)? thisOrMeInstanceForCallerOpt, ImmutableDictionary<IParameterSymbol, ArgumentInfo<Value>> argumentValuesMap, IDictionary<AnalysisEntity, PointsToAbstractValue> pointsToValuesOpt, IDictionary<AnalysisEntity, CopyAbstractValue> copyValuesOpt, IDictionary<AnalysisEntity, ValueContentAbstractValue> valueContentValuesOpt, bool isLambdaOrLocalFunction, bool hasParameterWithDelegateType)
            {
                return base.GetInitialInterproceduralAnalysisData(invokedMethod, invocationInstanceOpt, thisOrMeInstanceForCallerOpt, argumentValuesMap, pointsToValuesOpt, copyValuesOpt, valueContentValuesOpt, isLambdaOrLocalFunction, hasParameterWithDelegateType);
            }

            protected override void ApplyInterproceduralAnalysisResult(Data resultData, bool isLambdaOrLocalFunction, bool hasDelegateTypeArgument, Result analysisResult)
            {
                base.ApplyInterproceduralAnalysisResult(resultData, isLambdaOrLocalFunction, hasDelegateTypeArgument, analysisResult);
            }

            public override Value Visit(IOperation operation, object argument)
            {
                var result = base.Visit(operation, argument);
                CurrentAnalysisData.YieldKind = result.YieldKind;
                return result;
            }

            public override Value DefaultVisit(IOperation operation, object argument)
            {
                var result = base.DefaultVisit(operation, argument);
                return new Value(ThreadDependencyAnalysis.ValueDomain.Merge(CurrentAnalysisData.YieldKind, result.YieldKind), result.AlwaysComplete);
            }

            public override Value VisitSimpleAssignment(ISimpleAssignmentOperation operation, object argument)
            {
                return base.VisitSimpleAssignment(operation, argument);
            }

            public override Value VisitCompoundAssignment(ICompoundAssignmentOperation operation, object argument)
            {
                return base.VisitCompoundAssignment(operation, argument);
            }

            public override Value ComputeValueForCompoundAssignment(ICompoundAssignmentOperation operation, Value targetValue, Value assignedValue, ITypeSymbol targetType, ITypeSymbol assignedValueType)
            {
                return base.ComputeValueForCompoundAssignment(operation, targetValue, assignedValue, targetType, assignedValueType);
            }

            public override Value VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, object argument)
            {
                return base.VisitIncrementOrDecrement(operation, argument);
            }

            public override Value ComputeValueForIncrementOrDecrementOperation(IIncrementOrDecrementOperation operation, Value targetValue)
            {
                return base.ComputeValueForIncrementOrDecrementOperation(operation, targetValue);
            }

            public override Value VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, object argument)
            {
                return base.VisitDeconstructionAssignment(operation, argument);
            }

            protected override Value VisitAssignmentOperation(IAssignmentOperation operation, object argument)
            {
                return base.VisitAssignmentOperation(operation, argument);
            }

            public override Value VisitArrayInitializer(IArrayInitializerOperation operation, object argument)
            {
                return base.VisitArrayInitializer(operation, argument);
            }

            public override Value VisitLocalReference(ILocalReferenceOperation operation, object argument)
            {
                return base.VisitLocalReference(operation, argument);
            }

            public override Value VisitParameterReference(IParameterReferenceOperation operation, object argument)
            {
                return base.VisitParameterReference(operation, argument);
            }

            public override Value VisitArrayElementReference(IArrayElementReferenceOperation operation, object argument)
            {
                return base.VisitArrayElementReference(operation, argument);
            }

            public override Value VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, object argument)
            {
                return base.VisitDynamicMemberReference(operation, argument);
            }

            public override Value VisitEventReference(IEventReferenceOperation operation, object argument)
            {
                return base.VisitEventReference(operation, argument);
            }

            public override Value VisitFieldReference(IFieldReferenceOperation operation, object argument)
            {
                return base.VisitFieldReference(operation, argument);
            }

            public override Value VisitMethodReference(IMethodReferenceOperation operation, object argument)
            {
                return base.VisitMethodReference(operation, argument);
            }

            public override Value VisitPropertyReference(IPropertyReferenceOperation operation, object argument)
            {
                return base.VisitPropertyReference(operation, argument);
            }

            public override Value VisitFlowCaptureReference(IFlowCaptureReferenceOperation operation, object argument)
            {
                return base.VisitFlowCaptureReference(operation, argument);
            }

            public override Value VisitFlowCapture(IFlowCaptureOperation operation, object argument)
            {
                return base.VisitFlowCapture(operation, argument);
            }

            public override Value VisitDefaultValue(IDefaultValueOperation operation, object argument)
            {
                return base.VisitDefaultValue(operation, argument);
            }

            public override Value VisitInterpolation(IInterpolationOperation operation, object argument)
            {
                return base.VisitInterpolation(operation, argument);
            }

            public override Value VisitInterpolatedStringText(IInterpolatedStringTextOperation operation, object argument)
            {
                return base.VisitInterpolatedStringText(operation, argument);
            }

            protected override void PostProcessArgument(IArgumentOperation operation, bool isEscaped)
            {
                base.PostProcessArgument(operation, isEscaped);
            }

            public override Value VisitConstantPattern(IConstantPatternOperation operation, object argument)
            {
                return base.VisitConstantPattern(operation, argument);
            }

            public override Value VisitParenthesized(IParenthesizedOperation operation, object argument)
            {
                return base.VisitParenthesized(operation, argument);
            }

            public override Value VisitTranslatedQuery(ITranslatedQueryOperation operation, object argument)
            {
                return base.VisitTranslatedQuery(operation, argument);
            }

            public override Value VisitConversion(IConversionOperation operation, object argument)
            {
                return base.VisitConversion(operation, argument);
            }

            public override Value VisitObjectCreation(IObjectCreationOperation operation, object argument)
            {
                return base.VisitObjectCreation(operation, argument);
            }

            public override Value VisitInvocation_NonLambdaOrDelegateOrLocalFunction(IMethodSymbol method, IOperation visitedInstance, ImmutableArray<IArgumentOperation> visitedArguments, bool invokedAsDelegate, IOperation originalOperation, Value defaultValue)
            {
                var result = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(method, visitedInstance, visitedArguments, invokedAsDelegate, originalOperation, defaultValue);
                var threadDependencyInfoForReturn = AbstractThreadDependencyAnalyzer.GetThreadDependencyInfoForReturn(WellKnownTypeProvider, method);
                if (threadDependencyInfoForReturn.AlwaysCompleted)
                {
                    return new Value(result.YieldKind, alwaysComplete: true);
                }

                return result;
            }

            public override Value VisitInvocation_LocalFunction(IMethodSymbol localFunction, ImmutableArray<IArgumentOperation> visitedArguments, IOperation originalOperation, Value defaultValue)
            {
                return base.VisitInvocation_LocalFunction(localFunction, visitedArguments, originalOperation, defaultValue);
            }

            public override Value VisitInvocation_Lambda(IFlowAnonymousFunctionOperation lambda, ImmutableArray<IArgumentOperation> visitedArguments, IOperation originalOperation, Value defaultValue)
            {
                return base.VisitInvocation_Lambda(lambda, visitedArguments, originalOperation, defaultValue);
            }

            public override void HandleEnterLockOperation(IOperation lockedObject)
            {
                base.HandleEnterLockOperation(lockedObject);
            }

            public override Value VisitTuple(ITupleOperation operation, object argument)
            {
                return base.VisitTuple(operation, argument);
            }

            public override Value VisitUnaryOperatorCore(IUnaryOperation operation, object argument)
            {
                return base.VisitUnaryOperatorCore(operation, argument);
            }

            public override Value VisitBinaryOperatorCore(IBinaryOperation operation, object argument)
            {
                return base.VisitBinaryOperatorCore(operation, argument);
            }

            public override Value VisitIsNull(IIsNullOperation operation, object argument)
            {
                return base.VisitIsNull(operation, argument);
            }

            public override Value VisitCaughtException(ICaughtExceptionOperation operation, object argument)
            {
                return base.VisitCaughtException(operation, argument);
            }

            public override Value VisitFlowAnonymousFunction(IFlowAnonymousFunctionOperation operation, object argument)
            {
                return base.VisitFlowAnonymousFunction(operation, argument);
            }

            public override Value VisitStaticLocalInitializationSemaphore(IStaticLocalInitializationSemaphoreOperation operation, object argument)
            {
                return base.VisitStaticLocalInitializationSemaphore(operation, argument);
            }

            public override Value VisitAnonymousObjectCreation(IAnonymousObjectCreationOperation operation, object argument)
            {
                return base.VisitAnonymousObjectCreation(operation, argument);
            }

            public override Value GetAssignedValueForPattern(IIsPatternOperation operation, Value operandValue)
            {
                return base.GetAssignedValueForPattern(operation, operandValue);
            }

            public override Value VisitAwait(IAwaitOperation operation, object argument)
            {
                var result = base.VisitAwait(operation, argument);

                switch (result.YieldKind)
                {
                    case YieldKind.NotYielded when GetCachedAbstractValue(operation.Operation).AlwaysComplete == true:
                        return new Value(YieldKind.NotYielded, alwaysComplete: true);

                    case YieldKind.Unknown:
                    case YieldKind.NotYielded:
                    case YieldKind.MaybeYielded:
                        return new Value(YieldKind.MaybeYielded, alwaysComplete: false);

                    case YieldKind.Yielded:
                        return new Value(YieldKind.Yielded, alwaysComplete: false);

                    default:
                        throw new InvalidOperationException();
                }
            }

            public override Value VisitLock(ILockOperation operation, object argument)
            {
                return base.VisitLock(operation, argument);
            }
        }
    }
}
