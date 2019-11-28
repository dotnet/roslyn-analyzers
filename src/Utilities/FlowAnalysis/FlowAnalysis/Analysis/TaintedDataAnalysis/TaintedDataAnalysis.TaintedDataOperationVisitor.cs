// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    using ValueContentAnalysisResult = DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>;

    internal partial class TaintedDataAnalysis
    {
        private sealed class TaintedDataOperationVisitor : AnalysisEntityDataFlowOperationVisitor<TaintedDataAnalysisData, TaintedDataAnalysisContext, TaintedDataAnalysisResult, TaintedDataAbstractValue>
        {
            private readonly TaintedDataAnalysisDomain _taintedDataAnalysisDomain;

            /// <summary>
            /// Mapping of a tainted data sinks to their originating sources.
            /// </summary>
            /// <remarks>Keys are <see cref="SymbolAccess"/> sinks where the tainted data entered, values are <see cref="SymbolAccess"/>s where the tainted data originated from.</remarks>
            private Dictionary<SymbolAccess, (ImmutableHashSet<SinkKind>.Builder SinkKinds, ImmutableHashSet<SymbolAccess>.Builder SourceOrigins)> TaintedSourcesBySink { get; }

            public TaintedDataOperationVisitor(TaintedDataAnalysisDomain taintedDataAnalysisDomain, TaintedDataAnalysisContext analysisContext)
                : base(analysisContext)
            {
                _taintedDataAnalysisDomain = taintedDataAnalysisDomain;
                this.TaintedSourcesBySink = new Dictionary<SymbolAccess, (ImmutableHashSet<SinkKind>.Builder SinkKinds, ImmutableHashSet<SymbolAccess>.Builder SourceOrigins)>();
            }

            public ImmutableArray<TaintedDataSourceSink> GetTaintedDataSourceSinkEntries()
            {
                ImmutableArray<TaintedDataSourceSink>.Builder builder = ImmutableArray.CreateBuilder<TaintedDataSourceSink>();
                foreach (KeyValuePair<SymbolAccess, (ImmutableHashSet<SinkKind>.Builder SinkKinds, ImmutableHashSet<SymbolAccess>.Builder SourceOrigins)> kvp in this.TaintedSourcesBySink)
                {
                    builder.Add(
                        new TaintedDataSourceSink(
                            kvp.Key,
                            kvp.Value.SinkKinds.ToImmutable(),
                            kvp.Value.SourceOrigins.ToImmutable()));
                }

                return builder.ToImmutableArray();
            }

            protected override void AddTrackedEntities(TaintedDataAnalysisData analysisData, HashSet<AnalysisEntity> builder, bool forInterproceduralAnalysis)
                => analysisData.AddTrackedEntities(builder);

            protected override bool Equals(TaintedDataAnalysisData value1, TaintedDataAnalysisData value2)
            {
                return value1.Equals(value2);
            }

            protected override TaintedDataAbstractValue GetAbstractDefaultValue(ITypeSymbol type)
            {
                return TaintedDataAbstractValue.NotTainted;
            }

            protected override TaintedDataAbstractValue GetAbstractValue(AnalysisEntity analysisEntity)
            {
                return this.CurrentAnalysisData.TryGetValue(analysisEntity, out TaintedDataAbstractValue value) ? value : TaintedDataAbstractValue.NotTainted;
            }

            protected override TaintedDataAnalysisData GetClonedAnalysisData(TaintedDataAnalysisData analysisData)
            {
                return (TaintedDataAnalysisData)analysisData.Clone();
            }

            protected override bool HasAbstractValue(AnalysisEntity analysisEntity)
            {
                return this.CurrentAnalysisData.HasAbstractValue(analysisEntity);
            }

            protected override bool HasAnyAbstractValue(TaintedDataAnalysisData data)
            {
                return data.HasAnyAbstractValue;
            }

            protected override TaintedDataAnalysisData MergeAnalysisData(TaintedDataAnalysisData value1, TaintedDataAnalysisData value2)
            {
                return _taintedDataAnalysisDomain.Merge(value1, value2);
            }

            protected override void UpdateValuesForAnalysisData(TaintedDataAnalysisData targetAnalysisData)
            {
                UpdateValuesForAnalysisData(targetAnalysisData.CoreAnalysisData, CurrentAnalysisData.CoreAnalysisData);
            }

            protected override void ResetCurrentAnalysisData()
            {
                this.CurrentAnalysisData.Reset(this.ValueDomain.UnknownOrMayBeValue);
            }

            public override TaintedDataAnalysisData GetEmptyAnalysisData()
            {
                return new TaintedDataAnalysisData();
            }

            protected override TaintedDataAnalysisData GetExitBlockOutputData(TaintedDataAnalysisResult analysisResult)
            {
                return new TaintedDataAnalysisData(analysisResult.ExitBlockOutput.Data);
            }

            protected override void ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(TaintedDataAnalysisData dataAtException, ThrownExceptionInfo throwBranchWithExceptionType)
            {
                base.ApplyMissingCurrentAnalysisDataForUnhandledExceptionData(dataAtException.CoreAnalysisData, CurrentAnalysisData.CoreAnalysisData, throwBranchWithExceptionType);
            }

            protected override void SetAbstractValue(AnalysisEntity analysisEntity, TaintedDataAbstractValue value)
            {
                if (value.Kind == TaintedDataAbstractValueKind.Tainted
                    || this.CurrentAnalysisData.CoreAnalysisData.ContainsKey(analysisEntity))
                {
                    // Only track tainted data, or sanitized data.
                    // If it's new, and it's untainted, we don't care.
                    SetAbstractValueCore(CurrentAnalysisData, analysisEntity, value);
                }
            }

            private static void SetAbstractValueCore(TaintedDataAnalysisData taintedAnalysisData, AnalysisEntity analysisEntity, TaintedDataAbstractValue value)
                => taintedAnalysisData.SetAbstractValue(analysisEntity, value);

            protected override void ResetAbstractValue(AnalysisEntity analysisEntity)
            {
                this.SetAbstractValue(analysisEntity, ValueDomain.UnknownOrMayBeValue);
            }

            protected override void StopTrackingEntity(AnalysisEntity analysisEntity, TaintedDataAnalysisData analysisData)
            {
                analysisData.RemoveEntries(analysisEntity);
            }

            public override TaintedDataAbstractValue DefaultVisit(IOperation operation, object? argument)
            {
                // This handles most cases of tainted data flowing from child operations to parent operations.
                // Examples:
                // - tainted input parameters to method calls returns, and out/ref parameters, tainted (assuming no interprocedural)
                // - adding a tainted value to something makes the result tainted
                // - instantiating an object with tainted data makes the new object tainted

                List<TaintedDataAbstractValue>? taintedValues = null;
                foreach (IOperation childOperation in operation.Children)
                {
                    TaintedDataAbstractValue childValue = Visit(childOperation, argument);
                    if (childValue.Kind == TaintedDataAbstractValueKind.Tainted)
                    {
                        if (taintedValues == null)
                        {
                            taintedValues = new List<TaintedDataAbstractValue>();
                        }

                        taintedValues.Add(childValue);
                    }
                }

                if (taintedValues != null)
                {
                    if (taintedValues.Count == 1)
                    {
                        return taintedValues[0];
                    }
                    else
                    {
                        return TaintedDataAbstractValue.MergeTainted(taintedValues);
                    }
                }
                else
                {
                    return ValueDomain.UnknownOrMayBeValue;
                }
            }

            protected override TaintedDataAbstractValue ComputeAnalysisValueForReferenceOperation(IOperation operation, TaintedDataAbstractValue defaultValue)
            {
                // If the property reference itself is a tainted data source
                if (operation is IPropertyReferenceOperation propertyReferenceOperation
                    && this.DataFlowAnalysisContext.SourceInfos.IsSourceProperty(propertyReferenceOperation.Property))
                {
                    return TaintedDataAbstractValue.CreateTainted(propertyReferenceOperation.Member, propertyReferenceOperation.Syntax, this.OwningSymbol);
                }

                if (AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity? analysisEntity))
                {
                    return this.CurrentAnalysisData.TryGetValue(analysisEntity, out TaintedDataAbstractValue value) ? value : defaultValue;
                }

                return defaultValue;
            }

            // So we can hook into constructor calls.
            public override TaintedDataAbstractValue VisitObjectCreation(IObjectCreationOperation operation, object? argument)
            {
                TaintedDataAbstractValue baseValue = base.VisitObjectCreation(operation, argument);
                ProcessDataEnteringInvocationOrCreationSink(operation.Constructor, operation.Arguments, operation);
                return baseValue;
            }

            public override TaintedDataAbstractValue VisitInvocation_NonLambdaOrDelegateOrLocalFunction(
                IMethodSymbol method,
                IOperation? visitedInstance,
                ImmutableArray<IArgumentOperation> visitedArguments,
                bool invokedAsDelegate,
                IOperation originalOperation,
                TaintedDataAbstractValue defaultValue)
            {
                // Always invoke base visit.
                TaintedDataAbstractValue result = base.VisitInvocation_NonLambdaOrDelegateOrLocalFunction(
                    method,
                    visitedInstance,
                    visitedArguments,
                    invokedAsDelegate,
                    originalOperation,
                    defaultValue);
                ProcessDataEnteringInvocationOrCreationSink(method, visitedArguments, originalOperation);
                PooledHashSet<string>? taintedTargets = null;
                PooledHashSet<(string, string)>? taintedParameterPairs = null;
                PooledHashSet<string>? sanitizedArguments = null;
                try
                {
                    if (this.IsSanitizingMethod(method, out bool sanitizeReturn, out bool sanitizeInstance, out sanitizedArguments))
                    {
                        if (sanitizeReturn)
                        {
                            result = TaintedDataAbstractValue.NotTainted;
                        }

                        if (visitedInstance != null && sanitizeInstance)
                        {
                            result = TaintedDataAbstractValue.NotTainted;
                            CacheAbstractValueForBothOperationAndEntity(visitedInstance, result);
                        }

                        if (sanitizedArguments != null)
                        {
                            foreach (IArgumentOperation arg in visitedArguments)
                            {
                                result = TaintedDataAbstractValue.NotTainted;
                                CacheAbstractValueForBothOperationAndEntity(arg, result);
                            }
                        }
                    }
                    else if (this.DataFlowAnalysisContext.SourceInfos.IsSourceMethod(
                        method,
                        visitedArguments,
                        new Lazy<PointsToAnalysisResult?>(() => DataFlowAnalysisContext.PointsToAnalysisResultOpt),
                        new Lazy<(PointsToAnalysisResult?, ValueContentAnalysisResult?)>(() => (DataFlowAnalysisContext.PointsToAnalysisResultOpt, DataFlowAnalysisContext.ValueContentAnalysisResultOpt)),
                        out taintedTargets))
                    {
                        foreach (string taintedTarget in taintedTargets)
                        {
                            if (taintedTarget != TaintedTargetValue.Return)
                            {
                                IArgumentOperation argumentOperation = visitedArguments.FirstOrDefault(o => o.Parameter.Name == taintedTarget);
                                if (argumentOperation != null)
                                {
                                    this.CacheAbstractValue(argumentOperation, TaintedDataAbstractValue.CreateTainted(argumentOperation.Parameter, argumentOperation.Syntax, method));
                                }
                                else
                                {
                                    Debug.Fail("Are the tainted data sources misconfigured?");
                                }
                            }
                            else
                            {
                                result = TaintedDataAbstractValue.CreateTainted(method, originalOperation.Syntax, this.OwningSymbol);
                            }
                        }
                    }

                    if (this.DataFlowAnalysisContext.SourceInfos.IsSourceTransferMethod(
                        method,
                        visitedArguments,
                        visitedArguments
                            .Where(s => this.GetCachedAbstractValue(s).Kind == TaintedDataAbstractValueKind.Tainted)
                            .Select(s => s.Parameter.Name)
                            .ToImmutableArray(),
                        out taintedParameterPairs))
                    {
                        foreach ((string ifTaintedParameter, string thenTaintedTarget) in taintedParameterPairs)
                        {
                            if (thenTaintedTarget == TaintedTargetValue.Return)
                            {
                                result = TaintedDataAbstractValue.CreateTainted(method, originalOperation.Syntax, this.OwningSymbol);
                            }
                            else
                            {
                                IArgumentOperation thenTaintedTargetOperation = visitedArguments.FirstOrDefault(o => o.Parameter.Name == thenTaintedTarget);
                                if (thenTaintedTargetOperation != null)
                                {
                                    CacheAbstractValueForBothOperationAndEntity(
                                        thenTaintedTargetOperation,
                                        this.GetCachedAbstractValue(
                                            visitedArguments.FirstOrDefault(o => o.Parameter.Name == ifTaintedParameter)));
                                }
                                else
                                {
                                    Debug.Fail("Are the tainted data sources misconfigured?");
                                }
                            }
                        }
                    }
                }
                finally
                {
                    taintedTargets?.Free();
                    taintedParameterPairs?.Free();
                    sanitizedArguments?.Free();
                }

                return result;
            }

            public override TaintedDataAbstractValue VisitInvocation_LocalFunction(IMethodSymbol localFunction, ImmutableArray<IArgumentOperation> visitedArguments, IOperation originalOperation, TaintedDataAbstractValue defaultValue)
            {
                // Always invoke base visit.
                TaintedDataAbstractValue baseValue = base.VisitInvocation_LocalFunction(localFunction, visitedArguments, originalOperation, defaultValue);
                ProcessDataEnteringInvocationOrCreationSink(localFunction, visitedArguments, originalOperation);
                return baseValue;
            }

            /// <summary>
            /// Computes abstract value for out or ref arguments when not performing interprocedural analysis.
            /// </summary>
            /// <param name="analysisEntity">Analysis entity.</param>
            /// <param name="operation">IArgumentOperation.</param>
            /// <param name="defaultValue">Default TaintedDataAbstractValue if we don't need to override.</param>
            /// <returns>Abstract value of the output parameter.</returns>
            protected override TaintedDataAbstractValue ComputeAnalysisValueForEscapedRefOrOutArgument(
                AnalysisEntity analysisEntity,
                IArgumentOperation operation,
                TaintedDataAbstractValue defaultValue)
            {
                // Note this method is only called when interprocedural DFA is *NOT* performed.
                if (operation.Parent is IInvocationOperation invocationOperation)
                {
                    Debug.Assert(!this.TryGetInterproceduralAnalysisResult(invocationOperation, out TaintedDataAnalysisResult _));

                    // Treat ref or out arguments as the same as the invocation operation.
                    TaintedDataAbstractValue returnValueAbstractValue = this.GetCachedAbstractValue(invocationOperation);
                    return returnValueAbstractValue;
                }
                else
                {
                    return defaultValue;
                }
            }

            // So we can treat the array as tainted when it's passed to other object constructors.
            // See HttpRequest_Form_Array_List_Diagnostic and HttpRequest_Form_List_Diagnostic tests.
            public override TaintedDataAbstractValue VisitArrayInitializer(IArrayInitializerOperation operation, object? argument)
            {
                HashSet<SymbolAccess>? sourceOrigins = null;
                TaintedDataAbstractValue baseAbstractValue = base.VisitArrayInitializer(operation, argument);
                if (baseAbstractValue.Kind == TaintedDataAbstractValueKind.Tainted)
                {
                    sourceOrigins = new HashSet<SymbolAccess>(baseAbstractValue.SourceOrigins);
                }

                IEnumerable<TaintedDataAbstractValue> taintedAbstractValues =
                    operation.ElementValues
                        .Select<IOperation, TaintedDataAbstractValue>(e => this.GetCachedAbstractValue(e))
                        .Where(v => v.Kind == TaintedDataAbstractValueKind.Tainted);
                if (baseAbstractValue.Kind == TaintedDataAbstractValueKind.Tainted)
                {
                    taintedAbstractValues = taintedAbstractValues.Concat(baseAbstractValue);
                }

                TaintedDataAbstractValue? result = null;
                if (taintedAbstractValues.Any())
                {
                    result = TaintedDataAbstractValue.MergeTainted(taintedAbstractValues);
                }

                IArrayCreationOperation? arrayCreationOperation = operation.GetAncestor<IArrayCreationOperation>(OperationKind.ArrayCreation);
                if (arrayCreationOperation?.Type is IArrayTypeSymbol arrayTypeSymbol
                    && this.DataFlowAnalysisContext.SourceInfos.IsSourceArray(arrayTypeSymbol, out TaintArrayKind taintArrayKind)
                    && taintArrayKind == TaintArrayKind.Constant
                    && operation.ElementValues.All(s => GetValueContentAbstractValue(s).IsLiteralState))
                {
                    TaintedDataAbstractValue taintedDataAbstractValue = TaintedDataAbstractValue.CreateTainted(arrayTypeSymbol, arrayCreationOperation.Syntax, this.OwningSymbol);
                    result = result == null ? taintedDataAbstractValue : TaintedDataAbstractValue.MergeTainted(result, taintedDataAbstractValue);
                }

                if (result != null)
                {
                    return result;
                }
                else
                {
                    return baseAbstractValue;
                }
            }

            protected override TaintedDataAbstractValue VisitAssignmentOperation(IAssignmentOperation operation, object? argument)
            {
                TaintedDataAbstractValue taintedDataAbstractValue = base.VisitAssignmentOperation(operation, argument);
                ProcessAssignmentOperation(operation);
                return taintedDataAbstractValue;
            }

            private void TrackTaintedDataEnteringSink(
                ISymbol sinkSymbol,
                Location sinkLocation,
                IEnumerable<SinkKind> sinkKinds,
                IEnumerable<SymbolAccess> sources)
            {
                SymbolAccess sink = new SymbolAccess(sinkSymbol, sinkLocation, this.OwningSymbol);
                this.TrackTaintedDataEnteringSink(sink, sinkKinds, sources);
            }

            private void TrackTaintedDataEnteringSink(SymbolAccess sink, IEnumerable<SinkKind> sinkKinds, IEnumerable<SymbolAccess> sources)
            {
                if (!this.TaintedSourcesBySink.TryGetValue(sink, out (ImmutableHashSet<SinkKind>.Builder SinkKinds, ImmutableHashSet<SymbolAccess>.Builder SourceOrigins) data))
                {
                    data = (ImmutableHashSet.CreateBuilder<SinkKind>(), ImmutableHashSet.CreateBuilder<SymbolAccess>());
                    this.TaintedSourcesBySink.Add(sink, data);
                }

                data.SinkKinds.UnionWith(sinkKinds);
                data.SourceOrigins.UnionWith(sources);
            }

            /// <summary>
            /// Determines if the data is tainted and if it's entering a sink as a method call or constructor argument, and if so, flags it.
            /// </summary>
            /// <param name="targetMethod">Method being invoked.</param>
            /// <param name="visitedArguments">Arguments to the method.</param>
            /// <param name="originalOperation">Original IOperation for the method/constructor invocation.</param>
            private void ProcessDataEnteringInvocationOrCreationSink(
                IMethodSymbol targetMethod,
                ImmutableArray<IArgumentOperation> visitedArguments,
                IOperation originalOperation)
            {
                CheckArgumentsForTaint(targetMethod, visitedArguments, originalOperation);
                if (this.TryGetInterproceduralAnalysisResult(originalOperation, out TaintedDataAnalysisResult? subResult)
                    && !subResult.TaintedDataSourceSinks.IsEmpty)
                {
                    foreach (TaintedDataSourceSink sourceSink in subResult.TaintedDataSourceSinks)
                    {
                        if (!this.TaintedSourcesBySink.TryGetValue(
                                sourceSink.Sink,
                                out (ImmutableHashSet<SinkKind>.Builder SinkKinds, ImmutableHashSet<SymbolAccess>.Builder SourceOrigins) data))
                        {
                            data = (ImmutableHashSet.CreateBuilder<SinkKind>(), ImmutableHashSet.CreateBuilder<SymbolAccess>());
                            this.TaintedSourcesBySink.Add(sourceSink.Sink, data);
                        }

                        data.SinkKinds.UnionWith(sourceSink.SinkKinds);
                        data.SourceOrigins.UnionWith(sourceSink.SourceOrigins);
                    }
                }
            }

            private void ProcessAssignmentOperation(IAssignmentOperation assignmentOperation)
            {
                if (assignmentOperation.Target != null
                    && assignmentOperation.Target is IPropertyReferenceOperation propertyReferenceOperation
                    && this.IsPropertyASink(propertyReferenceOperation, out HashSet<SinkKind>? sinkKinds))
                {
                    IOperation value = assignmentOperation.Value;
                    UpdateAbstractValueForArrayBeforeEnteringSinkForArray(assignmentOperation, value);
                    TaintedDataAbstractValue assignmentValueAbstractValue = GetCachedAbstractValue(value);
                    if (assignmentValueAbstractValue.Kind == TaintedDataAbstractValueKind.Tainted)
                    {
                        this.TrackTaintedDataEnteringSink(
                            propertyReferenceOperation.Member,
                            propertyReferenceOperation.Syntax.GetLocation(),
                            sinkKinds,
                            assignmentValueAbstractValue.SourceOrigins);
                    }
                }
            }

            /// <summary>
            /// Determines if the instance method call returns tainted data.
            /// </summary>
            /// <param name="method">Instance method being called.</param>
            /// <returns>True if the method returns tainted data, false otherwise.</returns>
            private bool IsSanitizingMethod(IMethodSymbol method, out bool sanitizeReturn, out bool sanitizeInstance, out PooledHashSet<string>? sanitizedArguments)
            {
                sanitizeReturn = false;
                sanitizeInstance = false;
                sanitizedArguments = null;
                foreach (SanitizerInfo sanitizerInfo in this.DataFlowAnalysisContext.SanitizerInfos.GetInfosForType(method.ContainingType))
                {
                    if (method.MethodKind == MethodKind.Constructor
                        && sanitizerInfo.IsConstructorSanitizing)
                    {
                        sanitizeReturn = true;
                    }
                    else if (sanitizerInfo.SanitizingMethods.TryGetValue(method.MetadataName, out (bool SanitizeReturn, bool SanitizeInstance, ImmutableHashSet<string> SanitizedArguments) sanitizingTargets))
                    {
                        if (sanitizingTargets.SanitizeReturn)
                        {
                            sanitizeReturn = true;
                        }

                        if (sanitizingTargets.SanitizeInstance)
                        {
                            sanitizeInstance = true;
                        }

                        if (!sanitizingTargets.SanitizedArguments.IsEmpty)
                        {
                            if (sanitizedArguments == null)
                            {
                                sanitizedArguments = PooledHashSet<string>.GetInstance();
                            }

                            sanitizedArguments.Union(sanitizingTargets.SanitizedArguments);
                        }
                    }
                }

                return sanitizeReturn || sanitizeInstance || sanitizedArguments != null;
            }

            /// <summary>
            /// If data is tainted and passed as arguments to a method which is a sink, track it.
            /// </summary>
            /// <param name="method">Method being invoked.</param>
            /// <param name="visitedArguments">Arguments to the method.</param>
            private void CheckArgumentsForTaint(IMethodSymbol method, ImmutableArray<IArgumentOperation> visitedArguments, IOperation originalOperation)
            {
                IEnumerable<IArgumentOperation>? taintedArguments = null;
                foreach (SinkInfo sinkInfo in this.DataFlowAnalysisContext.SinkInfos.GetInfosForType(method.ContainingType))
                {
                    taintedArguments ??= GetTaintedArguments(visitedArguments);
                    if (taintedArguments == null)
                    {
                        return;
                    }

                    foreach (IArgumentOperation taintedArgument in taintedArguments)
                    {
                        if ((method.MethodKind == MethodKind.Constructor
                                && sinkInfo.IsAnyStringParameterInConstructorASink
                                && taintedArguments.Any(a => a.Parameter.Type.SpecialType == SpecialType.System_String))
                            || (sinkInfo.SinkMethodParameters.TryGetValue(method.MetadataName, out ImmutableHashSet<string> sinkParameters)
                                && taintedArguments.Any(a => sinkParameters.Contains(a.Parameter.MetadataName)))
                            || (originalOperation is IInvocationOperation invocationOperation
                                && invocationOperation.Instance != null
                                && this.GetCachedAbstractValue(invocationOperation.Instance).Kind == TaintedDataAbstractValueKind.Tainted
                                && sinkInfo.SinkMethodParametersWithTaintedInstance.TryGetValue(method.MetadataName, out ImmutableHashSet<string> sinkMethodParametersWithTaintedInstance)
                                && taintedArguments.Any(a => sinkMethodParametersWithTaintedInstance.Contains(a.Parameter.MetadataName))))
                        {
                            TaintedDataAbstractValue abstractValue = this.GetCachedAbstractValue(taintedArgument);
                            this.TrackTaintedDataEnteringSink(method, originalOperation.Syntax.GetLocation(), sinkInfo.SinkKinds, abstractValue.SourceOrigins);
                        }
                    }
                }
            }

            /// <summary>
            /// Determines if a property is a sink.
            /// </summary>
            /// <param name="propertyReferenceOperation">Property to check if it's a sink.</param>
            /// <param name="sinkKinds">If the property is a sink, <see cref="HashSet{SinkInfo}"/> containing the kinds of sinks; null otherwise.</param>
            /// <returns>True if the property is a sink, false otherwise.</returns>
            private bool IsPropertyASink(IPropertyReferenceOperation propertyReferenceOperation, [NotNullWhen(returnValue: true)] out HashSet<SinkKind>? sinkKinds)
            {
                Lazy<HashSet<SinkKind>> lazySinkKinds = new Lazy<HashSet<SinkKind>>(() => new HashSet<SinkKind>());
                foreach (SinkInfo sinkInfo in this.DataFlowAnalysisContext.SinkInfos.GetInfosForType(propertyReferenceOperation.Member.ContainingType))
                {
                    if (lazySinkKinds.IsValueCreated && lazySinkKinds.Value.IsSupersetOf(sinkInfo.SinkKinds))
                    {
                        continue;
                    }

                    if (sinkInfo.SinkProperties.Contains(propertyReferenceOperation.Member.MetadataName))
                    {
                        lazySinkKinds.Value.UnionWith(sinkInfo.SinkKinds);
                    }
                }

                if (lazySinkKinds.IsValueCreated)
                {
                    sinkKinds = lazySinkKinds.Value;
                    return true;
                }
                else
                {
                    sinkKinds = null;
                    return false;
                }
            }

            private IEnumerable<IArgumentOperation> GetTaintedArguments(ImmutableArray<IArgumentOperation> arguments)
            {
                foreach (IArgumentOperation arg in arguments)
                {
                    UpdateAbstractValueForArrayBeforeEnteringSinkForArray(arg, arg.Value);
                }

                return arguments.Where(
                    a => this.GetCachedAbstractValue(a).Kind == TaintedDataAbstractValueKind.Tainted
                         && (a.Parameter.RefKind == RefKind.None
                             || a.Parameter.RefKind == RefKind.Ref
                             || a.Parameter.RefKind == RefKind.In));
            }

            private void CacheAbstractValueForBothOperationAndEntity(IOperation operation, TaintedDataAbstractValue value)
            {
                CacheAbstractValue(operation, value);
                if (AnalysisEntityFactory.TryCreate(operation, out AnalysisEntity? analysisEntity))
                {
                    // Cause we don't save every array as tainted, there's could be a new sanitized array entity we need to save.
                    SetAbstractValueCore(CurrentAnalysisData, analysisEntity, value);
                }
            }

            /// <summary>
            /// Set array as tainted when there's <see cref="SourceInfo"/> taint all kinds of array before entering sink.
            /// </summary>
            /// <param name="operation">This operation could be IArgumentOperation or IAssignmentOperation.</param>
            /// <param name="value">Operation that could produce array.</param>
            private void UpdateAbstractValueForArrayBeforeEnteringSinkForArray(IOperation operation, IOperation value)
            {
                if (value.Type is IArrayTypeSymbol arrayTypeSymbol)
                {
                    TaintedDataAbstractValue taintedDataAbstractValue = GetCachedAbstractValue(value);

                    // Array is new untainted or sanitized.
                    if (taintedDataAbstractValue.Kind == TaintedDataAbstractValueKind.NotTainted
                        && this.DataFlowAnalysisContext.SourceInfos.IsSourceArray(arrayTypeSymbol, out TaintArrayKind taintArrayKind)
                        && taintArrayKind == TaintArrayKind.All)
                    {
                        if (AnalysisEntityFactory.TryCreate(value, out AnalysisEntity? analysisEntity))
                        {
                            if (analysisEntity.SymbolOpt != null && !this.CurrentAnalysisData.TryGetValue(analysisEntity, out taintedDataAbstractValue))
                            {
                                // We're relying on us not tracking AnalysisEntities unless they're sanitized or tainted.
                                taintedDataAbstractValue = TaintedDataAbstractValue.CreateTainted(analysisEntity.SymbolOpt, analysisEntity.SymbolOpt.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax(), this.OwningSymbol);
                                SetAbstractValue(analysisEntity, taintedDataAbstractValue);
                            }
                        }
                        else
                        {
                            ISymbol? taintedSymbol = null;
                            switch (value)
                            {
                                case IInvocationOperation invocationOperation:
                                    taintedSymbol = invocationOperation.TargetMethod;
                                    break;

                                case IArrayCreationOperation arrayCreationOperation:
                                    taintedSymbol = arrayCreationOperation.Type;
                                    break;

                                default:
                                    break;
                            }

                            if (taintedSymbol != null)
                            {
                                taintedDataAbstractValue = TaintedDataAbstractValue.CreateTainted(taintedSymbol, value.Syntax, this.OwningSymbol);
                            }
                        }
                    }

                    CacheAbstractValue(operation, taintedDataAbstractValue);
                    CacheAbstractValue(value, taintedDataAbstractValue);
                }
            }

            protected override void ApplyInterproceduralAnalysisResultCore(TaintedDataAnalysisData resultData)
                => ApplyInterproceduralAnalysisResultHelper(resultData.CoreAnalysisData);

            protected override TaintedDataAnalysisData GetTrimmedCurrentAnalysisData(IEnumerable<AnalysisEntity> withEntities)
                => GetTrimmedCurrentAnalysisDataHelper(withEntities, CurrentAnalysisData.CoreAnalysisData, SetAbstractValueCore);

        }
    }
}
