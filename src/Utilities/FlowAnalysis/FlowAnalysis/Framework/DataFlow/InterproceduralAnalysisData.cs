﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T>

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
    /// <summary>
    /// Contains the caller's analysis context data passed to context sensitive interprocedural analysis, <see cref="InterproceduralAnalysisKind.ContextSensitive"/>.
    /// This includes the following:
    /// 1. Caller's tracked analysis data at the call site, which is the initial analysis data for callee.
    /// 2. Information about the invocation instance on which the method is invoked.
    /// 3. Information about arguments for the invocation.
    /// 4. Captured variables (for lambda/local function invocations).
    /// 5. Information about ref/out parameter entities that share address with callee's parameters.
    /// 6. Operation call stack for the current interprocedural analysis.
    /// 7. Set of analysis contexts currently being analyzed.
    /// </summary>
    public sealed class InterproceduralAnalysisData<TAnalysisData, TAnalysisContext, TAbstractAnalysisValue>
        : CacheBasedEquatable<InterproceduralAnalysisData<TAnalysisData, TAnalysisContext, TAbstractAnalysisValue>>
        where TAnalysisContext : class, IDataFlowAnalysisContext
    {
        public InterproceduralAnalysisData(
            TAnalysisData initialAnalysisData,
            (AnalysisEntity, PointsToAbstractValue)? invocationInstanceOpt,
            (AnalysisEntity, PointsToAbstractValue)? thisOrMeInstanceForCallerOpt,
            ImmutableDictionary<IParameterSymbol, ArgumentInfo<TAbstractAnalysisValue>> argumentValuesMap,
            ImmutableDictionary<ISymbol, PointsToAbstractValue> capturedVariablesMap,
            ImmutableDictionary<AnalysisEntity, CopyAbstractValue> addressSharedEntities,
            ImmutableStack<IOperation> callStack,
            ImmutableHashSet<TAnalysisContext> methodsBeingAnalyzed,
            Func<IOperation, TAbstractAnalysisValue> getCachedAbstractValueFromCaller,
            Func<IMethodSymbol, ControlFlowGraph> getInterproceduralControlFlowGraph,
            Func<IOperation, AnalysisEntity> getAnalysisEntityForFlowCapture,
            Func<ISymbol, ImmutableStack<IOperation>> getInterproceduralCallStackForOwningSymbol)
        {
            Debug.Assert(initialAnalysisData != null);
            Debug.Assert(argumentValuesMap != null);
            Debug.Assert(addressSharedEntities != null);
            Debug.Assert(callStack != null);
            Debug.Assert(methodsBeingAnalyzed != null);
            Debug.Assert(getCachedAbstractValueFromCaller != null);
            Debug.Assert(getInterproceduralControlFlowGraph != null);
            Debug.Assert(getAnalysisEntityForFlowCapture != null);
            Debug.Assert(getInterproceduralCallStackForOwningSymbol != null);

            InitialAnalysisData = initialAnalysisData;
            InvocationInstanceOpt = invocationInstanceOpt;
            ThisOrMeInstanceForCallerOpt = thisOrMeInstanceForCallerOpt;
            ArgumentValuesMap = argumentValuesMap;
            CapturedVariablesMap = capturedVariablesMap;
            AddressSharedEntities = addressSharedEntities;
            CallStack = callStack;
            MethodsBeingAnalyzed = methodsBeingAnalyzed;
            GetCachedAbstractValueFromCaller = getCachedAbstractValueFromCaller;
            GetInterproceduralControlFlowGraph = getInterproceduralControlFlowGraph;
            GetAnalysisEntityForFlowCapture = getAnalysisEntityForFlowCapture;
            GetInterproceduralCallStackForOwningSymbol = getInterproceduralCallStackForOwningSymbol;
        }

        public TAnalysisData InitialAnalysisData { get; }
        public (AnalysisEntity InstanceOpt, PointsToAbstractValue PointsToValue)? InvocationInstanceOpt { get; }
        public (AnalysisEntity Instance, PointsToAbstractValue PointsToValue)? ThisOrMeInstanceForCallerOpt { get; }
        public ImmutableDictionary<IParameterSymbol, ArgumentInfo<TAbstractAnalysisValue>> ArgumentValuesMap { get; }
        public ImmutableDictionary<ISymbol, PointsToAbstractValue> CapturedVariablesMap { get; }
        public ImmutableDictionary<AnalysisEntity, CopyAbstractValue> AddressSharedEntities { get; }
        public ImmutableStack<IOperation> CallStack { get; }
        public ImmutableHashSet<TAnalysisContext> MethodsBeingAnalyzed { get; }
        public Func<IOperation, TAbstractAnalysisValue> GetCachedAbstractValueFromCaller { get; }
        public Func<IMethodSymbol, ControlFlowGraph> GetInterproceduralControlFlowGraph { get; }
        public Func<IOperation, AnalysisEntity> GetAnalysisEntityForFlowCapture { get; }
        public Func<ISymbol, ImmutableStack<IOperation>> GetInterproceduralCallStackForOwningSymbol { get; }

        protected override void ComputeHashCodeParts(ArrayBuilder<int> builder)
        {
            builder.Add(InitialAnalysisData.GetHashCodeOrDefault());
            AddHashCodeParts(InvocationInstanceOpt, builder);
            AddHashCodeParts(ThisOrMeInstanceForCallerOpt, builder);
            builder.Add(HashUtilities.Combine(ArgumentValuesMap));
            builder.Add(HashUtilities.Combine(CapturedVariablesMap));
            builder.Add(HashUtilities.Combine(AddressSharedEntities));
            builder.Add(HashUtilities.Combine(CallStack));
            builder.Add(HashUtilities.Combine(MethodsBeingAnalyzed));
        }

        private static void AddHashCodeParts(
            (AnalysisEntity InstanceOpt, PointsToAbstractValue PointsToValue)? instanceAndPointsToValueOpt,
            ArrayBuilder<int> builder)
        {
            if (instanceAndPointsToValueOpt.HasValue)
            {
                builder.Add(instanceAndPointsToValueOpt.Value.InstanceOpt.GetHashCodeOrDefault());
                builder.Add(instanceAndPointsToValueOpt.Value.PointsToValue.GetHashCode());
            }
            else
            {
                builder.Add(0);
            }
        }
    }
}
