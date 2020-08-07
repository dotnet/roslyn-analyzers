// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities.PooledObjects;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Analyzer.Utilities;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    using ValueContentAnalysisResult = DataFlowAnalysisResult<ValueContentBlockAnalysisResult, ValueContentAbstractValue>;

    public sealed partial class PlatformCompatabilityAnalyzer
    {
        private readonly struct RuntimeMethodValue : IAbstractAnalysisValue, IEquatable<RuntimeMethodValue>
        {
            private RuntimeMethodValue(string invokedPlatformCheckMethodName, string platformPropertyName, Version version, bool negated)
            {
                InvokedMethodName = invokedPlatformCheckMethodName ?? throw new ArgumentNullException(nameof(invokedPlatformCheckMethodName));
                PlatformName = platformPropertyName ?? throw new ArgumentNullException(nameof(platformPropertyName));
                Version = version ?? throw new ArgumentNullException(nameof(version));
                Negated = negated;
            }

            public string InvokedMethodName { get; }
            public string PlatformName { get; }
            public Version Version { get; }
            public bool Negated { get; }

            public IAbstractAnalysisValue GetNegatedValue()
                => new RuntimeMethodValue(InvokedMethodName, PlatformName, Version, !Negated);

            public static bool TryDecode(
                IMethodSymbol invokedPlatformCheckMethod,
                ImmutableArray<IArgumentOperation> arguments,
                ValueContentAnalysisResult? valueContentAnalysisResult,
                INamedTypeSymbol osPlatformType,
                [NotNullWhen(returnValue: true)] out RuntimeMethodValue? info)
            {
                Debug.Assert(!arguments.IsEmpty);

                if (arguments[0].Value is ILiteralOperation literal && literal.Type?.SpecialType == SpecialType.System_String)
                {
                    if (literal.ConstantValue.HasValue &&
                        TryParsePlatformNameAndVersion(literal.ConstantValue.Value.ToString(), out string platformName, out Version? version))
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
                var result = $"{InvokedMethodName};{PlatformName};{Version}";
                if (Negated)
                {
                    result = $"!{result}";
                }

                return result;
            }

            public bool Equals(RuntimeMethodValue other)
                => InvokedMethodName.Equals(other.InvokedMethodName, StringComparison.OrdinalIgnoreCase) &&
                    PlatformName.Equals(other.PlatformName, StringComparison.OrdinalIgnoreCase) &&
                    Version.Equals(other.Version) &&
                    Negated == other.Negated;

            public override bool Equals(object obj)
                => obj is RuntimeMethodValue otherInfo && Equals(otherInfo);

            public override int GetHashCode()
                => HashUtilities.Combine(InvokedMethodName.GetHashCode(), PlatformName.GetHashCode(), Version.GetHashCode(), Negated.GetHashCode());

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