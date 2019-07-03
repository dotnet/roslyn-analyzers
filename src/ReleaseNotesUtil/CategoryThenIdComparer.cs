using System;
using System.Collections.Generic;

namespace ReleaseNotesUtil
{
    /// <summary>
    /// Comparer for sorting rules by category, then by ID.
    /// </summary>
    internal class CategoryThenIdComparer : IComparer<RuleInfo>
    {
        public static CategoryThenIdComparer Instance = new CategoryThenIdComparer();

        public int Compare(RuleInfo x, RuleInfo y)
        {
            int c = String.Compare(x.Category, y.Category, StringComparison.InvariantCulture);
            if (c != 0)
            {
                return c;
            }

            return String.Compare(x.Id, y.Id, StringComparison.InvariantCulture);
        }
    }
}
