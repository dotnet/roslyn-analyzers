// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    internal abstract partial class AbstractIndex
    {
        private sealed class OperationBasedIndex : AbstractIndex
        {
            public OperationBasedIndex(IOperation operation)
            {
                Debug.Assert(operation != null);
                Operation = operation;
            }

            public IOperation Operation { get; }

            public override bool Equals(AbstractIndex other)
            {
                return other is OperationBasedIndex otherIndex &&
                    Operation.Equals(otherIndex.Operation);
            }

            public override int GetHashCode()
            {
                return HashUtilities.Combine(Operation.GetHashCode(), nameof(OperationBasedIndex).GetHashCode());
            }
        }
    }
}
