// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Represents an ExtraDictionaryAccess location.
    /// </summary>
    internal class ExtraTypeCheckingAbstractLocation : CacheBasedEquatable<ExtraTypeCheckingAbstractLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAbstractLocation"/> class.
        /// </summary>
        /// <param name="location">The pointsToValue location.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="accessLocation">The casting or type checking location.</param>
        public ExtraTypeCheckingAbstractLocation(
            AbstractLocation location,
            ISymbol targetType,
            Location accessLocation)
        {
            this.Location = location;
            this.TargetType = targetType;
            this.AccessLocation = accessLocation;
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        public AbstractLocation Location { get; }

        /// <summary>
        /// Gets the target type.
        /// </summary>
        public ISymbol TargetType { get; }

        /// <summary>
        /// Gets the access location.
        /// </summary>
        public Location AccessLocation { get; }

        /// <inheritdoc />
        protected override void ComputeHashCodeParts(Action<int> addPart)
        {
            addPart(this.Location.GetHashCode());
            addPart(this.TargetType.GetHashCode());

            if (this.AccessLocation != null)
            {
                addPart(this.AccessLocation.GetHashCode());
            }
        }
    }
}
