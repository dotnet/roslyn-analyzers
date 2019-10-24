// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    /// <summary>
    /// The kind of array to be tainted.
    /// </summary>
    /// <remarks>If there are multiple unconsistent TaintArrayKinds for one array, the precedence is like All > Constant > None.</remarks>
    public enum TaintArrayKind
    {
        /// <summary>
        /// Not taint array.
        /// </summary>
        None,

        /// <summary>
        /// Only taint constant array.
        /// </summary>
        Constant,

        /// <summary>
        /// Taint all arrary.
        /// </summary>
        All,
    }
}
