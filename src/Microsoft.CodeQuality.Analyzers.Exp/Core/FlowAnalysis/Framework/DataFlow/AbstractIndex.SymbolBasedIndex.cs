// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T> - CacheBasedEquatable handles equality

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    internal abstract partial class AbstractIndex
    {
        private sealed class AnalysisEntityBasedIndex : AbstractIndex
        {
            public AnalysisEntityBasedIndex(AnalysisEntity analysisEntity)
            {
                AnalysisEntity = analysisEntity;
            }

            public AnalysisEntity AnalysisEntity { get; }

            protected override int ComputeHashCode() => HashUtilities.Combine(AnalysisEntity.GetHashCode(), nameof(AnalysisEntityBasedIndex).GetHashCode());
        }
    }
}
