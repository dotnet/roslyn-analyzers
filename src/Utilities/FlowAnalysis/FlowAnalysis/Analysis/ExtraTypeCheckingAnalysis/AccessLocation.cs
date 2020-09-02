// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Represents a dictionary access location.
    /// </summary>
    internal class AccessLocation : CacheBasedEquatable<AccessLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessLocation"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="accessSymbol">The symbol which was accessed.</param>
        /// <param name="diagnosticLocation">The location for diagnostic placement.</param>
        /// <param name="properties">The access location specific properties.</param>
        public AccessLocation(
            AbstractLocation location,
            ISymbol accessSymbol,
            SyntaxNode diagnosticLocation,
            IReadOnlyDictionary<string, object>? properties)
        {
            this.Location = location;
            this.AccessSymbol = accessSymbol;
            this.DiagnosticLocation = diagnosticLocation;
            this.Properties = properties;
        }

        /// <summary>
        /// Gets the location of the access.
        /// </summary>
        public AbstractLocation Location { get; }

        /// <summary>
        /// Gets the method or property that was accessed.
        /// </summary>
        public ISymbol AccessSymbol { get; }

        /// <summary>
        /// Gets the syntax node to put the diagnostic on.
        /// </summary>
        public SyntaxNode DiagnosticLocation { get; }

        /// <summary>
        /// Gets the location specific properties.
        /// </summary>
        public IReadOnlyDictionary<string, object>? Properties { get; }

        /// <inheritdoc />
        protected override void ComputeHashCodeParts(Action<int> addPart)
        {
            addPart(this.Location.GetHashCode());
            addPart(this.AccessSymbol.GetHashCode());
            addPart(this.DiagnosticLocation.GetHashCode());

            if (this.Properties != null)
            {
                foreach (KeyValuePair<string, object> kvp in this.Properties)
                {
                    addPart(kvp.Key.GetHashCode());
                    addPart(kvp.Value.GetHashCode());
                }
            }
        }
    }
}
