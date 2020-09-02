// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Represents the abstract value for tracking basic code locations.
    /// </summary>
    internal class SimpleAbstractValue : CacheBasedEquatable<SimpleAbstractValue>
    {
        /// <summary>
        /// Default value or value used for locations with non-interesting syntax nodes.
        /// </summary>
        public static readonly SimpleAbstractValue None = new SimpleAbstractValue(SimpleAbstractValueKind.None);

        /// <summary>
        /// An access to a location if interest of the analyzer.
        /// </summary>
        public static readonly SimpleAbstractValue Access = new SimpleAbstractValue(SimpleAbstractValueKind.Access);

        /// <summary>
        /// Unknown state of access.
        /// </summary>
        public static readonly SimpleAbstractValue Unknown = new SimpleAbstractValue(SimpleAbstractValueKind.Unknown);

        /// <summary>
        /// A cache to reduce computation of previous location count.
        /// </summary>
        private readonly int cachedPreviousLocationCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAbstractValue"/> class.
        /// </summary>
        /// <param name="kind">The abstract value kind.</param>
        public SimpleAbstractValue(SimpleAbstractValueKind kind)
            : this(kind, default, Array.Empty<SimpleAbstractValue>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAbstractValue"/> class.
        /// </summary>
        /// <param name="kind">The abstract value kind.</param>
        /// <param name="accessLocation">The list of access locations.</param>
        /// <param name="previousLocations">Previous access locations.</param>
        private SimpleAbstractValue(
            SimpleAbstractValueKind kind,
            AccessLocation? accessLocation,
            IReadOnlyList<SimpleAbstractValue> previousLocations)
        {
            this.Kind = kind;
            this.AccessLocation = accessLocation;
            this.PreviousLocations = previousLocations;

            HashSet<SimpleAbstractValue> locations = new HashSet<SimpleAbstractValue>();
            SimpleAbstractValue.GetNestedPreviousLocationCount(this, locations);
            this.cachedPreviousLocationCount = locations.Count;
        }

        /// <summary>
        /// Gets the kind of abstract value.
        /// </summary>
        public SimpleAbstractValueKind Kind { get; }

        /// <summary>
        /// Gets the list of access locations.
        /// </summary>
        public AccessLocation? AccessLocation { get; }

        /// <summary>
        /// Gets the previous locations.
        /// </summary>
        public IReadOnlyList<SimpleAbstractValue> PreviousLocations { get; }

        /// <summary>
        /// Adds an access location to the abstract value.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="accessSymbol">The dictionary method or property accessed.</param>
        /// <param name="diagnosticLocation">The location for diagnostic placement.</param>
        /// <param name="isTypeChecking">True if type checking problem, otherise a casting problem.</param>
        /// <returns>The abstract value.</returns>
        public SimpleAbstractValue WithAccessLocation(
            AbstractLocation location,
            IOperation operation,
            ISymbol accessSymbol,
            SyntaxNode diagnosticLocation,
            bool isTypeChecking)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>(2)
            {
                ["IsTypeChecking"] = isTypeChecking.ToString(),
                ["Operation"] = operation
            };

            return new SimpleAbstractValue(
                this.Kind,
                new AccessLocation(location, accessSymbol, diagnosticLocation, properties),
                this.PreviousLocations);
        }

        /// <summary>
        /// Updates with previous locations.
        /// </summary>
        /// <param name="previousLocations">The previous locations to add.</param>
        /// <returns>The abstract value.</returns>
        public SimpleAbstractValue WithPreviousLocations(
            IEnumerable<SimpleAbstractValue> previousLocations)
        {
            HashSet<SimpleAbstractValue> uniqueLocations = new HashSet<SimpleAbstractValue>();

            foreach (SimpleAbstractValue v in this.PreviousLocations)
            {
                uniqueLocations.Add(v);
            }

            foreach (SimpleAbstractValue v in previousLocations)
            {
                uniqueLocations.Add(v);
            }

            return new SimpleAbstractValue(
                this.Kind,
                this.AccessLocation,
                new List<SimpleAbstractValue>(uniqueLocations));
        }

        /// <summary>
        /// Gets the number of nested previous locations.
        /// </summary>
        /// <returns>The number of unique nested locations.</returns>
        internal int GetPreviousLocationCount()
        {
            return this.cachedPreviousLocationCount;
        }

        /// <summary>
        /// Accumulate a list of hash codes to be combined for the object hash code.
        /// </summary>
        /// <param name="action">The action which takes the hash code values to combine.</param>
        protected override void ComputeHashCodeParts(Action<int> action)
        {
            if (action != null)
            {
                action((int)this.Kind);

                if (this.AccessLocation != null)
                {
                    action(this.AccessLocation.GetHashCode());
                }

                foreach (SimpleAbstractValue previousValue in this.PreviousLocations)
                {
                    action(previousValue.GetHashCode());
                }
            }
        }

        /// <summary>
        /// Gets all the unique nested locations.
        /// </summary>
        /// <param name="previous">The location to check for nested previous locations.</param>
        /// <param name="locations">The hash of unique locations.</param>
        private static void GetNestedPreviousLocationCount(
            SimpleAbstractValue previous,
            HashSet<SimpleAbstractValue> locations)
        {
            foreach (SimpleAbstractValue location in previous.PreviousLocations)
            {
                locations.Add(location);
                SimpleAbstractValue.GetNestedPreviousLocationCount(location, locations);
            }
        }
    }
}
