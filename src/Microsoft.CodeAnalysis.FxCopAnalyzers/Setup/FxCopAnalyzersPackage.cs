// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace FxCopAnalyzersSetup
{
    [Guid(PackageGuid)]
#pragma warning disable CA1812
    class FxCopAnalyzersPackage : Package
#pragma warning restore CA1812
    {
        private const string PackageGuid = "4A41D270-A97F-4639-A352-28732FC410E4";
    }
}
