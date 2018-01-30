// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Factory to create <see cref="AnalysisEntity"/> objects for operations, symbol declarations, etc.
    /// This factory also tracks analysis entities that share the same instance location (e.g. value type members).
    /// </summary>
    internal sealed class AnalysisEntityFactory
    {
        private readonly ImmutableDictionary<PointsToAbstractValue, ImmutableHashSet<AnalysisEntity>.Builder>.Builder _analysisEntitiesPerInstance;
        private readonly Dictionary<IOperation, AnalysisEntity> _analysisEntityMap;
        private readonly Dictionary<ISymbol, PointsToAbstractValue> _instanceLocationsForSymbols;
        private readonly Func<IOperation, PointsToAbstractValue> _getPointsToAbstractValueOpt;
        
        public AnalysisEntityFactory(
            Func<IOperation, PointsToAbstractValue> getPointsToAbstractValueOpt, INamedTypeSymbol containingTypeSymbol)
        {
            _getPointsToAbstractValueOpt = getPointsToAbstractValueOpt;
            _analysisEntitiesPerInstance = ImmutableDictionary.CreateBuilder<PointsToAbstractValue, ImmutableHashSet<AnalysisEntity>.Builder>();
            _analysisEntityMap = new Dictionary<IOperation, AnalysisEntity>();
            _instanceLocationsForSymbols = new Dictionary<ISymbol, PointsToAbstractValue>();

            var thisOrMeInstanceLocation = AbstractLocation.CreateThisOrMeLocation(containingTypeSymbol);
            var instanceLocation = new PointsToAbstractValue(thisOrMeInstanceLocation);
            ThisOrMeInstance = AnalysisEntity.CreateThisOrMeInstance(containingTypeSymbol, instanceLocation);
            AddToMap(instanceLocation, ThisOrMeInstance);
        }

        public AnalysisEntity ThisOrMeInstance { get; }

        private ImmutableArray<AbstractIndex> CreateAbstractIndices<T>(ImmutableArray<T> indices)
            where T : IOperation
        {
            if (indices.Length > 0)
            {
                var builder = ImmutableArray.CreateBuilder<AbstractIndex>();
                foreach (var index in indices)
                {
                    builder.Add(CreateAbstractIndex(index));
                }

                return builder.ToImmutable();
            }

            return ImmutableArray<AbstractIndex>.Empty;
        }

        private AbstractIndex CreateAbstractIndex(IOperation operation)
        {
            if (operation.ConstantValue.HasValue && operation.ConstantValue.Value is int index)
            {
                return AbstractIndex.Create((uint)index);
            }
            else if (TryCreate(operation, out AnalysisEntity analysisEntity))
            {
                return AbstractIndex.Create(analysisEntity);
            }
            else
            {
                return AbstractIndex.Create(operation);
            }
        }

        public bool TryCreate(IOperation operation, out AnalysisEntity analysisEntity)
        {
            if (_analysisEntityMap.TryGetValue(operation, out analysisEntity))
            {
                return analysisEntity != null;
            }

            analysisEntity = null;
            ISymbol symbolOpt = null;
            ImmutableArray<AbstractIndex> indices = ImmutableArray<AbstractIndex>.Empty;
            IOperation instanceOpt = null;
            switch (operation)
            {
                case ILocalReferenceOperation localReference:
                    symbolOpt = localReference.Local;
                    break;

                case IParameterReferenceOperation parameterReference:
                    symbolOpt = parameterReference.Parameter;
                    break;

                case IMemberReferenceOperation memberReference:
                    instanceOpt = memberReference.Instance;
                    GetSymbolAndIndicesForMemberReference(memberReference, ref symbolOpt, ref indices);
                    
                    // Workaround for https://github.com/dotnet/roslyn/issues/22736 (IPropertyReferenceExpressions in IAnonymousObjectCreationExpression are missing a receiver).
                    if (instanceOpt == null &&
                        symbolOpt != null &&
                        memberReference is IPropertyReferenceOperation propertyReference)
                    {
                        instanceOpt = propertyReference.GetAnonymousObjectCreation();
                    }

                    break;

                case IArrayElementReferenceOperation arrayElementReference:
                    instanceOpt = arrayElementReference.ArrayReference;
                    indices = CreateAbstractIndices(arrayElementReference.Indices);
                    break;

                case IDynamicIndexerAccessOperation dynamicIndexerAccess:
                    instanceOpt = dynamicIndexerAccess.Operation;
                    indices = CreateAbstractIndices(dynamicIndexerAccess.Arguments);
                    break;

                case IConditionalAccessInstanceOperation conditionalAccessInstance:
                    IConditionalAccessOperation conditionalAccess = conditionalAccessInstance.GetConditionalAccess();
                    instanceOpt = conditionalAccess.Operation;
                    if (conditionalAccessInstance.Parent is IMemberReferenceOperation memberReferenceParent)
                    {
                        GetSymbolAndIndicesForMemberReference(memberReferenceParent, ref symbolOpt, ref indices);
                    }
                    break;

                case IInstanceReferenceOperation instanceReference:
                    instanceOpt = instanceReference.GetInstance();
                    if (instanceOpt == null)
                    {
                        // Reference to this or base instance.
                        analysisEntity = ThisOrMeInstance;
                    }
                    else
                    {
                        var instanceLocation = _getPointsToAbstractValueOpt(instanceReference);
                        analysisEntity = AnalysisEntity.Create(instanceReference, instanceLocation);
                        AddToMap(instanceLocation, analysisEntity);
                    }
                    break;

                case IInvocationOperation invocation:
                    symbolOpt = invocation.TargetMethod;
                    instanceOpt = invocation.Instance;
                    break;

                case IConversionOperation conversion:
                    return TryCreate(conversion.Operand, out analysisEntity);

                case IParenthesizedOperation parenthesized:
                    return TryCreate(parenthesized.Operand, out analysisEntity);

                default:
                    break;
            }

            if (symbolOpt != null || !indices.IsEmpty)
            {
                TryCreate(symbolOpt, indices, operation.Type, instanceOpt, out analysisEntity);
            }

            _analysisEntityMap[operation] = analysisEntity;
            return analysisEntity != null;
        }

        private void GetSymbolAndIndicesForMemberReference(IMemberReferenceOperation memberReference, ref ISymbol symbolOpt, ref ImmutableArray<AbstractIndex> indices)
        {
            switch (memberReference)
            {
                case IFieldReferenceOperation fieldReference:
                    symbolOpt = fieldReference.Member;
                    break;

                case IEventReferenceOperation eventReference:
                    symbolOpt = eventReference.Member;
                    break;

                case IPropertyReferenceOperation propertyReference:
                    // We are only tracking:
                    // 1) Indexers
                    // 2) Read-only properties.
                    // 3) Properties with a backing field (auto-generated properties)
                    if (propertyReference.Arguments.Length > 0 ||
                        propertyReference.Property.IsReadOnly ||
                        propertyReference.Property.IsPropertyWithBackingField())
                    {
                        symbolOpt = propertyReference.Property;
                        indices = propertyReference.Arguments.Length > 0 ?
                            CreateAbstractIndices(propertyReference.Arguments.Select(a => a.Value).ToImmutableArray()) :
                            ImmutableArray<AbstractIndex>.Empty;
                    }
                    break;
            }
        }

        public bool TryCreateForSymbolDeclaration(ISymbol symbol, out AnalysisEntity analysisEntity)
        {
            Debug.Assert(symbol != null);
            Debug.Assert(symbol.Kind == SymbolKind.Local || symbol.Kind == SymbolKind.Parameter || symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property);

            var indices = ImmutableArray<AbstractIndex>.Empty;
            IOperation instance = null;
            var type = symbol.GetMemerOrLocalOrParameterType();
            Debug.Assert(type != null);

            return TryCreate(symbol, indices, type, instance, out analysisEntity);
        }

        public bool TryCreateForElementInitializer(IOperation instance, ImmutableArray<AbstractIndex> indices, ITypeSymbol elementType, out AnalysisEntity analysisEntity)
        {
            Debug.Assert(instance != null);
            Debug.Assert(!indices.IsEmpty);
            Debug.Assert(elementType != null);

            ISymbol symbol = null;
            return TryCreate(symbol, indices, elementType, instance, out analysisEntity);
        }

        private bool TryCreate(ISymbol symbolOpt, ImmutableArray<AbstractIndex> indices,
            ITypeSymbol type, IOperation instanceOpt, out AnalysisEntity symbolWithLocationinfo)
        {
            Debug.Assert(symbolOpt != null || !indices.IsEmpty);
            Debug.Assert(type != null);

            symbolWithLocationinfo = null;

            // Only analyze member symbols if we have points to analysis result.
            if (_getPointsToAbstractValueOpt == null &&
                symbolOpt?.Kind != SymbolKind.Local &&
                symbolOpt?.Kind != SymbolKind.Parameter)
            {
                return false;
            }

            PointsToAbstractValue instanceLocationOpt = null;
            AnalysisEntity parentOpt = null;
            if (instanceOpt?.Type != null)
            {
                if (instanceOpt.Type.HasValueCopySemantics())
                {
                    if (TryCreate(instanceOpt, out parentOpt))
                    {
                        instanceLocationOpt = parentOpt.InstanceLocation;
                    }
                    else
                    {
                        // For value type allocations, we store the points to location.
                        var instancePointsToValue = _getPointsToAbstractValueOpt(instanceOpt);
                        if (instancePointsToValue.Kind != PointsToAbstractValueKind.NoLocation)
                        {
                            instanceLocationOpt = instancePointsToValue;
                        }
                    }
                }
                else
                {
                    instanceLocationOpt = _getPointsToAbstractValueOpt(instanceOpt);
                }
            }

            symbolWithLocationinfo = Create(symbolOpt, indices, type, instanceLocationOpt, parentOpt);
            return true;
        }

        private PointsToAbstractValue EnsureLocation(PointsToAbstractValue instanceLocationOpt, ISymbol symbolOpt, AnalysisEntity parentOpt, ITypeSymbol type)
        {
            if (instanceLocationOpt == null && symbolOpt != null)
            {
                Debug.Assert(symbolOpt.Kind == SymbolKind.Local || symbolOpt.Kind == SymbolKind.Parameter || symbolOpt.IsStatic);

                if (!_instanceLocationsForSymbols.TryGetValue(symbolOpt, out instanceLocationOpt))
                {
                    if (parentOpt != null)
                    {
                        instanceLocationOpt = parentOpt.InstanceLocation;
                    }
                    else
                    {
                        var location = AbstractLocation.CreateSymbolLocation(symbolOpt);
                        instanceLocationOpt = new PointsToAbstractValue(location);
                    }

                    _instanceLocationsForSymbols.Add(symbolOpt, instanceLocationOpt);
                }
            }

            return instanceLocationOpt;
        }

        private AnalysisEntity Create(ISymbol symbolOpt, ImmutableArray<AbstractIndex> indices, ITypeSymbol type, PointsToAbstractValue instanceLocationOpt, AnalysisEntity parentOpt)
        {
            instanceLocationOpt = EnsureLocation(instanceLocationOpt, symbolOpt, parentOpt, type);
            Debug.Assert(instanceLocationOpt != null);
            var analysisEntity = AnalysisEntity.Create(symbolOpt, indices, type, instanceLocationOpt, parentOpt);
            AddToMap(instanceLocationOpt, analysisEntity);
            return analysisEntity;
        }

        private void AddToMap(PointsToAbstractValue instanceLocation, AnalysisEntity analysisEntity)
        {
            Debug.Assert(instanceLocation != null);
            if (!_analysisEntitiesPerInstance.TryGetValue(instanceLocation, out var builder))
            {
                builder = ImmutableHashSet.CreateBuilder<AnalysisEntity>();
            }

            builder.Add(analysisEntity);
            _analysisEntitiesPerInstance[instanceLocation] = builder;
        }

        public AnalysisEntity CreateWithNewInstanceRoot(AnalysisEntity analysisEntity, AnalysisEntity newRootInstance)
        {
            if (analysisEntity.InstanceLocation == newRootInstance.InstanceLocation)
            {
                return analysisEntity;
            }

            if (analysisEntity.ParentOpt == null)
            {
                return newRootInstance;
            }

            AnalysisEntity parentOpt = CreateWithNewInstanceRoot(analysisEntity.ParentOpt, newRootInstance);
            return Create(analysisEntity.SymbolOpt, analysisEntity.Indices, analysisEntity.Type, newRootInstance.InstanceLocation, parentOpt);
        }

        public ImmutableHashSet<AnalysisEntity> GetAnalysisEntitiesCreatedFromInstance(PointsToAbstractValue instance)
            => _analysisEntitiesPerInstance.TryGetValue(instance, out ImmutableHashSet<AnalysisEntity>.Builder builder) && builder != null ?
                builder.ToImmutable() :
                ImmutableHashSet<AnalysisEntity>.Empty;
    }
}