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
            MinimumOSPlatformAttribute,
            ObsoletedInOSPlatformAttribute,
            RemovedInOSPlatformAttribute,
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
            public bool Initialized => SupportedPlatforms != null;
            public SmallDictionary<string, PooledSortedSet<Version>> SupportedPlatforms { get; }
            public SmallDictionary<string, PooledSortedSet<Version>> UnsupportedPlatforms { get; }
            public SmallDictionary<string, Version> ObsoletedPlatforms { get; }
        }

        /* TODO : Might remove later
        private readonly struct PlatformAttributeInfo : IEquatable<PlatformAttributeInfo>
        {
            public PlatformAttributeType AttributeType { get; }
            public string PlatformName { get; }
            public Version Version { get; }

            private PlatformAttributeInfo(PlatformAttributeType attributeType, string platformName, Version version)
            {
                AttributeType = attributeType;
                PlatformName = platformName;
                Version = version;
            }

            public static bool TryParsePlatformAttributeInfo(AttributeData osAttribute, out PlatformAttributeInfo parsedAttribute)
            {
                if (!osAttribute.ConstructorArguments.IsEmpty &&
                    osAttribute.ConstructorArguments[0] is { } argument &&
                    argument.Kind == TypedConstantKind.Primitive &&
                    argument.Type.SpecialType == SpecialType.System_String &&
                    !argument.IsNull &&
                    !argument.Value.Equals(string.Empty) &&
                    TryParsePlatformNameAndVersion(osAttribute.ConstructorArguments[0].Value.ToString(), out string platformName, out Version? version))
                {
                    parsedAttribute = new PlatformAttributeInfo(SwitchAttrributeType(osAttribute.AttributeClass.Name), platformName, version);
                    return true;
                }
                parsedAttribute = default;
                return false;
            }

            private static PlatformAttributeType SwitchAttrributeType(string osAttributeName)
                => osAttributeName switch
                {
                    MinimumOSPlatformAttribute => PlatformAttributeType.MinimumOSPlatformAttribute,
                    ObsoletedInOSPlatformAttribute => PlatformAttributeType.ObsoletedInOSPlatformAttribute,
                    RemovedInOSPlatformAttribute => PlatformAttributeType.RemovedInOSPlatformAttribute,
                    _ => throw new NotImplementedException(),
                };

            public override bool Equals(object obj)
            {
                if (obj is PlatformAttributeInfo info)
                {
                    return Equals(info);
                }
                return false;
            }

            public override int GetHashCode() => HashUtilities.Combine(AttributeType.GetHashCode(), PlatformName.GetHashCode(), Version.GetHashCode());

            public static bool operator ==(PlatformAttributeInfo left, PlatformAttributeInfo right) => left.Equals(right);

            public static bool operator !=(PlatformAttributeInfo left, PlatformAttributeInfo right) => !(left == right);

            public bool Equals(PlatformAttributeInfo other) =>
                AttributeType == other.AttributeType && PlatformName.Equals(other.PlatformName, StringComparison.InvariantCultureIgnoreCase) && Version.Equals(other.Version);
        }*/

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
