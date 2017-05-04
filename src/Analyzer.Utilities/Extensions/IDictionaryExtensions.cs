using System.Collections.Generic;

namespace Analyzer.Utilities.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void AddKeyValueIfNotNull<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value)
        {
            if (key != null && value != null)
            {
                dictionary.Add(key, value);
            }
        }
    }
}
