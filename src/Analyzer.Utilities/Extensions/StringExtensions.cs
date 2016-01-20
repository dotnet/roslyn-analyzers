using System;

namespace Analyzer.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static bool IsPlural(this string word)
        {
            if (!word.EndsWith("s", StringComparison.Ordinal) &&
                !word.EndsWith("i", StringComparison.Ordinal) &&
                !word.EndsWith("ae", StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }
    }
}
