// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Operations.ControlFlow;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// Subtype for all backward dataflow analyses.
    /// These analyses operate on the control flow graph starting from the exit block,
    /// flowing the dataflow values backward to the predecessor blocks until a fix point is reached.
    /// </summary>
    internal abstract class BackwardDataFlowAnalysis<TAnalysisData, TAnalysisResult, TAbstractAnalysisValue> : DataFlowAnalysis<TAnalysisData, TAnalysisResult, TAbstractAnalysisValue>
        where TAnalysisData : class
        where TAnalysisResult : AbstractBlockAnalysisResult<TAnalysisData, TAbstractAnalysisValue>
    {
        protected BackwardDataFlowAnalysis(AbstractAnalysisDomain<TAnalysisData> analysisDomain, DataFlowOperationVisitor<TAnalysisData, TAbstractAnalysisValue> operationVisitor)
            : base(analysisDomain, operationVisitor)
        {
        }

        [DebuggerStepThrough]
        protected override BasicBlock GetEntry(ControlFlowGraph cfg) => cfg.Exit;

        [DebuggerStepThrough]
        protected override IEnumerable<BasicBlock> GetPredecessors(BasicBlock block) => block.Successors;

        [DebuggerStepThrough]
        protected override IEnumerable<BasicBlock> GetSuccessors(BasicBlock block) => block.Predecessors;

        [DebuggerStepThrough]
        protected override TAnalysisData GetInput(DataFlowAnalysisInfo<TAnalysisData> result) => result.Output;

        [DebuggerStepThrough]
        protected override TAnalysisData GetOutput(DataFlowAnalysisInfo<TAnalysisData> result) => result.Input;

        [DebuggerStepThrough]
        protected override void UpdateInput(DataFlowAnalysisResultBuilder<TAnalysisData> builder, BasicBlock block, TAnalysisData newInput) =>
            base.UpdateOutput(builder, block, newInput);

        [DebuggerStepThrough]
        protected override void UpdateOutput(DataFlowAnalysisResultBuilder<TAnalysisData> builder, BasicBlock block, TAnalysisData newOutput) =>
            base.UpdateInput(builder, block, newOutput);
    }
}
