// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Analysis result from execution of <see cref="ExtraTypeCheckingAnalysis"/> on a control flow graph.
    /// </summary>
    internal sealed class ExtraTypeCheckingAnalysisResult : DataFlowAnalysisResult<ExtraTypeCheckingBlockAnalysisResult, SimpleAbstractValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysisResult"/> class.
        /// </summary>
        /// <param name="coreAnalysisResult">The core analysis result.</param>
        public ExtraTypeCheckingAnalysisResult(
            DataFlowAnalysisResult<ExtraTypeCheckingBlockAnalysisResult, SimpleAbstractValue> coreAnalysisResult)
            : base(coreAnalysisResult)
        {
        }
    }
}
