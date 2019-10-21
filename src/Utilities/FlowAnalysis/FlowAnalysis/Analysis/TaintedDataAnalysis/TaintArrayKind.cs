// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
    public enum TaintArrayKind
    {
        /// <summary>
        /// Taint all arrary.
        /// </summary>
        All,

        /// <summary>
        /// Only taint constant array.
        /// </summary>
        Constant,

        /// <summary>
        /// Not taint array.
        /// </summary>
        None,
    }
}
