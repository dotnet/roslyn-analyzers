// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    internal partial class ThreadDependencyAnalysis
    {
        internal class Result : DataFlowAnalysisResult<BlockResult, Value>
        {
            internal Result(DataFlowAnalysisResult<BlockResult, Value> other)
                : base(other)
            {
            }
        }
    }
}
