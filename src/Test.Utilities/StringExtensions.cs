﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Test.Utilities
{
    public static class StringExtensions
    {
        public static string NormalizeLineEndings(this string input)
            => Regex.Replace(input, "(?<!\r)\n", "\r\n");
    }
}
