// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    public sealed partial class PlatformCompatabilityAnalyzer
    {
        private enum PlatformAttributeType
        {
            SupportedOSPlatformAttribute,
            ObsoletedInOSPlatformAttribute,
            UnsupportedOSPlatformAttribute,
        }

        private class PlatformAttributes
        {
            public Version? Obsoleted { get; set; }
            public Version? SupportedFirst { get; set; }
            public Version? SupportedSecond { get; set; }
            public Version? UnsupportedFirst { get; set; }
            public Version? UnsupportedSecond { get; set; }
            public bool HasAttribute() => SupportedFirst != null || UnsupportedFirst != null || SupportedSecond != null || UnsupportedSecond != null || Obsoleted != null;
        }

        private static bool TryParsePlatformNameAndVersion(string osString, out string osPlatformName, [NotNullWhen(true)] out Version? version)
        {
            version = null;
            osPlatformName = string.Empty;
            for (int i = 0; i < osString.Length; i++)
            {
                if (char.IsDigit(osString[i]))
                {
                    if (i > 0 && Version.TryParse(osString.Substring(i), out Version? parsedVersion))
                    {
                        osPlatformName = osString.Substring(0, i);
                        version = parsedVersion;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            osPlatformName = osString;
            version = new Version(0, 0);
            return true;
        }
    }
}
