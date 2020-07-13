// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
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
        private static readonly ImmutableArray<string> s_osPlatformAttributes = ImmutableArray.Create("MinimumOSPlatformAttribute", "ObsoletedInOSPlatformAttribute", "RemovedInOSPlatformAttribute");
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableAddedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckAddedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableObsoleteMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckObsoleteMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableRemovedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckRemovedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private const char SeparatorDash = '-';
        private const char SeparatorSemicolon = ';';
        private const string MinimumOsAttributeName = nameof(PlatformAttrbiteType.MinimumOSPlatformAttribute);
        private const string ObsoleteAttributeName = nameof(PlatformAttrbiteType.ObsoletedInOSPlatformAttribute);
        private const string RemovedAttributeName = nameof(PlatformAttrbiteType.RemovedInOSPlatformAttribute);
        private const string TargetPlatformAttributeName = nameof(PlatformAttrbiteType.TargetPlatformAttribute);
        private const string Windows = nameof(Windows);
        private static readonly Regex s_neutralTfmRegex = new Regex(@"^net([5-9]|standard\d|coreapp\d)\.\d$", RegexOptions.IgnoreCase);

        internal static DiagnosticDescriptor MinimumOsRule = DiagnosticDescriptorHelper.Create(RuleId,
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MinimumOsRule, ObsoleteRule, RemovedRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var typeName = WellKnownTypeNames.SystemRuntimeInteropServicesRuntimeInformation + "Helper";

                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName, out var runtimeInformationType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesOSPlatform, out var osPlatformType))
                {
                    return;
                }

                context.RegisterOperationBlockStartAction(context => AnalyzerOperationBlock(context, runtimeInformationType, osPlatformType));
            });
        }

        private void AnalyzerOperationBlock(OperationBlockStartAnalysisContext context, INamedTypeSymbol runtimeInformationType, INamedTypeSymbol osPlatformType)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope - disposed in OperationBlockEndAction.
            var parsedTfms = ParseTfm(context.Options, context.OwningSymbol, context.Compilation, context.CancellationToken);
#pragma warning restore CA2000 
            var platformSpecificOperations = PooledDictionary<IOperation, ImmutableArray<PlatformAttrbiuteInfo>>.GetInstance();

            context.RegisterOperationAction(context =>
            {
                AnalyzeInvocationOperation(context.Operation, context, parsedTfms, ref platformSpecificOperations);
            }
            , OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.FieldReference);

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
                        cfg, context.OwningSymbol, CreateOperationVisitor, wellKnownTypeProvider,
                        context.Options, MinimumOsRule, performValueContentAnalysis: true,
                        context.CancellationToken, out var valueContentAnalysisResult);
                    if (analysisResult == null)
                    {
                        return;
                    }

                    foreach (var platformSpecificOperation in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Key.Kind, platformSpecificOperation.Key.Syntax];

                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Unknown)
                        {
                            continue;
                        }

                        string operationName;

                        if (platformSpecificOperation.Key is IInvocationOperation iOperation)
                        {
                            operationName = iOperation.TargetMethod.Name;
                        }
                        else if (platformSpecificOperation.Key is IPropertyReferenceOperation pOperation)
                        {
                            operationName = pOperation.Property.Name;
                        }
                        else if (platformSpecificOperation.Key is IFieldReferenceOperation fOperation)
                        {
                            operationName = fOperation.Field.Name;

                        }
                        else
                        {
                            Debug.Fail("Should never happen");
                            return;
                        }

                        PlatformAttrbiuteInfo attribute = platformSpecificOperation.Value.FirstOrDefault();
                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Empty || value.Kind == GlobalFlowStateAnalysisValueSetKind.Unset)
                        {
                            context.ReportDiagnostic(platformSpecificOperation.Key.CreateDiagnostic(SwitchRule(attribute.AttributeType), operationName, attribute.OsPlatformName, attribute.Version.ToString()));
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
                                        if (attribute.OsPlatformName.Equals(info.PlatformPropertyName, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (info.InvokedPlatformCheckMethodName.Equals(s_platformCheckMethods[0], StringComparison.InvariantCulture))
                                            {
                                                if (attribute.AttributeType == PlatformAttrbiteType.MinimumOSPlatformAttribute && AttributeVersionsMatch(attribute.AttributeType, attribute.Version, info.Version))
                                                {
                                                    guarded = true;
                                                }
                                            }
                                            else
                                            {
                                                if ((attribute.AttributeType == PlatformAttrbiteType.ObsoletedInOSPlatformAttribute || attribute.AttributeType == PlatformAttrbiteType.RemovedInOSPlatformAttribute)
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
                                context.ReportDiagnostic(platformSpecificOperation.Key.CreateDiagnostic(SwitchRule(attribute.AttributeType), operationName, attribute.OsPlatformName, attribute.Version.ToString()));
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
                    => new OperationVisitor(GetPlatformCheckMethods(runtimeInformationType), osPlatformType, context);
            });

            static ImmutableArray<IMethodSymbol> GetPlatformCheckMethods(INamedTypeSymbol runtimeInformationType)
            {
                return runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m => s_platformCheckMethods.Contains(m.Name) && !m.Parameters.IsEmpty).ToImmutableArray<IMethodSymbol>();
            }
        }

        private static void AnalyzeInvocationOperation(IOperation operation, OperationAnalysisContext context,
            PooledConcurrentSet<PlatformAttrbiuteInfo>? parsedTfms, ref PooledDictionary<IOperation, ImmutableArray<PlatformAttrbiuteInfo>> platformSpecificOperations)
        {
            using var builder = ArrayBuilder<PlatformAttrbiuteInfo>.GetInstance();
            ImmutableArray<AttributeData> attributes;

            if (operation is IInvocationOperation iOperation)
            {
                attributes = FindPlatformAttributes(iOperation.TargetMethod.GetAttributes(), iOperation.TargetMethod.ContainingType);
            }
            else if (operation is IPropertyReferenceOperation pOperation)
            {
                attributes = FindPlatformAttributes(pOperation.Property.GetAttributes(), pOperation.Property.ContainingType);
            }
            else if (operation is IFieldReferenceOperation fOperation)
            {
                attributes = FindPlatformAttributes(fOperation.Field.GetAttributes(), fOperation.Field.ContainingType);
            }

            foreach (AttributeData attribute in attributes)
            {
                bool suppressed = false;
                if (PlatformAttrbiuteInfo.TryParseAttributeData(attribute, out PlatformAttrbiuteInfo parsedAttribute))
                {
                    if (parsedTfms != null)
                    {
                        foreach (PlatformAttrbiuteInfo tfm in parsedTfms)
                        {
                            if (tfm.OsPlatformName.Equals(parsedAttribute.OsPlatformName, StringComparison.InvariantCultureIgnoreCase))
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
            }

            if (builder.Count > 0)
            {
                platformSpecificOperations.Add(operation, builder.ToImmutable());
            }
        }

        private static DiagnosticDescriptor SwitchRule(PlatformAttrbiteType attributeType)
        {
            if (attributeType == PlatformAttrbiteType.MinimumOSPlatformAttribute)
                return MinimumOsRule;
            if (attributeType == PlatformAttrbiteType.ObsoletedInOSPlatformAttribute)
                return ObsoleteRule;
            return RemovedRule;
        }

        private static PooledConcurrentSet<PlatformAttrbiuteInfo>? ParseTfm(AnalyzerOptions options, ISymbol containingSymbol, Compilation compilation, CancellationToken cancellationToken)
        {
            string? tfmString = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetFramework, MinimumOsRule, containingSymbol, compilation, cancellationToken);
            if (tfmString != null)
            {
                PooledConcurrentSet<PlatformAttrbiuteInfo> platformInfos = PooledConcurrentSet<PlatformAttrbiuteInfo>.GetInstance();
                var tfms = tfmString.Split(SeparatorSemicolon);

                foreach (var tfm in tfms)
                {
                    var tokens = tfm.Split(SeparatorDash);
                    PlatformAttrbiuteInfo platformInfo;
                    if (tokens.Length == 1)
                    {
                        platformInfo = new PlatformAttrbiuteInfo();
                        platformInfo.Version = new Version();
                        platformInfo.OsPlatformName = s_neutralTfmRegex.IsMatch(tokens[0]) ? string.Empty : Windows;
                        platformInfos.Add(platformInfo);
                    }
                    else
                    {
                        Debug.Assert(tokens.Length == 2);
                        if (PlatformAttrbiuteInfo.TryParseTfmString(tokens[1], out PlatformAttrbiuteInfo parsedTfm))
                        {
                            var tpmv = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetPlatformMinVersion, MinimumOsRule, containingSymbol, compilation, cancellationToken);
                            if (tpmv != null)
                            {
                                if (Version.TryParse(tpmv, out Version version))
                                {
                                    parsedTfm.Version = version;
                                }
                            }
                            platformInfos.Add(parsedTfm);
                        }
                    }
                }
                return platformInfos;
            }
            return null;
        }

        private static ImmutableArray<AttributeData> FindPlatformAttributes(ImmutableArray<AttributeData> immediateAttributes, INamedTypeSymbol parent)
        {
            using var builder = ArrayBuilder<AttributeData>.GetInstance();
            foreach (AttributeData attribute in immediateAttributes)
            {
                if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name))
                {
                    builder.Add(attribute);
                }
            }
            while (parent != null)
            {
                var current = parent.GetAttributes();
                foreach (var attribute in current)
                {
                    if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name) && !TargetPlatformAttributeName.Equals(attribute.AttributeClass.Name, StringComparison.InvariantCulture))
                    {
                        builder.Add(attribute);
                    }
                }
                parent = parent.BaseType;
            }
            return builder.ToImmutableArray();
        }

        private static bool IsSuppressedByAttribute(PlatformAttrbiuteInfo diagnosingAttribute, ISymbol containingSymbol)
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
                            if (PlatformAttrbiuteInfo.TryParseAttributeData(attribute, out PlatformAttrbiuteInfo parsedAttribute))
                            {
                                if (diagnosingAttribute.OsPlatformName == parsedAttribute.OsPlatformName && AttributeVersionsMatch(diagnosingAttribute, parsedAttribute))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                containingSymbol = containingSymbol.ContainingSymbol;
            }
            return false;
        }

        private static bool AttributeVersionsMatch(PlatformAttrbiuteInfo diagnosingAttribute, PlatformAttrbiuteInfo osAttribute)
        {
            return AttributeVersionsMatch(diagnosingAttribute.AttributeType, diagnosingAttribute.Version, osAttribute.Version);
        }

        private static bool AttributeVersionsMatch(PlatformAttrbiteType attributeType, Version diagnosingVersion, Version suppressingVersion)
        {
            if (attributeType == PlatformAttrbiteType.MinimumOSPlatformAttribute)
            {
                if (diagnosingVersion.Major != suppressingVersion.Major)
                {
                    return diagnosingVersion.Major < suppressingVersion.Major;
                }
                if (diagnosingVersion.Minor != suppressingVersion.Minor)
                {
                    return diagnosingVersion.Minor < suppressingVersion.Minor;
                }
                if (diagnosingVersion.Build != suppressingVersion.Build)
                {
                    return diagnosingVersion.Build < suppressingVersion.Build;
                }

                return diagnosingVersion.Revision <= suppressingVersion.Revision;
            }
            else
            {
                Debug.Assert(attributeType == PlatformAttrbiteType.ObsoletedInOSPlatformAttribute || attributeType == PlatformAttrbiteType.RemovedInOSPlatformAttribute);

                if (diagnosingVersion.Major != suppressingVersion.Major)
                {
                    return diagnosingVersion.Major > suppressingVersion.Major;
                }
                if (diagnosingVersion.Minor != suppressingVersion.Minor)
                {
                    return diagnosingVersion.Minor > suppressingVersion.Minor;
                }
                if (diagnosingVersion.Build != suppressingVersion.Build)
                {
                    return diagnosingVersion.Build > suppressingVersion.Build;
                }

                return diagnosingVersion.Revision > suppressingVersion.Revision;
            }
        }
    }
}