// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    internal abstract partial class AbstractIndex
    {
        private sealed class ConstantValueIndex : AbstractIndex
        {
            public ConstantValueIndex(uint index)
            {
                Index = index;
            }

            public uint Index { get; }

            public override bool Equals(AbstractIndex other)
            {
                return other is ConstantValueIndex otherIndex &&
                    Index == otherIndex.Index;
            }

            public override int GetHashCode()
            {
                return HashUtilities.Combine(Index.GetHashCode(), nameof(ConstantValueIndex).GetHashCode());
            }
        }
    }
}
