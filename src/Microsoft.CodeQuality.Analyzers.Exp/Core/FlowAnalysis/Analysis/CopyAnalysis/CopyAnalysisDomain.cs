// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis
{
    using CopyAnalysisData = IDictionary<AnalysisEntity, CopyAbstractValue>;

    internal partial class CopyAnalysis : ForwardDataFlowAnalysis<CopyAnalysisData, CopyBlockAnalysisResult, CopyAbstractValue>
    {
        /// <summary>
        /// An abstract analysis domain implementation <see cref="CopyAnalysis"/>.
        /// </summary>
        private class CopyAnalysisDomain : AnalysisEntityMapAbstractDomain<CopyAbstractValue>
        {
            public static readonly CopyAnalysisDomain Instance = new CopyAnalysisDomain(CopyAbstractValueDomain.Default);

            private CopyAnalysisDomain(AbstractValueDomain<CopyAbstractValue> valueDomain)
            : base(valueDomain)
            {
            }

            protected override CopyAbstractValue GetDefaultValue(AnalysisEntity analysisEntity) => new CopyAbstractValue(analysisEntity);
        }
    }
}