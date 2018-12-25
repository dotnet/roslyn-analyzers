// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
    /// <summary>
    /// Result from execution of a <see cref="DataFlowAnalysis"/> on a basic block.
    /// </summary>
    internal abstract class AbstractBlockAnalysisResult : IDisposable
    {
        protected AbstractBlockAnalysisResult(BasicBlock basicBlock)
        {
            BasicBlock = basicBlock;
        }

        public BasicBlock BasicBlock { get; }

        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly - We want to ensure that we cleanup managed resources even when object was not explicitly disposed.
        ~AbstractBlockAnalysisResult()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            Dispose(true);  // We want to explicitly cleanup managed resources, so pass 'true'
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
