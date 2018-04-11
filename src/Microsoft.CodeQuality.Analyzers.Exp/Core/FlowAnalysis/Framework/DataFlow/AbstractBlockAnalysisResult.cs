// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Result from execution of a <see cref="DataFlowAnalysis"/> on a basic block.
    /// </summary>
    internal abstract class AbstractBlockAnalysisResult<TAnalysisData, TAbstractAnalysisValue>
    {
        protected AbstractBlockAnalysisResult(BasicBlock basicBlock)
        {
            BasicBlock = basicBlock;
        }

        public BasicBlock BasicBlock { get; }
    }
}
