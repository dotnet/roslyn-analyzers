// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Analyzer.Utilities.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///  Note: This doesnot handle all cases of plural for example "Men", "Vertices" etc
        ///  Word ending with 'i' and 'ae' is to avoid irregular plurals 
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static bool IsPlural(this string word)
        {
            if (!word.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
                !word.EndsWith("i", StringComparison.OrdinalIgnoreCase) &&
                !word.EndsWith("ae", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}
