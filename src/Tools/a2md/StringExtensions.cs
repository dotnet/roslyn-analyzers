using System.Collections.Generic;

namespace a2md
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values);
    }
}
