// StringContentright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;

    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        /// <summary>
        /// An abstract analysis domain implementation <see cref="StringContentAnalysis"/>.
        /// </summary>
        private class StringContentAnalysisDomain : AnalysisEntityMapAbstractDomain<StringContentAbstractValue>
        {
            public static readonly StringContentAnalysisDomain Instance = new StringContentAnalysisDomain(StringContentAbstractValueDomain.Default);

            private StringContentAnalysisDomain(AbstractValueDomain<StringContentAbstractValue> valueDomain) : base(valueDomain)
            {
            }

            protected override StringContentAbstractValue GetDefaultValue(AnalysisEntity analysisEntity) => StringContentAbstractValue.MayBeContainsNonLiteralState;
            protected override bool CanSkipNewEntry(AnalysisEntity analysisEntity, StringContentAbstractValue value) => value.NonLiteralState == StringContainsNonLiteralState.Maybe;
        }
    }
}