// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed partial class PlatformCompatabilityAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1417";
        private static readonly ImmutableArray<string> s_platformCheckMethods = ImmutableArray.Create("IsOSPlatformOrLater", "IsOSPlatformEarlierThan");
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableAddedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckAddedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableObsoleteMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckObsoleteMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableRemovedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckRemovedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private const char SeparatorDash = '-';
        private const char SeparatorSemicolon = ';';
        private const char SeparatorDot = '.';
        private const string MinimumOsAttributeName = nameof(MinimumOSPlatformAttribute);
        private const string ObsoleteAttributeName = nameof(ObsoletedInOSPlatformAttribute);
        private const string RemovedAttributeName = nameof(RemovedInOSPlatformAttribute);
        private const string Windows = nameof(Windows);
        private static readonly Regex s_neutralTfmRegex = new Regex(@"^net([5-9]|standard\d|coreapp\d)\.\d$", RegexOptions.IgnoreCase);
        private static readonly Regex s_osParseRegex = new Regex(@"([a-z]{3,7})((\d{1,2})\.?(\d)?\.?(\d)?\.?(\d)?)*", RegexOptions.IgnoreCase);

        internal static DiagnosticDescriptor AddedRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableAddedMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor ObsoleteRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableAddedMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableObsoleteMessage,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);
        internal static DiagnosticDescriptor RemovedRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableAddedMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableRemovedMessage,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddedRule, ObsoleteRule, RemovedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var typeName = WellKnownTypeNames.SystemRuntimeInteropServicesRuntimeInformation + "Helper";

                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName, out var runtimeInformationType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesOSPlatform, out var osPlatformType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeVersioningOSPlatformAttribute, out var osAttribute))
                {
                    return;
                }

                context.RegisterOperationBlockStartAction(context => AnalyzerOperationBlock(context, osAttribute, runtimeInformationType, osPlatformType));
            });
        }

        private void AnalyzerOperationBlock(OperationBlockStartAnalysisContext context, INamedTypeSymbol osAttribute, INamedTypeSymbol runtimeInformationType, INamedTypeSymbol osPlatformType)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope - disposed in OperationBlockEndAction.
            var parsedTfms = ParseTfm(context.Options, context.OwningSymbol, context.Compilation, context.CancellationToken);
#pragma warning restore CA2000 
            var platformSpecificOperations = PooledDictionary<IInvocationOperation, ImmutableArray<OsAttributeInfo>>.GetInstance();
            var needsValueContentAnalysis = false;

            context.RegisterOperationAction(context =>
            {
                AnalyzeInvocationOperation((IInvocationOperation)context.Operation, osAttribute, context, parsedTfms, ref platformSpecificOperations);
            }
            , OperationKind.Invocation);

            context.RegisterOperationBlockEndAction(context =>
            {
                try
                {
                    if (platformSpecificOperations.Count == 0 || !(context.OperationBlocks.GetControlFlowGraph() is { } cfg))
                    {
                        return;
                    }

                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                    var analysisResult = GlobalFlowStateAnalysis.TryGetOrComputeResult(
                        cfg, context.OwningSymbol, CreateOperationVisitor,
                        wellKnownTypeProvider, context.Options, AddedRule, performPointsToAnalysis: needsValueContentAnalysis,
                        performValueContentAnalysis: needsValueContentAnalysis, context.CancellationToken,
                        out var pointsToAnalysisResult, out var valueContentAnalysisResult);
                    if (analysisResult == null)
                    {
                        return;
                    }

                    Debug.Assert(valueContentAnalysisResult == null || needsValueContentAnalysis);
                    Debug.Assert(pointsToAnalysisResult == null || needsValueContentAnalysis);

                    foreach (var platformSpecificOperation in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Key.Kind, platformSpecificOperation.Key.Syntax];
                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Unknown)
                        {
                            continue;
                        }

                        OsAttributeInfo parsedAttribute = platformSpecificOperation.Value.FirstOrDefault();
                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Empty || value.Kind == GlobalFlowStateAnalysisValueSetKind.Unset)
                        {
                            context.ReportDiagnostic(platformSpecificOperation.Key.CreateDiagnostic(SwitchRule(parsedAttribute.AttributeType), platformSpecificOperation.Key.TargetMethod.Name,
                                       parsedAttribute.OsPlatform!, $"{parsedAttribute.Version[0]}.{parsedAttribute.Version[1]}.{parsedAttribute.Version[2]}.{parsedAttribute.Version[3]}"));
                        }
                        else
                        {
                            bool guarded = false;
                            foreach (var analysisValue in value.AnalysisValues)
                            {
                                if (analysisValue is RuntimeMethodInfo info)
                                {
                                    if (!info.Negated)
                                    {
                                        OsAttributeInfo attribute = platformSpecificOperation.Value.First();
                                        if (attribute.OsPlatform!.Equals(info.PlatformPropertyName, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (info.InvokedPlatformCheckMethodName.Equals(s_platformCheckMethods[0], StringComparison.InvariantCulture))
                                            {
                                                if (attribute.AttributeType == OsAttrbiteType.MinimumOSPlatformAttribute && AttributeVersionsMatch(attribute.AttributeType, attribute.Version, info.Version))
                                                {
                                                    guarded = true;
                                                }
                                            }
                                            else
                                            {
                                                if ((attribute.AttributeType == OsAttrbiteType.ObsoletedInOSPlatformAttribute || attribute.AttributeType == OsAttrbiteType.RemovedInOSPlatformAttribute)
                                                        && AttributeVersionsMatch(attribute.AttributeType, attribute.Version, info.Version))
                                                {
                                                    guarded = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!guarded)
                            {
                                context.ReportDiagnostic(platformSpecificOperation.Key.CreateDiagnostic(SwitchRule(parsedAttribute.AttributeType), platformSpecificOperation.Key.TargetMethod.Name,
                                           parsedAttribute.OsPlatform!, $"{parsedAttribute.Version[0]}.{parsedAttribute.Version[1]}.{parsedAttribute.Version[2]}.{parsedAttribute.Version[3]}"));
                            }
                        }
                    }
                }
                finally
                {
                    platformSpecificOperations.Free();
                    parsedTfms?.Free();
                }

                return;

                OperationVisitor CreateOperationVisitor(GlobalFlowStateAnalysisContext context)
                    => new OperationVisitor(GetPlatformCheckMethods(runtimeInformationType, osPlatformType), osPlatformType, context);
            });

            static ImmutableArray<IMethodSymbol> GetPlatformCheckMethods(INamedTypeSymbol runtimeInformationType, INamedTypeSymbol osPlatformType)
            {
                using var builder = ArrayBuilder<IMethodSymbol>.GetInstance();
                var methods = runtimeInformationType.GetMembers().OfType<IMethodSymbol>();
                foreach (var method in methods)
                {
                    if (s_platformCheckMethods.Contains(method.Name) &&
                        method.Parameters.Length >= 1 &&
                        method.Parameters[0].Type.Equals(osPlatformType) &&
                        method.Parameters.Skip(1).All(p => p.Type.SpecialType == SpecialType.System_Int32))
                    {
                        builder.Add(method);
                    }
                }

                return builder.ToImmutable();
            }
        }

        private static void AnalyzeInvocationOperation(IInvocationOperation operation, INamedTypeSymbol osAttribute, OperationAnalysisContext context,
            PooledConcurrentSet<OsAttributeInfo>? parsedTfms, ref PooledDictionary<IInvocationOperation, ImmutableArray<OsAttributeInfo>> platformSpecificOperations)
        {
            var attributes = GetApplicableAttributes(operation.TargetMethod.GetAttributes(), operation.TargetMethod.ContainingType, osAttribute);
            using var builder = ArrayBuilder<OsAttributeInfo>.GetInstance();
            foreach (AttributeData attribute in attributes)
            {
                bool suppressed = false;
                OsAttributeInfo parsedAttribute = OsAttributeInfo.ParseAttributeData(attribute);
                if (parsedTfms != null)
                {
                    foreach (OsAttributeInfo tfm in parsedTfms)
                    {
                        if (tfm.OsPlatform != null && tfm.OsPlatform.Equals(parsedAttribute.OsPlatform, StringComparison.InvariantCultureIgnoreCase))
                        {
                            suppressed = AttributeVersionsMatch(parsedAttribute, tfm);
                        }
                    }
                }

                suppressed = suppressed || IsSuppressedByAttribute(parsedAttribute, context.ContainingSymbol);

                if (!suppressed)
                {
                    builder.Add(parsedAttribute);
                }
            }

            if (builder.Count > 0)
            {
                platformSpecificOperations.Add(operation, builder.ToImmutable());
            }
        }

        private static DiagnosticDescriptor SwitchRule(OsAttrbiteType attributeType)
        {
            if (attributeType == OsAttrbiteType.MinimumOSPlatformAttribute)
                return AddedRule;
            if (attributeType == OsAttrbiteType.ObsoletedInOSPlatformAttribute)
                return ObsoleteRule;
            return RemovedRule;
        }

        private static PooledConcurrentSet<OsAttributeInfo>? ParseTfm(AnalyzerOptions options, ISymbol containingSymbol, Compilation compilation, CancellationToken cancellationToken)
        { // ((net[5-9]|netstandard\d|netcoreapp\d)\.\d(-([a-z]{3,7})(\d{1,2}\.?\d?\.?\d?\.?\d?)*)?)+
            string? tfmString = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetFramework, AddedRule, containingSymbol, compilation, cancellationToken);
            if (tfmString != null)
            {
                PooledConcurrentSet<OsAttributeInfo> platformInfos = PooledConcurrentSet<OsAttributeInfo>.GetInstance();
                var tfms = tfmString.Split(SeparatorSemicolon);

                foreach (var tfm in tfms)
                {
                    var tokens = tfm.Split(SeparatorDash);
                    OsAttributeInfo platformInfo = new OsAttributeInfo();
                    platformInfo.Version = new int[4];
                    if (tokens.Length == 1)
                    {
                        if (!s_neutralTfmRegex.IsMatch(tokens[0]))
                        {
                            platformInfo.OsPlatform = Windows;
                        }
                    }
                    else
                    {
                        Debug.Assert(tokens.Length == 2);
                        Match match = s_osParseRegex.Match(tokens[1]);
                        if (match.Success)
                        {
                            platformInfo.OsPlatform = match.Groups[1].Value;
                            for (int i = 3; i < 7; i++)
                            {
                                if (!string.IsNullOrEmpty(match.Groups[i].Value))
                                {
                                    platformInfo.Version[i - 3] = int.Parse(match.Groups[i].Value, CultureInfo.InvariantCulture);
                                }
                            }
                        }
                        var tpmv = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetPlatformMinVersion, AddedRule, containingSymbol, compilation, cancellationToken);
                        if (tpmv != null)
                        {
                            var splitted = tpmv.Split(SeparatorDot);
                            int i = 0;
                            foreach (var token in splitted)
                            {
                                platformInfo.Version[i] = int.Parse(token, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    platformInfos.Add(platformInfo);
                }
                return platformInfos;
            }
            return null;
        }

        private static List<AttributeData> GetApplicableAttributes(ImmutableArray<AttributeData> immediateAttributes, INamedTypeSymbol type, INamedTypeSymbol osAttribute)
        {
            var attributes = new List<AttributeData>();
            foreach (AttributeData attribute in immediateAttributes)
            {
                if (attribute.AttributeClass.DerivesFromOrImplementsAnyConstructionOf(osAttribute))
                {
                    attributes.Add(attribute);
                }
            }
            while (type != null)
            {
                var current = type.GetAttributes();
                foreach (var attribute in current)
                {
                    if (attribute.AttributeClass.DerivesFromOrImplementsAnyConstructionOf(osAttribute))
                    {
                        attributes.Add(attribute);
                    }
                }
                type = type.BaseType;
            }
            return attributes;
        }

        private static bool IsSuppressedByAttribute(OsAttributeInfo diagnosingAttribute, ISymbol containingSymbol)
        {
            while (containingSymbol != null)
            {
                var attributes = containingSymbol.GetAttributes();
                if (attributes != null)
                {
                    foreach (AttributeData attribute in attributes)
                    {
                        if (diagnosingAttribute.AttributeType.ToString().Equals(attribute.AttributeClass.Name, StringComparison.InvariantCulture))
                        {
                            OsAttributeInfo parsedAttribute = OsAttributeInfo.ParseAttributeData(attribute);
                            if (diagnosingAttribute.OsPlatform!.Equals(parsedAttribute.OsPlatform, StringComparison.InvariantCultureIgnoreCase) && AttributeVersionsMatch(diagnosingAttribute, parsedAttribute))
                            {
                                return true;
                            }
                        }
                    }
                }
                containingSymbol = containingSymbol.ContainingSymbol;
            }
            return false;
        }

        private static bool AttributeVersionsMatch(OsAttributeInfo diagnosingAttribute, OsAttributeInfo tfm)
        {
            if (diagnosingAttribute.AttributeType == OsAttrbiteType.MinimumOSPlatformAttribute)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (diagnosingAttribute.Version[i] < tfm.Version[i])
                    {
                        return true;
                    }
                    else if (diagnosingAttribute.Version[i] > tfm.Version[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (diagnosingAttribute.Version[i] > tfm.Version[i])
                    {
                        return true;
                    }
                    else if (diagnosingAttribute.Version[i] < tfm.Version[i])
                    {
                        return false;
                    }
                }
                return true;
            };
        }

        private static bool AttributeVersionsMatch(OsAttrbiteType attributeType, int[] diagnosingVersion, int[] suppressingVersion)
        {
            if (attributeType == OsAttrbiteType.MinimumOSPlatformAttribute)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (diagnosingVersion[i] < suppressingVersion[i])
                    {
                        return true;
                    }
                    else if (diagnosingVersion[i] > suppressingVersion[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                Debug.Assert(attributeType == OsAttrbiteType.ObsoletedInOSPlatformAttribute || attributeType == OsAttrbiteType.RemovedInOSPlatformAttribute);

                for (int i = 0; i < 4; i++)
                {
                    if (diagnosingVersion[i] > suppressingVersion[i])
                    {
                        return true;
                    }
                    else if (diagnosingVersion[i] < suppressingVersion[i])
                    {
                        return false;
                    }
                }
                return true;
            };
        }

        private enum OsAttrbiteType
        {
            None, MinimumOSPlatformAttribute, ObsoletedInOSPlatformAttribute, RemovedInOSPlatformAttribute
        }

        private struct OsAttributeInfo : IEquatable<OsAttributeInfo>
        {
            public OsAttrbiteType AttributeType { get; set; }
            public string? OsPlatform { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
            public int[] Version { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

            public static OsAttributeInfo ParseAttributeData(AttributeData osAttibute)
            {
                OsAttributeInfo platformInfo = new OsAttributeInfo();
                switch (osAttibute.AttributeClass.Name)
                {
                    case MinimumOsAttributeName:
                        platformInfo.AttributeType = OsAttrbiteType.MinimumOSPlatformAttribute; break;
                    case ObsoleteAttributeName:
                        platformInfo.AttributeType = OsAttrbiteType.ObsoletedInOSPlatformAttribute; break;
                    case RemovedAttributeName:
                        platformInfo.AttributeType = OsAttrbiteType.RemovedInOSPlatformAttribute; break;
                    default:
                        platformInfo.AttributeType = OsAttrbiteType.None; break;
                }

                platformInfo.Version = new int[4];
                Match match = s_osParseRegex.Match(osAttibute.ConstructorArguments[0].Value.ToString());
                if (match.Success)
                {
                    platformInfo.OsPlatform = match.Groups[1].Value;
                    for (int i = 3; i < 7; i++)
                    {
                        if (!string.IsNullOrEmpty(match.Groups[i].Value))
                        {
                            platformInfo.Version[i - 3] = int.Parse(match.Groups[i].Value, CultureInfo.InvariantCulture);
                        }
                    }
                }

                return platformInfo;
            }

            public override bool Equals(object obj)
            {
                if (obj is OsAttributeInfo info)
                {
                    return Equals(info);
                }
                return false;
            }

            public override int GetHashCode() =>
                HashUtilities.Combine(AttributeType.GetHashCode(), OsPlatform?.GetHashCode(), Version.GetHashCode());

            public static bool operator ==(OsAttributeInfo left, OsAttributeInfo right) => left.Equals(right);

            public static bool operator !=(OsAttributeInfo left, OsAttributeInfo right) => !(left == right);

            public bool Equals(OsAttributeInfo other)
            {
                if (AttributeType == other.AttributeType && OsPlatform == other.OsPlatform)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (Version[i] != other.Version[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }
    }
}