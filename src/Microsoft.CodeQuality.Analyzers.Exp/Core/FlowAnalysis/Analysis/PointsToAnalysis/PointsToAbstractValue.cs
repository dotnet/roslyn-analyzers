// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// Abstract points to value for an <see cref="AnalysisEntity"/>/<see cref="IOperation"/> tracked by <see cref="PointsToAnalysis"/>.
    /// It contains the set of possible <see cref="AbstractLocation"/>s that the entity or the operation can point to and the <see cref="Kind"/> of the location(s).
    /// </summary>
    internal class PointsToAbstractValue: IEquatable<PointsToAbstractValue>
    {
        public static PointsToAbstractValue Undefined = new PointsToAbstractValue(PointsToAbstractValueKind.Undefined);
        public static PointsToAbstractValue NoLocation = new PointsToAbstractValue(PointsToAbstractValueKind.NoLocation);
        public static PointsToAbstractValue Unknown = new PointsToAbstractValue(PointsToAbstractValueKind.Unknown);
        
        private PointsToAbstractValue(ImmutableHashSet<AbstractLocation> locations, PointsToAbstractValueKind kind)
        {
            Locations = locations;
            Kind = kind;
        }

        private PointsToAbstractValue(PointsToAbstractValueKind kind)
            : this(ImmutableHashSet<AbstractLocation>.Empty, kind)
        {
            Debug.Assert(kind != PointsToAbstractValueKind.Known);
        }

        public PointsToAbstractValue(AbstractLocation location)
            : this(ImmutableHashSet.Create(location))
        {
        }

        public PointsToAbstractValue(ImmutableHashSet<AbstractLocation> locations)
            : this(locations, PointsToAbstractValueKind.Known)
        {
            Debug.Assert(locations.Count > 0);            
        }

        public ImmutableHashSet<AbstractLocation> Locations { get; }
        public PointsToAbstractValueKind Kind { get; }

        public static bool operator ==(PointsToAbstractValue value1, PointsToAbstractValue value2)
        {
            if ((object)value1 == null)
            {
                return (object)value2 == null;
            }

            return value1.Equals(value2);
        }

        public static bool operator !=(PointsToAbstractValue value1, PointsToAbstractValue value2)
        {
            return !(value1 == value2);
        }

        public bool Equals(PointsToAbstractValue other)
        {
            return other != null &&
                Kind == other.Kind &&
                Locations.SetEquals(other.Locations);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PointsToAbstractValue);
        }

        public override int GetHashCode()
        {
            int hashCode = HashUtilities.Combine(Kind.GetHashCode(), Locations.Count.GetHashCode());
            foreach (var location in Locations)
            {
                hashCode = HashUtilities.Combine(location.GetHashCode(), hashCode);
            }

            return hashCode;
        }
    }
}
