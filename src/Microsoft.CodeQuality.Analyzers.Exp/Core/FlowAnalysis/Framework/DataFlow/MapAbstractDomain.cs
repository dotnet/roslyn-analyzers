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

        public override int Compare(IDictionary<TKey, TValue> oldValue, IDictionary<TKey, TValue> newValue)
        {
            if ((oldValue == null && newValue != null) ||
                (oldValue != null && newValue == null))
            {
                return -1;
            }

            if (oldValue == null && newValue == null || ReferenceEquals(oldValue, newValue))
            {
                return 0;
            }

            if (oldValue.Count != newValue.Count)
            {
                return -1;
            }

            foreach (var kvp in oldValue)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                if (!newValue.TryGetValue(key, out TValue otherValue))
                {
                    return -1;
                }

                var valuesAreEquals = _valueDomain.Compare(value, otherValue);

                // old < new ?
                if (valuesAreEquals < 0)
                {
                    return -1;
                }
            }

            return 0;
        }

        public override IDictionary<TKey, TValue> Merge(IDictionary<TKey, TValue> value1, IDictionary<TKey, TValue> value2)
        {
            if (value1 == null && value2 != null) return value2;
            if (value1 != null && value2 == null) return value1;
            if (value1 == null && value2 == null) return null;

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