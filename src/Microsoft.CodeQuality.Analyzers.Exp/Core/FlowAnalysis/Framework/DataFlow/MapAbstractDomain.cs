// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations.DataFlow
{
    /// <summary>
    /// An abstract domain implementation for analyses that store dictionary typed data.
    /// </summary>
    internal class MapAbstractDomain<TKey, TValue> : AbstractDomain<IDictionary<TKey, TValue>>
    {
        private AbstractDomain<TValue> _valueDomain;

        public MapAbstractDomain(AbstractDomain<TValue> valueDomain)
        {
            _valueDomain = valueDomain;
        }

        public override IDictionary<TKey, TValue> Bottom => new Dictionary<TKey, TValue>();

        /// <summary>
        /// Compares if the abstract dataflow values in <paramref name="oldValue"/> against the values in <paramref name="newValue"/> to ensure
        /// dataflow function is a monotically increasing function. See https://en.wikipedia.org/wiki/Monotonic_function for understanding monotonic functions.
        /// <returns>
        /// 1) 0, if both the dictionaries are identical.
        /// 2) -1, if dictionaries are not identical and for every key in <paramref name="oldValue"/>, the corresponding key exists in <paramref name="newValue"/> and
        ///    the value of each such key in <paramref name="oldValue"/> is lesser than or equals the value in <paramref name="newValue"/>.
        /// 3) 1, otherwise.
        /// </returns>
        public override int Compare(IDictionary<TKey, TValue> oldValue, IDictionary<TKey, TValue> newValue)
        {
            if (oldValue == null && newValue != null)
            {
                return -1;
            }
            
            if (oldValue != null && newValue == null)
            {
                return 1;
            }

            if (oldValue == null || ReferenceEquals(oldValue, newValue))
            {
                return 0;
            }

            // Ensure that every key in oldValue exists in newValue and the value corresponding to that key
            // is not greater in oldValue as compared to the value in newValue
            bool newValueIsBigger = false;
            foreach (var kvp in oldValue)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                if (!newValue.TryGetValue(key, out TValue otherValue))
                {
                    return 1;
                }

                var result = _valueDomain.Compare(value, otherValue);

                if (result > 0)
                {
                    return 1;
                }
                else if (result < 0)
                {
                    newValueIsBigger = true;
                }
            }

            return newValueIsBigger ? -1 : 0;
        }

        public override IDictionary<TKey, TValue> Merge(IDictionary<TKey, TValue> value1, IDictionary<TKey, TValue> value2)
        {
            if (value1 == null && value2 != null)
            {
                return value2;
            }

            if (value1 != null && value2 == null)
            {
                return value1;
            }

            if (value1 == null)
            {
                return null;
            }

            var result = new Dictionary<TKey, TValue>(value1);
            foreach (var entry in value2)
            {
                if (result.TryGetValue(entry.Key, out TValue value))
                {
                    value = _valueDomain.Merge(value, entry.Value);

                    if (value != null)
                    {
                        result[entry.Key] = value;
                    }
                    else
                    {
                        result.Remove(entry.Key);
                    }
                }
                else
                {
                    result.Add(entry.Key, entry.Value);
                }
            }

            return result;
        }
    }
}