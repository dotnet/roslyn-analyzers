// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// Kind for the <see cref="PointsToAbstractValue"/>.
    /// </summary>
    internal enum PointsToAbstractValueKind
    {
        /// <summary>
        /// Undefined value.
        /// </summary>
        Undefined,

        /// <summary>
        /// Points to one or more known possible locations.
        /// </summary>
        Known,

        /// <summary>
        /// Indicates no tracked location (for e.g. literals, constants, etc.).
        /// </summary>
        NoLocation,

        /// <summary>
        /// Points to unknown set of locations.
        /// </summary>
        Unknown,
    }
}
