// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ExtraTypeCheckingAnalysis
{
    using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

    /// <summary>
    /// Abstract value domain for various Analysis classes to merge and compare <see cref="SimpleAbstractValue"/> values.
    /// </summary>
    internal class SimpleAbstractValueDomain : AbstractValueDomain<SimpleAbstractValue>
    {
        /// <summary>
        /// Gets the default value domain.
        /// </summary>
        public static SimpleAbstractValueDomain Default { get; } = new SimpleAbstractValueDomain();

        /// <summary>
        /// Gets the bottom of the data flow.
        /// </summary>
        public override SimpleAbstractValue Bottom => SimpleAbstractValue.None;

        /// <summary>
        /// Gets the value that represents unknown or default value. This is the top value.
        /// </summary>
        public override SimpleAbstractValue UnknownOrMayBeValue => SimpleAbstractValue.Unknown;

        /// <summary>
        /// Compares two abstract values to determine which is higher or lower.
        /// </summary>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">new value.</param>
        /// <param name="assertMonotonicity">True if monotonicity should be asserted.</param>
        /// <returns>Returns standard compare result.</returns>
        public override int Compare(
            SimpleAbstractValue oldValue,
            SimpleAbstractValue newValue,
            bool assertMonotonicity)
        {
            if (oldValue == null ||
                newValue == null)
            {
                return 0;
            }

            int left = (int)oldValue.Kind;
            int right = (int)newValue.Kind;

            int result = left.CompareTo(right);
            if (result == 0)
            {
                int oldCount = oldValue.GetPreviousLocationCount();
                int newCount = newValue.GetPreviousLocationCount();
                result = oldCount.CompareTo(newCount);
            }

            return result;
        }

        /// <summary>
        /// Performs a merge of two abstract values on two flow branches.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>The merged abstract value.</returns>
        public override SimpleAbstractValue Merge(
            SimpleAbstractValue value1,
            SimpleAbstractValue value2)
        {
            if (object.ReferenceEquals(value1, value2))
            {
                return value1;
            }

            if (value1 == null)
            {
                return value2;
            }

            if (value2 == null)
            {
                return value1;
            }

            int locationCount1 = value1.GetPreviousLocationCount();
            int locationCount2 = value2.GetPreviousLocationCount();

            if (value1.Kind == value2.Kind)
            {
                if (locationCount1 > locationCount2)
                {
                    return value1;
                }

                if (locationCount2 > locationCount1)
                {
                    return value2;
                }
            }

            if (value1.Kind == SimpleAbstractValueKind.None)
            {
                return value2;
            }

            if (value2.Kind == SimpleAbstractValueKind.None)
            {
                return value2;
            }

            // Value1 should be an Access. Value2 is likely an access as well so returning
            // value1 is appropriate.
            return value1;
        }
    }
}
