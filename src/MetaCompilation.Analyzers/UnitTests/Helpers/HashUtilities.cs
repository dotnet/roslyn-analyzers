using System;
using System.Collections.Generic;

namespace Roslyn.Utilities
{
    internal static class Hash
    {
        internal static int CombineValues<T>(IEnumerable<T> values, int maxItemsToHash = int.MaxValue)
        {
            if (values == null)
            {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values)
            {
                if (count++ >= maxItemsToHash)
                {
                    break;
                }

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null)
                {
                    hashCode = Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues(IEnumerable<string> values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue)
        {
            if (values == null)
            {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values)
            {
                if (count++ >= maxItemsToHash)
                {
                    break;
                }

                if (value != null)
                {
                    hashCode = Combine(stringComparer.GetHashCode(value), hashCode);
                }
            }

            return hashCode;
        }

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        internal static int Combine(int newKey, int currentKey)
        {
            return unchecked((currentKey * (int)0xA5555529) + newKey);
        }

        internal static int Combine(bool newKeyPart, int currentKey)
        {
            return Combine(currentKey, newKeyPart ? 1 : 0);
        }
    }
}