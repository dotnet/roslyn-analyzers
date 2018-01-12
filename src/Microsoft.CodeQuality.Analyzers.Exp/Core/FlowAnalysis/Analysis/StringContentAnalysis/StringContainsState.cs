// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    /// <summary>
    /// String state for presence of literal or non-literal values for symbol/operation tracked by <see cref="StringContentAnalysis"/>.
    /// </summary>
    internal enum StringContainsState
    {
        /// <summary>The lattice could not be sure of the state.</summary>
        Maybe = 0,
        /// <summary>The variable does not contain any instances of the specified kind of string.</summary>
        No,
        /// <summary>The variable contains at least one instance of the specified kind of string.</summary>
        Yes,
    }
}
