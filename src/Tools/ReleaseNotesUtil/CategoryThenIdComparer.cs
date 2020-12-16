// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ReleaseNotesUtil
{
    /// <summary>
    /// Comparer for sorting rules by category, then by ID.
    /// </summary>
    internal class CategoryThenIdComparer : IComparer<RuleInfo>
    {
        public static CategoryThenIdComparer Instance = new();

        public int Compare([AllowNull] RuleInfo x, [AllowNull] RuleInfo y)
        {
            int c = string.Compare(x?.Category, y?.Category, StringComparison.InvariantCulture);
            if (c != 0)
            {
                return c;
            }

            return string.Compare(x?.Id, y?.Id, StringComparison.InvariantCulture);
        }
    }
}
