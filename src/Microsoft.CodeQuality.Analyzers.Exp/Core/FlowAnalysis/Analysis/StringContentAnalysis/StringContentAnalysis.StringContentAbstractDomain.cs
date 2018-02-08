// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis
{
    using StringContentAnalysisData = IDictionary<AnalysisEntity, StringContentAbstractValue>;

    internal partial class StringContentAnalysis : ForwardDataFlowAnalysis<StringContentAnalysisData, StringContentBlockAnalysisResult, StringContentAbstractValue>
    {
        /// <summary>
        /// Abstract value domain for <see cref="StringContentAnalysis"/> to merge and compare <see cref="StringContentAbstractValue"/> values.
        /// </summary>
        private sealed class StringContentAbstractValueDomain : AbstractDomain<StringContentAbstractValue>
        {
            public static StringContentAbstractValueDomain Default = new StringContentAbstractValueDomain();

            private StringContentAbstractValueDomain() { }

            public override StringContentAbstractValue Bottom => StringContentAbstractValue.UndefinedState;

            public override int Compare(StringContentAbstractValue oldValue, StringContentAbstractValue newValue)
            {
                return Comparer<StringContentAbstractValue>.Default.Compare(oldValue, newValue);
            }

            public override StringContentAbstractValue Merge(StringContentAbstractValue value1, StringContentAbstractValue value2)
            {
                return value1.Merge(value2);
            }
        }
    }
}
