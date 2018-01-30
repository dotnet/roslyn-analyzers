// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// <para>
    /// Primary entity for which analysis data is tracked by <see cref="DataFlowAnalysis"/>.
    /// </para>
    /// <para>
    /// The entity is based on one or more of the following:
    ///     1. An <see cref="ISymbol"/>.
    ///     2. One or more <see cref="AbstractIndex"/> indices to index into the parent key.
    ///     3. "this" or "Me" instance.
    ///     4. An allocation or an object creation.
    /// </para>
    /// <para>
    /// Each entity has:
    ///     1. An associated non-null <see cref="Type"/> and
    ///     2. A non-null <see cref="InstanceLocation"/> indicating the abstract location at which the entity is located and
    ///     3. An optional parent key if this key has the same <see cref="InstanceLocation"/> as the parent (i.e. parent is a value type).
    /// </para>
    /// </summary>
    internal sealed class AnalysisEntity : IEquatable<AnalysisEntity>
    {
        private AnalysisEntity(ISymbol symbolOpt, ImmutableArray<AbstractIndex> indices, PointsToAbstractValue location, ITypeSymbol type, AnalysisEntity parentOpt)
        {
            Debug.Assert(!indices.IsDefault);
            Debug.Assert(symbolOpt != null || !indices.IsEmpty);
            Debug.Assert(location != null);
            Debug.Assert(type != null);
            Debug.Assert(parentOpt == null || parentOpt.Type.HasValueCopySemantics());

            SymbolOpt = symbolOpt;
            Indices = indices;
            InstanceLocation = location;
            Type = type;
            ParentOpt = parentOpt;
        }

        private AnalysisEntity(IInstanceReferenceOperation instanceReferenceOperation, PointsToAbstractValue location)
        {
            Debug.Assert(instanceReferenceOperation != null);
            Debug.Assert(location != null);

            InstanceReferenceOperationSyntaxOpt = instanceReferenceOperation.Syntax;
            InstanceLocation = location;
            Type = instanceReferenceOperation.Type;
            Indices = ImmutableArray<AbstractIndex>.Empty;
        }

        private AnalysisEntity(INamedTypeSymbol typeSymbol, PointsToAbstractValue location)
        {
            Debug.Assert(typeSymbol != null);
            Debug.Assert(location != null);

            SymbolOpt = typeSymbol;
            InstanceLocation = location;
            Type = typeSymbol;
            Indices = ImmutableArray<AbstractIndex>.Empty;
        }

        public static AnalysisEntity Create(ISymbol symbolOpt, ImmutableArray<AbstractIndex> indices,
            ITypeSymbol type, PointsToAbstractValue instanceLocation, AnalysisEntity parentOpt)
        {
            Debug.Assert(symbolOpt != null || !indices.IsEmpty);
            Debug.Assert(instanceLocation != null);
            Debug.Assert(type != null);
            Debug.Assert(parentOpt == null || parentOpt.InstanceLocation == instanceLocation);

            return new AnalysisEntity(symbolOpt, indices, instanceLocation, type, parentOpt);
        }

        public static AnalysisEntity Create(IInstanceReferenceOperation instanceReferenceOperation, PointsToAbstractValue instanceLocation)
        {
            Debug.Assert(instanceReferenceOperation != null);
            Debug.Assert(instanceLocation != null);

            return new AnalysisEntity(instanceReferenceOperation, instanceLocation);
        }

        public static AnalysisEntity CreateThisOrMeInstance(INamedTypeSymbol typeSymbol, PointsToAbstractValue instanceLocation)
        {
            Debug.Assert(typeSymbol != null);
            Debug.Assert(instanceLocation != null);
            Debug.Assert(instanceLocation.Locations.Count == 1);
            Debug.Assert(instanceLocation.Locations.Single().CreationOpt == null);
            Debug.Assert(instanceLocation.Locations.Single().SymbolOpt == typeSymbol);

            return new AnalysisEntity(typeSymbol, instanceLocation);
        }

        public bool IsChildOrInstanceMember
        {
            get
            {
                bool result;
                if (SymbolOpt != null)
                {
                    result = SymbolOpt.Kind != SymbolKind.Parameter &&
                        SymbolOpt.Kind != SymbolKind.Local &&
                        !SymbolOpt.IsStatic;
                }
                else if (Indices.Length > 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }

                Debug.Assert(ParentOpt == null || result);
                return result;
            }
        }

        public ISymbol SymbolOpt { get; }
        public ImmutableArray<AbstractIndex> Indices { get; }
        public SyntaxNode InstanceReferenceOperationSyntaxOpt { get; }
        public PointsToAbstractValue InstanceLocation { get; }
        public ITypeSymbol Type { get; }
        public AnalysisEntity ParentOpt { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as AnalysisEntity);
        }

        public static bool operator ==(AnalysisEntity value1, AnalysisEntity value2)
        {
            if ((object)value1 == null)
            {
                return (object)value2 == null;
            }

            return value1.Equals(value2);
        }

        public static bool operator !=(AnalysisEntity value1, AnalysisEntity value2)
        {
            return !(value1 == value2);
        }

        public bool Equals(AnalysisEntity other)
        {
            return other != null &&
                SymbolOpt == other.SymbolOpt &&
                InstanceReferenceOperationSyntaxOpt == other.InstanceReferenceOperationSyntaxOpt &&
                Indices.SequenceEqual(other.Indices) &&
                InstanceLocation == other.InstanceLocation &&
                Type.Equals(other.Type) &&
                ParentOpt == other.ParentOpt;
        }

        public override int GetHashCode()
        {
            var hashCode = HashUtilities.Combine(SymbolOpt?.GetHashCode() ?? 0,
                HashUtilities.Combine(InstanceReferenceOperationSyntaxOpt?.GetHashCode() ?? 0,
                HashUtilities.Combine(InstanceLocation.GetHashCode(),
                HashUtilities.Combine(Type.GetHashCode(),
                HashUtilities.Combine(ParentOpt?.GetHashCode() ?? 0, Indices.Length.GetHashCode())))));

            foreach (AbstractIndex index in Indices)
            {
                hashCode = HashUtilities.Combine(index.GetHashCode(), hashCode);
            }

            return hashCode;
        }

        public bool HasAncestorOrSelf(AnalysisEntity ancestor)
        {
            Debug.Assert(ancestor != null);

            AnalysisEntity current = this;
            do
            {
                if (current == ancestor)
                {
                    return true;
                }

                current = current.ParentOpt;
            } while (current != null);

            return false;
        }
    }
}