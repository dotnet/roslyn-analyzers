// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    /// <summary>
    /// Simple value kind.
    /// </summary>
    internal enum SimpleAbstractValueKind
    {
        /// <summary>
        /// Indicates locations that are not relevant accesses.
        /// </summary>
        None,

        /// <summary>
        /// Indicates a relevant access based on analysis type.
        /// </summary>
        Access,

        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown,
    }
}
