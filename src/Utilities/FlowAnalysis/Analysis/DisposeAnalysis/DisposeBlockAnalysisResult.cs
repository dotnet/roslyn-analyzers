// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis
{
    using DisposeAnalysisData = DictionaryAnalysisData<AbstractLocation, DisposeAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="DisposeAnalysis"/> on a basic block.
    /// It store dispose values for each <see cref="AbstractLocation"/> at the start and end of the basic block.
    /// </summary>
    internal class DisposeBlockAnalysisResult : AbstractBlockAnalysisResult<AbstractLocation, DisposeAbstractValue>
    {
        public DisposeBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<DisposeAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input, blockAnalysisData.Output)
        {
        }
    }
}
