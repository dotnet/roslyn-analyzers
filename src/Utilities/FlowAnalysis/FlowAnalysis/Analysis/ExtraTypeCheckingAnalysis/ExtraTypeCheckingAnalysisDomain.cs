// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Represents the ExtraDictionaryAccessAnalysisDomain.
    /// </summary>
    internal class ExtraTypeCheckingAnalysisDomain : AbstractAnalysisDomain<ExtraTypeCheckingAnalysisData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraTypeCheckingAnalysisDomain"/> class.
        /// </summary>
        /// <param name="valueDomain">The value domain.</param>
        public ExtraTypeCheckingAnalysisDomain(AbstractValueDomain<SimpleAbstractValue> valueDomain)
        {
            this.ValueDomain = valueDomain;
        }

        /// <summary>
        /// Gets the value domain.
        /// </summary>
        private AbstractValueDomain<SimpleAbstractValue> ValueDomain { get; }

        /// <inheritdoc />
        public override ExtraTypeCheckingAnalysisData Clone(ExtraTypeCheckingAnalysisData value)
        {
            return new ExtraTypeCheckingAnalysisData(value);
        }

        /// <inheritdoc />
        public override int Compare(
            ExtraTypeCheckingAnalysisData oldValue,
            ExtraTypeCheckingAnalysisData newValue)
        {
            return this.Compare(oldValue, newValue, assertMonotonicity: true);
        }

        /// <inheritdoc />
        public override bool Equals(
            ExtraTypeCheckingAnalysisData value1,
            ExtraTypeCheckingAnalysisData value2)
        {
            return this.Compare(value1, value2, assertMonotonicity: false) == 0;
        }

        /// <inheritdoc />
        public override ExtraTypeCheckingAnalysisData Merge(
            ExtraTypeCheckingAnalysisData value1,
            ExtraTypeCheckingAnalysisData value2)
        {
            ExtraTypeCheckingAnalysisData result = new ExtraTypeCheckingAnalysisData();

            foreach (KeyValuePair<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> kvp in value2.Data)
            {
                if (value1.Data.TryGetValue(kvp.Key, out SimpleAbstractValue value))
                {
                    value = this.ValueDomain.Merge(value, kvp.Value);

                    if (value != null)
                    {
                        result.Data[kvp.Key] = value;
                    }
                    else
                    {
                        result.Data.Remove(kvp.Key);
                    }
                }
                else
                {
                    result.Data.Add(kvp);
                }
            }

            foreach (KeyValuePair<ExtraTypeCheckingAbstractLocation, SimpleAbstractValue> kvp in value1.Data)
            {
                if (!value2.Data.TryGetValue(kvp.Key, out SimpleAbstractValue _))
                {
                    // This was in the first dictionary only.
                    result.Data.Add(kvp);
                }
            }

            return result;
        }

#pragma warning disable CA1030 // Consider an event.
        /// <summary>
        /// Asserts monotonicity in debug builds.
        /// </summary>
        /// <param name="assertMonotonicity">True if assertion should be done.</param>
        [Conditional("DEBUG")]
        private static void RaiseNonMonotonicAssertIfNeeded(bool assertMonotonicity)
        {
            if (assertMonotonicity)
            {
                Debug.Fail("Non-monotonic merge");
            }
        }
#pragma warning restore CA1030

        /// <summary>
        /// Compares sorting order for objects.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="assertMonotonicity">True if monotonicity is to be asserted.</param>
        /// <returns>-1, 0, 1 for relative comparison.</returns>
        private int Compare(
            ExtraTypeCheckingAnalysisData oldValue,
            ExtraTypeCheckingAnalysisData newValue,
            bool assertMonotonicity)
        {
            if (ReferenceEquals(oldValue, newValue))
            {
                return 0;
            }

            if (newValue.Data.Count < oldValue.Data.Count)
            {
                RaiseNonMonotonicAssertIfNeeded(assertMonotonicity);
                return 1;
            }

            // Ensure that every key in oldValue exists in newValue and the value corresponding to that key
            // is not greater in oldValue as compared to the value in newValue
            bool newValueIsBigger = false;
            foreach (var kvp in oldValue.Data)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                if (!newValue.Data.TryGetValue(key, out SimpleAbstractValue otherValue))
                {
                    RaiseNonMonotonicAssertIfNeeded(assertMonotonicity);
                    return 1;
                }

                var result = this.ValueDomain.Compare(value, otherValue, assertMonotonicity);

                if (result > 0)
                {
                    RaiseNonMonotonicAssertIfNeeded(assertMonotonicity);
                    return 1;
                }
                else if (result < 0)
                {
                    newValueIsBigger = true;
                }
            }

            if (!newValueIsBigger)
            {
                newValueIsBigger = newValue.Data.Count > oldValue.Data.Count;
            }

            return newValueIsBigger ? -1 : 0;
        }
    }
}
