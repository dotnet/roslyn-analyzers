// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Analyzer.Utilities;

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T> - CacheBasedEquatable handles equality

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

            protected override int ComputeHashCode() => HashUtilities.Combine(Operation.GetHashCode(), nameof(OperationBasedIndex).GetHashCode());
        }
    }
}
