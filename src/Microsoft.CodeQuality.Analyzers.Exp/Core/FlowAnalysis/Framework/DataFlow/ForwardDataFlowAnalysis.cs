// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Subtype for all forward dataflow analyses.
    /// These analyses operate on the control flow graph starting from the entry block,
    /// flowing the dataflow values forward to the successor blocks until a fix point is reached.
    /// </summary>
    internal abstract class ForwardDataFlowAnalysis<TAnalysisData, TAnalysisResult, TAbstractAnalysisValue> : DataFlowAnalysis<TAnalysisData, TAnalysisResult, TAbstractAnalysisValue>
        where TAnalysisData: class
        where TAnalysisResult: class
    {
        protected ForwardDataFlowAnalysis(AbstractDomain<TAnalysisData> analysisDomain, DataFlowOperationWalker<TAnalysisData, TAbstractAnalysisValue> dataflowOperationWalker, DataFlowAnalysisResult<NullAnalysis.NullBlockAnalysisResult, NullAnalysis.NullAbstractValue> nullAnalysisResultOpt)
            : base (analysisDomain, dataflowOperationWalker, nullAnalysisResultOpt)
        {
        }
    }
}
