// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace AnalyzersStatusGenerator
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values);
    }
}
