// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis
{
    using PropertySetAnalysisData = DictionaryAnalysisData<AbstractLocation, PropertySetAbstractValue>;

    /// <summary>
    /// Result from execution of <see cref="PropertySetAnalysis"/> on a basic block.
    /// It stores BinaryFormatter values for each <see cref="AbstractLocation"/> at the start and end of the basic block.
    /// </summary>
    internal class PropertySetBlockAnalysisResult : AbstractBlockAnalysisResult<AbstractLocation, PropertySetAbstractValue>
    {
        public PropertySetBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<PropertySetAnalysisData> blockAnalysisData)
            : base(basicBlock, blockAnalysisData.Input, blockAnalysisData.Output)
        {
        }
    }
}
