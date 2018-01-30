// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// <para>
    /// Represents an abstract analysis location.
    /// This is may be used to represent a location where an <see cref="AnalysisEntity"/> resides, i.e. <see cref="AnalysisEntity.InstanceLocation"/> or
    /// a location that is pointed to by a reference type variable, and tracked with <see cref="PointsToAnalysis.PointsToAnalysis"/>.
    /// </para>
    /// <para>
    /// An analysis location can be created for one of the following cases:
    ///     1. An allocation or an object creation operation (<see cref="CreateAllocationLocation(IOperation, ITypeSymbol)"/>).
    ///     2. Location for the implicit 'this' or 'Me' instance being analyzed (<see cref="CreateThisOrMeLocation(INamedTypeSymbol)"/>).
    ///     3. Location created for certain symbols which do not have a declaration in executable code, i.e. no <see cref="IOperation"/> for declaration (such as parameter symbols, member symbols, etc. - <see cref="CreateSymbolLocation(ISymbol)"/>).
    /// </para>
    /// </summary>
    internal sealed class AbstractLocation : IEquatable<AbstractLocation>
    {
        private AbstractLocation(IOperation creationOpt, ISymbol symbolOpt, ITypeSymbol locationType)
        {
            Debug.Assert(creationOpt != null ^ symbolOpt != null);
            Debug.Assert(locationType != null);

            CreationOpt = creationOpt;
            SymbolOpt = symbolOpt;
            LocationType = locationType;
        }

        public static AbstractLocation CreateAllocationLocation(IOperation creation, ITypeSymbol locationType) => new AbstractLocation(creation, symbolOpt: null, locationType: locationType);
        public static AbstractLocation CreateThisOrMeLocation(INamedTypeSymbol namedTypeSymbol) => new AbstractLocation(creationOpt: null, symbolOpt: namedTypeSymbol, locationType: namedTypeSymbol);
        public static AbstractLocation CreateSymbolLocation(ISymbol symbol) => new AbstractLocation(creationOpt: null, symbolOpt: symbol, locationType: symbol.GetMemerOrLocalOrParameterType());

        public IOperation CreationOpt { get; }
        public ISymbol SymbolOpt { get; }
        public ITypeSymbol LocationType { get; }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as AbstractLocation);
        }

        public static bool operator ==(AbstractLocation value1, AbstractLocation value2)
        {
            if ((object)value1 == null)
            {
                return (object)value2 == null;
            }

            return value1.Equals(value2);
        }

        public static bool operator !=(AbstractLocation value1, AbstractLocation value2)
        {
            return !(value1 == value2);
        }

        public bool Equals(AbstractLocation other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other != null &&
                CreationOpt == other.CreationOpt &&
                SymbolOpt == other.SymbolOpt &&
                LocationType == other.LocationType;
        }

        public override int GetHashCode()
        {
            return HashUtilities.Combine(CreationOpt?.GetHashCode() ?? 0,
                HashUtilities.Combine(SymbolOpt?.GetHashCode() ?? 0, LocationType.GetHashCode()));
        }
    }
}
