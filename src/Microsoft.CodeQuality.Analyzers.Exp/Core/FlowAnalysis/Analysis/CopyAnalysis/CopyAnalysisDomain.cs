// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis
{
    using CopyAnalysisData = IDictionary<AnalysisEntity, CopyAbstractValue>;

    internal partial class CopyAnalysis : ForwardDataFlowAnalysis<CopyAnalysisData, CopyBlockAnalysisResult, CopyAbstractValue>
    {
        /// <summary>
        /// An abstract analysis domain implementation <see cref="CopyAnalysis"/>.
        /// </summary>
        private class CopyAnalysisDomain : MapAbstractDomain<AnalysisEntity, CopyAbstractValue>
        {
            public static readonly CopyAnalysisDomain Instance = new CopyAnalysisDomain(CopyAbstractValueDomain.Default);

            private CopyAnalysisDomain(AbstractValueDomain<CopyAbstractValue> valueDomain)
            : base(valueDomain)
            {
            }

            protected override CopyAnalysisData MergeCore(CopyAnalysisData map1, CopyAnalysisData map2)
            {
                Debug.Assert(map1 != null);
                Debug.Assert(map2 != null);
                AssertValidCopyAnalysisData(map1);
                AssertValidCopyAnalysisData(map2);

                var result = new Dictionary<AnalysisEntity, CopyAbstractValue>();
                foreach (var kvp in map1)
                {
                    var key = kvp.Key;
                    var value1 = kvp.Value;

                    // If the key exists in both maps, use the merged value.
                    // Otherwise, use the default value.
                    CopyAbstractValue mergedValue;
                    if (map2.TryGetValue(key, out var value2))
                    {
                        mergedValue = ValueDomain.Merge(value1, value2);
                    }
                    else
                    {
                        mergedValue = GetDefaultValue(key);
                    }

                    result.Add(key, mergedValue);
                }

                foreach (var kvp in map2)
                {
                    if (!result.ContainsKey(kvp.Key))
                    {
                        result.Add(kvp.Key, GetDefaultValue(kvp.Key));
                    }
                }

                AssertValidCopyAnalysisData(result);
                return result;

                CopyAbstractValue GetDefaultValue(AnalysisEntity analysisEntity) => new CopyAbstractValue(analysisEntity);
            }
        }
    }
}