// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;

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

            public override bool Equals(AbstractIndex other)
            {
                return other is AnalysisEntityBasedIndex otherIndex &&
                    AnalysisEntity.Equals(otherIndex.AnalysisEntity);
            }

            public override int GetHashCode()
            {
                return HashUtilities.Combine(AnalysisEntity.GetHashCode(), nameof(AnalysisEntityBasedIndex).GetHashCode());
            }
        }
    }
}
