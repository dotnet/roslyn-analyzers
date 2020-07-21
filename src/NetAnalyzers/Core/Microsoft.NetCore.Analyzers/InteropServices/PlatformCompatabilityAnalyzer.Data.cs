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

    public sealed partial class PlatformCompatabilityAnalyzer
    {
        private enum PlatformAttributeType
        {
            None, MinimumOSPlatformAttribute, ObsoletedInOSPlatformAttribute, RemovedInOSPlatformAttribute, TargetPlatformAttribute
        }

        private struct PlatformAttributeInfo : IEquatable<PlatformAttributeInfo>
        {
            public PlatformAttributeType AttributeType { get; set; }
            public string PlatformName { get; set; }
            public Version Version { get; set; }

            public static bool TryParsePlatformAttributeInfo(AttributeData osAttribute, out PlatformAttributeInfo parsedAttribute)
            {
                parsedAttribute = new PlatformAttributeInfo();
                switch (osAttribute.AttributeClass.Name)
                {
                    case MinimumOSPlatformAttribute:
                        parsedAttribute.AttributeType = PlatformAttributeType.MinimumOSPlatformAttribute;
                        break;
                    case ObsoletedInOSPlatformAttribute:
                        parsedAttribute.AttributeType = PlatformAttributeType.ObsoletedInOSPlatformAttribute;
                        break;
                    case RemovedInOSPlatformAttribute:
                        parsedAttribute.AttributeType = PlatformAttributeType.RemovedInOSPlatformAttribute;
                        break;
                    case TargetPlatformAttribute:
                        parsedAttribute.AttributeType = PlatformAttributeType.TargetPlatformAttribute;
                        break;
                    default:
                        parsedAttribute.AttributeType = PlatformAttributeType.None;
                        break;
                }

                if (osAttribute.ConstructorArguments.Length == 1 && osAttribute.ConstructorArguments[0].Type.SpecialType == SpecialType.System_String &&
                    !osAttribute.ConstructorArguments[0].IsNull && TryParsePlatformNameAndVersion(osAttribute.ConstructorArguments[0].Value.ToString(), out string platformName, out Version? version))
                {
                    parsedAttribute.PlatformName = platformName;
                    parsedAttribute.Version = version;
                    return true;
                }

                return false;
            }

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
                AttributeType == other.AttributeType && IsOSPlatformsEqual(PlatformName, other.PlatformName) && Version.Equals(other.Version);
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
                        break;
                    }
                }
            }

            return false;
        }

        private struct RuntimeMethodValue : IAbstractAnalysisValue, IEquatable<RuntimeMethodValue>
        {
            private RuntimeMethodValue(string invokedPlatformCheckMethodName, string platformPropertyName, Version version, bool negated)
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
                => new RuntimeMethodValue(InvokedPlatformCheckMethodName, PlatformPropertyName, Version, !Negated);

            public static bool TryDecode(
                IMethodSymbol invokedPlatformCheckMethod,
                ImmutableArray<IArgumentOperation> arguments,
                ValueContentAnalysisResult? valueContentAnalysisResult,
                INamedTypeSymbol osPlatformType,
                [NotNullWhen(returnValue: true)] out RuntimeMethodValue? info)
            {
                Debug.Assert(!arguments.IsEmpty);

                if (arguments[0].Value is ILiteralOperation literal && literal.Type.SpecialType == SpecialType.System_String)
                {
                    if (literal.ConstantValue.HasValue && TryParsePlatformNameAndVersion(literal.ConstantValue.Value.ToString(), out string platformName, out Version? version))
                    {
                        info = new RuntimeMethodValue(invokedPlatformCheckMethod.Name, platformName, version, negated: false);
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

                info = new RuntimeMethodValue(invokedPlatformCheckMethod.Name, osPlatformName, osVersion, negated: false);
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
                var result = $"{InvokedPlatformCheckMethodName};{PlatformPropertyName};{Version}";
                if (Negated)
                {
                    result = $"!{result}";
                }

                return result;
            }

            public bool Equals(RuntimeMethodValue other)
                => InvokedPlatformCheckMethodName.Equals(other.InvokedPlatformCheckMethodName, StringComparison.OrdinalIgnoreCase) &&
                    PlatformPropertyName.Equals(other.PlatformPropertyName, StringComparison.OrdinalIgnoreCase) &&
                    Version.Equals(other.Version) &&
                    Negated == other.Negated;

            public override bool Equals(object obj)
                => obj is RuntimeMethodValue otherInfo && Equals(otherInfo);

            public override int GetHashCode()
                => HashUtilities.Combine(InvokedPlatformCheckMethodName.GetHashCode(), PlatformPropertyName.GetHashCode(), Version.GetHashCode(), Negated.GetHashCode());

            bool IEquatable<IAbstractAnalysisValue>.Equals(IAbstractAnalysisValue other)
                => other is RuntimeMethodValue otherInfo && Equals(otherInfo);

            public static bool operator ==(RuntimeMethodValue left, RuntimeMethodValue right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RuntimeMethodValue left, RuntimeMethodValue right)
            {
                return !(left == right);
            }
        }
    }
}
