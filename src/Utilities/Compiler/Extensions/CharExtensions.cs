// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Analyzer.Utilities.Extensions
{
    internal static class CharExtensions
    {
        public static bool IsASCII(this char c) => (uint)c <= '\x007f';
    }
}
