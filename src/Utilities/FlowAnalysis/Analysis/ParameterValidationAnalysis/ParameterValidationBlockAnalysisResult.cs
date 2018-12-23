// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ParameterValidationAnalysis
{
    using ParameterValidationAnalysisData = DictionaryAnalysisData<AbstractLocation, ParameterValidationAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="ParameterValidationAnalysis"/> on a basic block.
    /// It stores ParameterValidation values for each <see cref="AbstractLocation"/> at the start and end of the basic block.
    /// </summary>
    internal class ParameterValidationBlockAnalysisResult : AbstractBlockAnalysisResult<AbstractLocation, ParameterValidationAbstractValue>
    {
        public ParameterValidationBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<ParameterValidationAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input, blockAnalysisData.Output)
        {
        }
    }
}
