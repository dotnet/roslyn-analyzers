// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis
{
    /// <summary>
    /// An abstract domain implementation for analyses that store dictionary typed data.
    /// </summary>
    internal class PointsToAnalysisDomain: AnalysisEntityMapAbstractDomain<PointsToAbstractValue>
    {
        private readonly DefaultPointsToValueGenerator _defaultPointsToValueGenerator;

        public PointsToAnalysisDomain(DefaultPointsToValueGenerator defaultPointsToValueGenerator, AbstractValueDomain<PointsToAbstractValue> valueDomain)
            : base(valueDomain)
        {
            _defaultPointsToValueGenerator = defaultPointsToValueGenerator;
        }

        protected override PointsToAbstractValue GetDefaultValue(AnalysisEntity analysisEntity)
            => _defaultPointsToValueGenerator.GetOrCreateDefaultValue(analysisEntity);
    }
}