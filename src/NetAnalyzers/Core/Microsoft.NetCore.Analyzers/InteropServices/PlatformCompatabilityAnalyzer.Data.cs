// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    using ValueContentAnalysisResult = DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>;

    public partial class PlatformCompatabilityAnalyzer
    {
        private enum PlatformAttrbiteType
        {
            None, MinimumOSPlatformAttribute, ObsoletedInOSPlatformAttribute, RemovedInOSPlatformAttribute, TargetPlatformAttribute
        }

        private struct PlatformAttrbiuteInfo : IEquatable<PlatformAttrbiuteInfo>
        {
            public PlatformAttrbiteType AttributeType { get; set; }
            public string OsPlatformName { get; set; }
            public Version Version { get; set; }

            public static bool TryParseAttributeData(AttributeData osAttibute, out PlatformAttrbiuteInfo parsedAttribute)
            {
                parsedAttribute = new PlatformAttrbiuteInfo();
                switch (osAttibute.AttributeClass.Name)
                {
                    case MinimumOsAttributeName:
                        parsedAttribute.AttributeType = PlatformAttrbiteType.MinimumOSPlatformAttribute; break;
                    case ObsoleteAttributeName:
                        parsedAttribute.AttributeType = PlatformAttrbiteType.ObsoletedInOSPlatformAttribute; break;
                    case RemovedAttributeName:
                        parsedAttribute.AttributeType = PlatformAttrbiteType.RemovedInOSPlatformAttribute; break;
                    case TargetPlatformAttributeName:
                        parsedAttribute.AttributeType = PlatformAttrbiteType.TargetPlatformAttribute; break;
                    default:
                        parsedAttribute.AttributeType = PlatformAttrbiteType.None; break;
                }

                if (TryParsePlatformString(osAttibute.ConstructorArguments[0].Value.ToString(), out string platformName, out Version? version))
                {
                    parsedAttribute.OsPlatformName = platformName;
                    parsedAttribute.Version = version;
                    return true;
                }

                return false;
            }

            public override bool Equals(object obj)
            {
                if (obj is PlatformAttrbiuteInfo info)
                {
                    return Equals(info);
                }
                return false;
            }

            public override int GetHashCode() => HashUtilities.Combine(AttributeType.GetHashCode(), OsPlatformName.GetHashCode(), Version.GetHashCode());

            public static bool operator ==(PlatformAttrbiuteInfo left, PlatformAttrbiuteInfo right) => left.Equals(right);

            public static bool operator !=(PlatformAttrbiuteInfo left, PlatformAttrbiuteInfo right) => !(left == right);

            public bool Equals(PlatformAttrbiuteInfo other) =>
                AttributeType == other.AttributeType && OsPlatformName == other.OsPlatformName && Version.Equals(other.Version);

            internal static bool TryParseTfmString(string osString, out PlatformAttrbiuteInfo parsedTfm)
            {
                parsedTfm = new PlatformAttrbiuteInfo();
                parsedTfm.AttributeType = PlatformAttrbiteType.None;

                if (TryParsePlatformString(osString, out string platformName, out Version? version))
                {
                    parsedTfm.OsPlatformName = platformName;
                    parsedTfm.Version = version;
                }
                return parsedTfm.Version != null;
            }
        }

        private static bool TryParsePlatformString(string osString, out string osPlatformName, [NotNullWhen(true)] out Version? version)
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
                        break;
                    }
                }
            }

            return false;
        }

        private struct RuntimeMethodInfo : IAbstractAnalysisValue, IEquatable<RuntimeMethodInfo>
        {
            private RuntimeMethodInfo(string invokedPlatformCheckMethodName, string platformPropertyName, Version version, bool negated)
            {
                InvokedPlatformCheckMethodName = invokedPlatformCheckMethodName ?? throw new ArgumentNullException(nameof(invokedPlatformCheckMethodName));
                PlatformPropertyName = platformPropertyName ?? throw new ArgumentNullException(nameof(platformPropertyName));
                Version = version ?? throw new ArgumentNullException(nameof(version));
                Negated = negated;
            }

            public string InvokedPlatformCheckMethodName { get; }
            public string PlatformPropertyName { get; }
            public Version Version { get; }
            public bool Negated { get; }

            public IAbstractAnalysisValue GetNegatedValue()
                => new RuntimeMethodInfo(InvokedPlatformCheckMethodName, PlatformPropertyName, Version, !Negated);

            public static bool TryDecode(
                IMethodSymbol invokedPlatformCheckMethod,
                ImmutableArray<IArgumentOperation> arguments,
                ValueContentAnalysisResult? valueContentAnalysisResult,
                INamedTypeSymbol osPlatformType,
                [NotNullWhen(returnValue: true)] out RuntimeMethodInfo? info)
            {
                Debug.Assert(!arguments.IsEmpty);
                if (arguments[0].Value is ILiteralOperation literal && literal.Type.SpecialType == SpecialType.System_String)
                {
                    if (literal.ConstantValue.HasValue && TryParsePlatformString(literal.ConstantValue.Value.ToString(), out string platformName, out Version? version))
                    {
                        info = new RuntimeMethodInfo(invokedPlatformCheckMethod.Name, platformName, version, negated: false);
                        return true;
                    }
                }

                if (!TryDecodeOSPlatform(arguments, osPlatformType, out var osPlatformName) ||
                    !TryDecodeOSVersion(arguments, valueContentAnalysisResult, out var osVersion))
                {
                    // Bail out
                    info = default;
                    return false;
                }

                info = new RuntimeMethodInfo(invokedPlatformCheckMethod.Name, osPlatformName, osVersion, negated: false);
                return true;
            }

            private static bool TryDecodeOSPlatform(
                ImmutableArray<IArgumentOperation> arguments,
                INamedTypeSymbol osPlatformType,
                [NotNullWhen(returnValue: true)] out string? osPlatformName)
            {
                return TryDecodeOSPlatform(arguments[0].Value, osPlatformType, out osPlatformName);
            }

            private static bool TryDecodeOSPlatform(
                IOperation argumentValue,
                INamedTypeSymbol osPlatformType,
                [NotNullWhen(returnValue: true)] out string? osPlatformName)
            {
                if ((argumentValue is IPropertyReferenceOperation propertyReference) &&
                    propertyReference.Property.ContainingType.Equals(osPlatformType))
                {
                    osPlatformName = propertyReference.Property.Name;
                    return true;
                }

                osPlatformName = null;
                return false;
            }

            private static bool TryDecodeOSVersion(
                ImmutableArray<IArgumentOperation> arguments,
                ValueContentAnalysisResult? valueContentAnalysisResult,
                [NotNullWhen(returnValue: true)] out Version? osVersion)
            {
                using var versionBuilder = ArrayBuilder<int>.GetInstance(4, fillWithValue: 0);
                var index = 0;
                foreach (var argument in arguments.Skip(1))
                {
                    if (!TryDecodeOSVersionPart(argument, valueContentAnalysisResult, out var osVersionPart))
                    {
                        osVersion = null;
                        return false;
                    }

                    versionBuilder[index++] = osVersionPart;
                }

                osVersion = new Version(versionBuilder[0], versionBuilder[1], versionBuilder[2], versionBuilder[3]);
                return true;

                static bool TryDecodeOSVersionPart(IArgumentOperation argument, ValueContentAnalysisResult? valueContentAnalysisResult, out int osVersionPart)
                {
                    if (argument.Value.ConstantValue.HasValue &&
                        argument.Value.ConstantValue.Value is int versionPart)
                    {
                        osVersionPart = versionPart;
                        return true;
                    }

                    if (valueContentAnalysisResult != null)
                    {
                        var valueContentValue = valueContentAnalysisResult[argument.Value];
                        if (valueContentValue.IsLiteralState &&
                            valueContentValue.LiteralValues.Count == 1 &&
                            valueContentValue.LiteralValues.Single() is int part)
                        {
                            osVersionPart = part;
                            return true;
                        }
                    }

                    osVersionPart = default;
                    return false;
                }
            }

            public override string ToString()
            {
                var versionStr = Version.ToString();
                var result = $"{InvokedPlatformCheckMethodName};{PlatformPropertyName};{versionStr}";
                if (Negated)
                {
                    result = $"!{result}";
                }

                return result;
            }

            public bool Equals(RuntimeMethodInfo other)
                => InvokedPlatformCheckMethodName.Equals(other.InvokedPlatformCheckMethodName, StringComparison.OrdinalIgnoreCase) &&
                    PlatformPropertyName.Equals(other.PlatformPropertyName, StringComparison.OrdinalIgnoreCase) &&
                    Version.Equals(other.Version) &&
                    Negated == other.Negated;

            public override bool Equals(object obj)
                => obj is RuntimeMethodInfo otherInfo && Equals(otherInfo);

            public override int GetHashCode()
                => HashUtilities.Combine(InvokedPlatformCheckMethodName.GetHashCode(), PlatformPropertyName.GetHashCode(), Version.GetHashCode(), Negated.GetHashCode());

            bool IEquatable<IAbstractAnalysisValue>.Equals(IAbstractAnalysisValue other)
                => other is RuntimeMethodInfo otherInfo && Equals(otherInfo);

            public static bool operator ==(RuntimeMethodInfo left, RuntimeMethodInfo right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RuntimeMethodInfo left, RuntimeMethodInfo right)
            {
                return !(left == right);
            }
        }
    }
}
