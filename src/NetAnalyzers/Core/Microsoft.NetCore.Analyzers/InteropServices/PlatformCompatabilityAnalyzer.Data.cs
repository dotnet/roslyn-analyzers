// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;

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

#pragma warning disable CA1815 // Override equals and operator equals on value types
        private readonly struct PlatformAttributes
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public PlatformAttributes(SmallDictionary<string, PooledSortedSet<Version>> supportedPlatforms,
                SmallDictionary<string, PooledSortedSet<Version>> unsupportedPlatforms,
                SmallDictionary<string, Version> obsoletedPlatforms)
            {
                SupportedPlatforms = supportedPlatforms;
                UnsupportedPlatforms = unsupportedPlatforms;
                ObsoletedPlatforms = obsoletedPlatforms;
            }
            public bool HasAttribute => SupportedPlatforms.Any() || UnsupportedPlatforms.Any() || ObsoletedPlatforms.Any();
            public SmallDictionary<string, PooledSortedSet<Version>> SupportedPlatforms { get; }
            public SmallDictionary<string, PooledSortedSet<Version>> UnsupportedPlatforms { get; }
            public SmallDictionary<string, Version> ObsoletedPlatforms { get; }
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
