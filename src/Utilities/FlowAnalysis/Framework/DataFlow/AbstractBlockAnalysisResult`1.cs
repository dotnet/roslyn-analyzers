// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
    /// <summary>
    /// Result from execution of a <see cref="DataFlowAnalysis"/> on a basic block.
    /// </summary>
    internal abstract class AbstractBlockAnalysisResult<TAnalysisData> : AbstractBlockAnalysisResult
        where TAnalysisData : AbstractAnalysisData
    {
        private readonly DataFlowAnalysisInfo<TAnalysisData> _blockAnalysisData;
        protected AbstractBlockAnalysisResult(BasicBlock basicBlock, DataFlowAnalysisInfo<TAnalysisData> blockAnalysisData)
            : base(basicBlock)
        {
            Debug.Assert(blockAnalysisData.Input == null || !blockAnalysisData.Input.IsDisposed);
            Debug.Assert(blockAnalysisData.Output == null || !blockAnalysisData.Output.IsDisposed);
            _blockAnalysisData = blockAnalysisData;
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                _blockAnalysisData.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
