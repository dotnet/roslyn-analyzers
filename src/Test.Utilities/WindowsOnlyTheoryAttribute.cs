﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;
using Xunit;

namespace Test.Utilities
{
    public sealed class WindowsOnlyTheoryAttribute : TheoryAttribute
    {
        public WindowsOnlyTheoryAttribute()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.Skip = "This test requires Windows to run";
            }
        }
    }
}
