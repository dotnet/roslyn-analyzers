// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    /// <summary>
    /// String state for presence of non-literal values for <see cref="AnalysisEntity"/>/<see cref="IOperation"/> tracked by <see cref="StringContentAnalysis"/>.
    /// </summary>
    internal enum StringContainsNonLiteralState
    {
        /// <summary>State is undefined.</summary>
        Undefined = 0,
        /// <summary>The variable does not contain any instances of non-literal string.</summary>
        No,
        /// <summary>The variable contains at least one instance of a non-literal string.</summary>
        Yes,
        /// <summary>The variable may or may not contain instances of a non-literal string.</summary>
        Maybe,

    }
}
