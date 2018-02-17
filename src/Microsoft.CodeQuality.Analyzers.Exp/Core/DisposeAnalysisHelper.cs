// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.DisposeAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp
{
    /// <summary>
    /// Helper for <see cref="DisposeAnalysis"/>.
    /// </summary>
    internal class DisposeAnalysisHelper
    {
        private static readonly string[] s_disposeOwnershipTransferLikelyTypes = new string[]
            {
                "System.IO.Stream",
                "System.IO.TextReader",
                "System.IO.TextWriter",
                "System.Resources.IResourceReader",
            };
        private static readonly ConditionalWeakTable<Compilation, DisposeAnalysisHelper> s_DisposeHelperCache =
            new ConditionalWeakTable<Compilation, DisposeAnalysisHelper>();
        private static readonly ConditionalWeakTable<Compilation, DisposeAnalysisHelper>.CreateValueCallback s_DisposeHelperCacheCallback =
            new ConditionalWeakTable<Compilation, DisposeAnalysisHelper>.CreateValueCallback(compilation => new DisposeAnalysisHelper(compilation));

        private static ImmutableHashSet<OperationKind> s_DisposableCreationKinds => ImmutableHashSet.Create(
            OperationKind.ObjectCreation,
            OperationKind.TypeParameterObjectCreation,
            OperationKind.DynamicObjectCreation,
            OperationKind.Invocation);

        public INamedTypeSymbol IDisposable { get; }
        private readonly INamedTypeSymbol _iCollection;
        private readonly INamedTypeSymbol _genericICollection;
        private readonly ImmutableHashSet<INamedTypeSymbol> _disposeOwnershipTransferLikelyTypes;
        private ConcurrentDictionary<INamedTypeSymbol, ImmutableHashSet<IFieldSymbol>> _lazyDisposableFieldsMap;

        private DisposeAnalysisHelper(Compilation compilation)
        {
            IDisposable = WellKnownTypes.IDisposable(compilation);
            if (IDisposable != null)
            {
                _iCollection = WellKnownTypes.ICollection(compilation);
                _genericICollection = WellKnownTypes.GenericICollection(compilation);
                _disposeOwnershipTransferLikelyTypes = GetDisposeOwnershipTransferLikelyTypes(compilation);
            }
        }

        private static ImmutableHashSet<INamedTypeSymbol> GetDisposeOwnershipTransferLikelyTypes(Compilation compilation)
        {
            var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>();
            foreach (var typeName in s_disposeOwnershipTransferLikelyTypes)
            {
                INamedTypeSymbol typeSymbol = compilation.GetTypeByMetadataName(typeName);
                if (typeSymbol != null)
                {
                    builder.Add(typeSymbol);
                }
            }

            return builder.ToImmutable();
        }

        private void EnsureDisposableFieldsMap()
        {
            if (_lazyDisposableFieldsMap == null)
            {
                Interlocked.CompareExchange(ref _lazyDisposableFieldsMap, new ConcurrentDictionary<INamedTypeSymbol, ImmutableHashSet<IFieldSymbol>>(), null);
            }
        }

        public static bool TryGetOrCreate(Compilation compilation, out DisposeAnalysisHelper disposeHelper)
        {
            disposeHelper = s_DisposeHelperCache.GetValue(compilation, s_DisposeHelperCacheCallback);
            if (disposeHelper.IDisposable == null)
            {
                disposeHelper = null;
                return false;
            }

            return true;
        }

        public bool TryGetOrComputeResult(
            ImmutableArray<IOperation> operationBlocks,
            IMethodSymbol containingMethod,
            out ControlFlowGraph cfg,
            out DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> disposeAnalysisResult)
        {
            return TryGetOrComputeResult(operationBlocks, containingMethod, out cfg, out disposeAnalysisResult, out var _);
        }

        public bool TryGetOrComputeResult(
            ImmutableArray<IOperation> operationBlocks,
            IMethodSymbol containingMethod,
            out ControlFlowGraph cfg,
            out DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> disposeAnalysisResult,
            out DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResult)
        {
            return TryGetOrComputeResult(operationBlocks, containingMethod, out cfg, out disposeAnalysisResult, out pointsToAnalysisResult, out var _);
        }

        public bool TryGetOrComputeResult(
            ImmutableArray<IOperation> operationBlocks,
            IMethodSymbol containingMethod,
            out ControlFlowGraph cfg,
            out DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> disposeAnalysisResult,
            out DataFlowAnalysisResult<PointsToBlockAnalysisResult, PointsToAbstractValue> pointsToAnalysisResult,
            out ImmutableDictionary<IFieldSymbol, PointsToAbstractValue> trackedInstanceFieldPointsToMap)
        {
            foreach (var operationRoot in operationBlocks)
            {
                IBlockOperation topmostBlock = operationRoot.GetTopmostParentBlock();
                if (topmostBlock != null)
                {
                    cfg = ControlFlowGraph.Create(topmostBlock);
                    var nullAnalysisResult = NullAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType);
                    pointsToAnalysisResult = PointsToAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType, nullAnalysisResult);
                    disposeAnalysisResult = DisposeAnalysis.GetOrComputeResult(cfg, IDisposable, _iCollection,
                        _genericICollection, _disposeOwnershipTransferLikelyTypes, containingMethod.ContainingType, pointsToAnalysisResult,
                        out trackedInstanceFieldPointsToMap, nullAnalysisResult);
                    return true;
                }
            }

            cfg = null;
            disposeAnalysisResult = null;
            pointsToAnalysisResult = null;
            trackedInstanceFieldPointsToMap = null;
            return false;
        }

        private bool HasDisposableOwnershipTransferForParameter(IMethodSymbol containingMethod) =>
            containingMethod.MethodKind == MethodKind.Constructor &&
            containingMethod.Parameters.Length == 1 &&
            _disposeOwnershipTransferLikelyTypes.Contains(containingMethod.Parameters[0].Type);

        public bool HasAnyDisposableCreationDescendant(ImmutableArray<IOperation> operationBlocks, IMethodSymbol containingMethod)
        {
            foreach (var operationBlock in operationBlocks)
            {
                foreach (var descendant in operationBlock.DescendantsAndSelf())
                {
                    if (s_DisposableCreationKinds.Contains(descendant.Kind) &&
                        descendant.Type.IsDisposable(IDisposable))
                    {
                        return true;
                    }
                }
            }

            return HasDisposableOwnershipTransferForParameter(containingMethod);
        }

        public ImmutableHashSet<IFieldSymbol> GetDisposableFields(INamedTypeSymbol namedType)
        {
            EnsureDisposableFieldsMap();
            if (_lazyDisposableFieldsMap.TryGetValue(namedType, out ImmutableHashSet<IFieldSymbol> disposableFields))
            {
                return disposableFields;
            }

            if (!namedType.IsDisposable(IDisposable))
            {
                disposableFields = ImmutableHashSet<IFieldSymbol>.Empty;
            }
            else
            {
                disposableFields = namedType.GetMembers().OfType<IFieldSymbol>().Where(f => f.Type.IsDisposable(IDisposable)).ToImmutableHashSet();
            }

            return _lazyDisposableFieldsMap.GetOrAdd(namedType, disposableFields);
        }

        /// <summary>
        /// Returns true if the given <paramref name="location"/> was created for an allocation in the <paramref name="containingMethod"/>
        /// or represents a location created for a constructor parameter whose type indicates dispose ownership transfer.
        /// </summary>
        public bool IsDisposableCreationOrDisposeOwnershipTransfer(AbstractLocation location, IMethodSymbol containingMethod)
        {
            if (location.CreationOpt == null)
            {
                return false;
            }

            if (s_DisposableCreationKinds.Contains(location.CreationOpt.Kind))
            {
                return true;
            }

            if (location.CreationOpt.Kind == OperationKind.ParameterReference)
            {
                return HasDisposableOwnershipTransferForParameter(containingMethod);
            }

            return false;
        }
    }
}
