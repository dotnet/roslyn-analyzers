// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    using PointsToAnalysisData = IDictionary<AnalysisEntity, PointsToAbstractValue>;

    /// <summary>
    /// An abstract analysis domain implementation <see cref="PointsToAnalysis"/>.
    /// </summary>
    internal class PointsToAnalysisDomain: AnalysisEntityMapAbstractDomain<PointsToAbstractValue>
    {
        public PointsToAnalysisDomain(DefaultPointsToValueGenerator defaultPointsToValueGenerator, AbstractValueDomain<PointsToAbstractValue> valueDomain)
            : base(valueDomain)
        {
            DefaultPointsToValueGenerator = defaultPointsToValueGenerator;
        }

        public DefaultPointsToValueGenerator DefaultPointsToValueGenerator { get; }

        protected override PointsToAbstractValue GetDefaultValue(AnalysisEntity analysisEntity) => DefaultPointsToValueGenerator.GetOrCreateDefaultValue(analysisEntity);

        public PointsToAnalysisData MergeAnalysisDataForBackEdge(PointsToAnalysisData map1, PointsToAnalysisData map2)
        {
            // Stop tracking points to values present in both branches.
            List<AnalysisEntity> keysInMap1 = map1.Keys.ToList();
            foreach (var key in keysInMap1)
            {
                if (map2.TryGetValue(key, out var value2) &&
                    value2 != map1[key])
                {
                    map1[key] = PointsToAbstractValue.Unknown;
                }
            }

            return Merge(map1, map2);
        }
    }
}