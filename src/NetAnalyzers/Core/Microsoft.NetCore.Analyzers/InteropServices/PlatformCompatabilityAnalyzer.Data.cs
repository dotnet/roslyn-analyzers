// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    public sealed partial class PlatformCompatabilityAnalyzer
    {
        /// <summary>
        /// Class used for keeping platform information of an API, all are optional properties
        /// 
        /// We need to keep only 2 values for [SupportedOSPlatform] attribute, first one will be the lowest version found, mostly for assembly level attribute which denotes when the API first introduced,
        /// second one would keep new APIs added later and requries higher platform version (if there is multiple version found in the API parents chain we will keep only highest version)
        /// 
        /// Same for [UnsupportedOSPlatform] attribute, an API could be unsupported at first and then start supported from some version then eventually removed. 
        /// So we only keep at most 2 versions of [UnsupportedOSPlatform] first one will be the lowest version found, second one will be second lowest if there is any
        /// 
        /// I wouldn't expect that [ObsoletedInOSPlatform] attribute used more than once (like obsoleted once, supported back and obsoleted again),
        /// so we will keep only one property for that, if any more attrbite found in the API parents chain we will keep the one with lowest versions
        /// 
        /// Properties:
        ///  - SupportedFirst - keeps lowest version of [SupportedOSPlatform] attribute found
        ///  - SupportedSecond - keeps the highest version of [SupportedOSPlatform] attribute if there is any
        ///  - UnsupportedFirst - keeps the lowest version of [UnsupportedOSPlatform] attribute found
        ///  - UnsupportedSecond - keeps the second lowest version of [UnsupportedOSPlatform] attribute found
        ///  - Obsoleted - keeps lowest version of [ObsoletedInOSPlatform] attrbite found
        /// </summary>
        private class PlatformAttributes
        {
            public Version? Obsoleted { get; set; }
            public Version? SupportedFirst { get; set; }
            public Version? SupportedSecond { get; set; }
            public Version? UnsupportedFirst { get; set; }
            public Version? UnsupportedSecond { get; set; }
            public bool HasAttribute() => SupportedFirst != null || UnsupportedFirst != null ||
                        SupportedSecond != null || UnsupportedSecond != null || Obsoleted != null;
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

                    return false;
                }
            }

            osPlatformName = osString;
            version = new Version(0, 0);
            return true;
        }
    }
}
