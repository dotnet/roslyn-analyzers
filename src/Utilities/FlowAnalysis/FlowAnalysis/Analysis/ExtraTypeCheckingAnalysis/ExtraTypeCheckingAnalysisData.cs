// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Represents analysis data.
    /// </summary>
    internal class ExtraTypeCheckingAnalysisData : AbstractAnalysisData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysisData"/> class.
        /// </summary>
        public ExtraTypeCheckingAnalysisData()
        {
            this.Data = new Dictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysisData"/> class.
        /// </summary>
        /// <param name="other">The object to clone.</param>
        public ExtraTypeCheckingAnalysisData(ExtraTypeCheckingAnalysisData other)
        {
            this.Data = new Dictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue>(other.Data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysisData"/> class.
        /// </summary>
        /// <param name="other">The object to clone.</param>
        public ExtraTypeCheckingAnalysisData(ImmutableDictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> other)
        {
            this.Data = new Dictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue>(other);
        }

        /// <summary>
        /// Gets the analysis data.
        /// </summary>
        public IDictionary<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> Data { get; }
    }
}
