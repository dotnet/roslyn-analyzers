// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T> - CacheBasedEquatable handles equality

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// Abstract PointsTo value for an <see cref="AnalysisEntity"/>/<see cref="IOperation"/> tracked by <see cref="PointsToAnalysis"/>.
    /// It contains the set of possible <see cref="AbstractLocation"/>s that the entity or the operation can point to and the <see cref="Kind"/> of the location(s).
    /// </summary>
    internal class PointsToAbstractValue: CacheBasedEquatable<PointsToAbstractValue>
    {
        public static PointsToAbstractValue Undefined = new PointsToAbstractValue(PointsToAbstractValueKind.Undefined, NullAbstractValue.MaybeNull);
        public static PointsToAbstractValue Invalid = new PointsToAbstractValue(PointsToAbstractValueKind.Invalid, NullAbstractValue.Invalid);
        public static PointsToAbstractValue Unknown = new PointsToAbstractValue(PointsToAbstractValueKind.Unknown, NullAbstractValue.MaybeNull);
        public static PointsToAbstractValue NoLocation = new PointsToAbstractValue(ImmutableHashSet.Create(AbstractLocation.NoLocation), NullAbstractValue.NotNull);
        public static PointsToAbstractValue NullLocation = new PointsToAbstractValue(ImmutableHashSet.Create(AbstractLocation.Null), NullAbstractValue.Null);

        private PointsToAbstractValue(ImmutableHashSet<AbstractLocation> locations, NullAbstractValue nullState)
        {
            Debug.Assert(!locations.IsEmpty);
            Debug.Assert(locations.All(location => !location.IsNull) || nullState != NullAbstractValue.NotNull);
            Debug.Assert(nullState != NullAbstractValue.Undefined);
            Debug.Assert(nullState != NullAbstractValue.Invalid);

            Locations = locations;
            Kind = PointsToAbstractValueKind.Known;
            NullState = nullState;
        }

        private PointsToAbstractValue(PointsToAbstractValueKind kind, NullAbstractValue nullState)
        {
            Debug.Assert(kind != PointsToAbstractValueKind.Known);
            Debug.Assert(nullState != NullAbstractValue.Null);

            Locations = ImmutableHashSet<AbstractLocation>.Empty;
            Kind = kind;
            NullState = nullState;
        }

        public static PointsToAbstractValue Create(AbstractLocation location, bool mayBeNull)
        {
            Debug.Assert(!location.IsNull, "Use 'PointsToAbstractValue.NullLocation' singleton");
            Debug.Assert(!location.IsNoLocation, "Use 'PointsToAbstractValue.NoLocation' singleton");

            return new PointsToAbstractValue(ImmutableHashSet.Create(location), mayBeNull ? NullAbstractValue.MaybeNull : NullAbstractValue.NotNull);
        }

        public static PointsToAbstractValue Create(ImmutableHashSet<AbstractLocation> locations, NullAbstractValue nullState)
        {
            Debug.Assert(!locations.IsEmpty);

            if (locations.Count == 1)
            {
                var location = locations.Single();
                if (location.IsNull)
                {
                    return NullLocation;
                }
                if (location.IsNoLocation)
                {
                    return NoLocation;
                }
            }

            return new PointsToAbstractValue(locations, nullState);
        }

        public PointsToAbstractValue MakeNonNull(IOperation operation)
        {
            if (NullState == NullAbstractValue.NotNull)
            {
                return this;
            }

            if (Kind != PointsToAbstractValueKind.Known)
            {
                return Create(AbstractLocation.CreateAllocationLocation(operation, operation.Type), mayBeNull: false);
            }

            var locations = Locations.Where(location => !location.IsNull).ToImmutableHashSet();
            if (locations.Count == Locations.Count)
            {
                locations = Locations;
            }

            return new PointsToAbstractValue(locations, NullAbstractValue.NotNull);
        }

        public PointsToAbstractValue MakeNull()
        {
            if (NullState == NullAbstractValue.Null)
            {
                return this;
            }

            if (Kind != PointsToAbstractValueKind.Known)
            {
                return NullLocation;
            }

            return new PointsToAbstractValue(Locations, NullAbstractValue.Null);
        }

        public PointsToAbstractValue MakeMayBeNull()
        {
            Debug.Assert(NullState != NullAbstractValue.Null);
            if (NullState == NullAbstractValue.MaybeNull || ReferenceEquals(this, Unknown))
            {
                return this;
            }
            else if (Locations.IsEmpty)
            {
                return Unknown;
            }

            Debug.Assert(Locations.All(location => !location.IsNull));
            return new PointsToAbstractValue(Locations, NullAbstractValue.MaybeNull);
        }

        public ImmutableHashSet<AbstractLocation> Locations { get; }
        public PointsToAbstractValueKind Kind { get; }
        public NullAbstractValue NullState { get; }

        protected override int ComputeHashCode()
        {
            int hashCode = HashUtilities.Combine(Kind.GetHashCode(),
                HashUtilities.Combine(NullState.GetHashCode(), Locations.Count.GetHashCode()));
            foreach (var location in Locations)
            {
                hashCode = HashUtilities.Combine(location.GetHashCode(), hashCode);
            }

            return hashCode;
        }
    }
}
