// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
    /// <summary>
    /// Result from execution of a <see cref="DataFlowAnalysis"/> on a basic block.
    /// </summary>
    internal abstract class AbstractBlockAnalysisResult<TKey, TValue> : AbstractBlockAnalysisResult
    {
        private readonly DictionaryAnalysisData<TKey, TValue> _inputDataOpt;
        private readonly DictionaryAnalysisData<TKey, TValue> _outputDataOpt;

        protected AbstractBlockAnalysisResult(
            BasicBlock basicBlock,
            DictionaryAnalysisData<TKey, TValue> inputDataOpt,
            DictionaryAnalysisData<TKey, TValue> outputDataOpt)
            : base (basicBlock)
        {
            _inputDataOpt = inputDataOpt != null ? new DictionaryAnalysisData<TKey, TValue>(inputDataOpt) : null;
            _outputDataOpt = outputDataOpt != null ? new DictionaryAnalysisData<TKey, TValue>(outputDataOpt) : null;
        }

        public IDictionary<TKey, TValue> InputData => _inputDataOpt ?? (IDictionary<TKey, TValue>)ImmutableDictionary<TKey, TValue>.Empty;
        public IDictionary<TKey, TValue> OutputData => _outputDataOpt ?? (IDictionary<TKey, TValue>)ImmutableDictionary<TKey, TValue>.Empty;

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                _inputDataOpt?.Dispose();
                _outputDataOpt?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
