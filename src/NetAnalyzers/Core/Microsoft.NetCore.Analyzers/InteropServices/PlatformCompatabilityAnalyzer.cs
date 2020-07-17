// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        private static readonly ImmutableArray<string> s_osPlatformAttributes = ImmutableArray.Create(MinimumOSPlatformAttribute, ObsoletedInOSPlatformAttribute, RemovedInOSPlatformAttribute);
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableAddedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckAddedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableObsoleteMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckObsoleteMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableRemovedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckRemovedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private const char SeparatorDash = '-';
        private const char SeparatorSemicolon = ';';
        private const string MinimumOSPlatformAttribute = nameof(MinimumOSPlatformAttribute);
        private const string ObsoletedInOSPlatformAttribute = nameof(ObsoletedInOSPlatformAttribute);
        private const string RemovedInOSPlatformAttribute = nameof(RemovedInOSPlatformAttribute);
        private const string TargetPlatformAttribute = nameof(TargetPlatformAttribute);
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

                var guardMethods = GetRuntimePlatformGuardMethods(runtimeInformationType);

                context.RegisterOperationBlockStartAction(context => AnalyzerOperationBlock(context, guardMethods, osPlatformType));
            });

            static ImmutableArray<IMethodSymbol> GetRuntimePlatformGuardMethods(INamedTypeSymbol runtimeInformationType)
            {
                return runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    s_platformCheckMethods.Contains(m.Name) && !m.Parameters.IsEmpty).ToImmutableArray();
            }
        }

        private void AnalyzerOperationBlock(OperationBlockStartAnalysisContext context, ImmutableArray<IMethodSymbol> guardMethods, INamedTypeSymbol osPlatformType)
        {
            var parsedTfms = ParseTfm(context.Options, context.OwningSymbol, context.Compilation, context.CancellationToken);
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
                    if (platformSpecificOperations.Count == 0)
                    {
                        return;
                    }

                    if (guardMethods.IsEmpty || !(context.OperationBlocks.GetControlFlowGraph() is { } cfg))
                    {
                        ReportDiagnosticsForAll(platformSpecificOperations, context);
                        return;
                    }

                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                    var analysisResult = GlobalFlowStateAnalysis.TryGetOrComputeResult(
                        cfg, context.OwningSymbol, CreateOperationVisitor, wellKnownTypeProvider,
                        context.Options, MinimumOsRule, performValueContentAnalysis: true,
                        context.CancellationToken, out var valueContentAnalysisResult);

                    if (analysisResult == null)
                    {
                        ReportDiagnosticsForAll(platformSpecificOperations, context);
                        return;
                    }

                    foreach (var platformSpecificOperation in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Key.Kind, platformSpecificOperation.Key.Syntax];

                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Unknown)
                        {
                            continue;
                        }

                        ReportDiangosticsIfNotGuarded(platformSpecificOperation.Key, platformSpecificOperation.Value, value, context);
                    }
                }
                finally
                {
                    platformSpecificOperations.Free();
                }

                return;

                OperationVisitor CreateOperationVisitor(GlobalFlowStateAnalysisContext context) => new OperationVisitor(guardMethods, osPlatformType, context);
            });
        }

        private static void ReportDiangosticsIfNotGuarded(IOperation operation, ImmutableArray<PlatformAttrbiuteInfo> attributes, GlobalFlowStateAnalysisValueSet value, OperationBlockAnalysisContext context)
        {
            PlatformAttrbiuteInfo attribute = attributes.FirstOrDefault();
            if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Empty || value.Kind == GlobalFlowStateAnalysisValueSetKind.Unset)
            {
                ReportDiagnostics(operation, attribute, context);
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
                    ReportDiagnostics(operation, attribute, context);
                }
            }
        }

        private static void ReportDiagnosticsForAll(PooledDictionary<IOperation, ImmutableArray<PlatformAttrbiuteInfo>> platformSpecificOperations, OperationBlockAnalysisContext context)
        {
            foreach (var platformSpecificOperation in platformSpecificOperations)
            {
                ReportDiagnostics(platformSpecificOperation.Key, platformSpecificOperation.Value.FirstOrDefault(), context);
            }
        }

        private static void ReportDiagnostics(IOperation operation, PlatformAttrbiuteInfo attribute, OperationBlockAnalysisContext context)
        {
            string operationName = string.Empty;

            if (operation is IInvocationOperation iOperation)
            {
                operationName = iOperation.TargetMethod.Name;
            }
            else if (operation is IPropertyReferenceOperation pOperation)
            {
                operationName = pOperation.Property.Name;
            }
            else if (operation is IFieldReferenceOperation fOperation)
            {
                operationName = fOperation.Field.Name;
            }

            context.ReportDiagnostic(operation.CreateDiagnostic(SwitchRule(attribute.AttributeType), operationName, attribute.OsPlatformName, attribute.Version.ToString()));
        }

        private static void AnalyzeInvocationOperation(IOperation operation, OperationAnalysisContext context,
            List<PlatformAttrbiuteInfo>? parsedTfms, ref PooledDictionary<IOperation, ImmutableArray<PlatformAttrbiuteInfo>> platformSpecificOperations)
        {
            using var builder = ArrayBuilder<PlatformAttrbiuteInfo>.GetInstance();
            AttributeData[]? attributes = null;

            if (operation is IInvocationOperation iOperation)
            {
                attributes = FindAllPlatformAttributesApplied(iOperation.TargetMethod.GetAttributes(), iOperation.TargetMethod.ContainingType);
            }
            else if (operation is IPropertyReferenceOperation pOperation)
            {
                if (!(pOperation.Parent is IBinaryOperation bo && (bo.OperatorKind == BinaryOperatorKind.Equals || bo.OperatorKind == BinaryOperatorKind.NotEquals)))
                    attributes = FindAllPlatformAttributesApplied(pOperation.Property.GetAttributes(), pOperation.Property.ContainingType);
            }
            else if (operation is IFieldReferenceOperation fOperation)
            {
                if (!(fOperation.Parent is IBinaryOperation bo && (bo.OperatorKind == BinaryOperatorKind.Equals || bo.OperatorKind == BinaryOperatorKind.NotEquals)))
                    attributes = FindAllPlatformAttributesApplied(fOperation.Field.GetAttributes(), fOperation.Field.ContainingType);
            }

            if (attributes != null)
            {
                foreach (AttributeData attribute in attributes)
                {
                    if (PlatformAttrbiuteInfo.TryParseAttributeData(attribute, out PlatformAttrbiuteInfo parsedAttribute))
                    {
                        if (!(IsSuppressedByTfm(parsedTfms, parsedAttribute) || IsSuppressedByAttribute(parsedAttribute, context.ContainingSymbol)))
                        {
                            builder.Add(parsedAttribute);
                        }
                    }
                }
            }

            if (builder.Count > 0)
            {
                platformSpecificOperations.Add(operation, builder.ToImmutable());
            }
        }

        private static bool IsSuppressedByTfm(List<PlatformAttrbiuteInfo>? parsedTfms, PlatformAttrbiuteInfo parsedAttribute)
        {
            if (parsedTfms != null)
            {
                foreach (PlatformAttrbiuteInfo tfm in parsedTfms)
                {
                    if (tfm.OsPlatformName.Equals(parsedAttribute.OsPlatformName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return AttributeVersionsMatch(parsedAttribute, tfm);
                    }
                }
            }

            return false;
        }

        private static DiagnosticDescriptor SwitchRule(PlatformAttrbiteType attributeType)
        {
            if (attributeType == PlatformAttrbiteType.MinimumOSPlatformAttribute)
                return MinimumOsRule;
            if (attributeType == PlatformAttrbiteType.ObsoletedInOSPlatformAttribute)
                return ObsoleteRule;
            return RemovedRule;
        }

        private static List<PlatformAttrbiuteInfo>? ParseTfm(AnalyzerOptions options, ISymbol containingSymbol, Compilation compilation, CancellationToken cancellationToken)
        {
            string? tfmString = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetFramework, MinimumOsRule, containingSymbol, compilation, cancellationToken);
            if (tfmString != null)
            {
                using var builder = ArrayBuilder<PlatformAttrbiuteInfo>.GetInstance();

                var tfms = tfmString.Split(SeparatorSemicolon);

                foreach (var tfm in tfms)
                {
                    var tokens = tfm.Split(SeparatorDash);

                    if (tokens.Length == 1)
                    {
                        if (!s_neutralTfmRegex.IsMatch(tokens[0]))
                        {
                            builder.Add(CreateWindowsOnlyPlatformInfo());
                        }
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
                            builder.Add(parsedTfm);
                        }
                    }
                }
                return builder.ToList();
            }
            return null;
        }

        private static PlatformAttrbiuteInfo CreateWindowsOnlyPlatformInfo()
        {
            var platformInfo = new PlatformAttrbiuteInfo();
            platformInfo.Version = new Version(7, 0);
            platformInfo.OsPlatformName = Windows;
            return platformInfo;
        }

        private static AttributeData[] FindAllPlatformAttributesApplied(ImmutableArray<AttributeData> immediateAttributes, INamedTypeSymbol parent)
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
                    if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name) && !TargetPlatformAttribute.Equals(attribute.AttributeClass.Name, StringComparison.InvariantCulture))
                    {
                        builder.Add(attribute);
                    }
                }
                parent = parent.BaseType;
            }
            return builder.ToArray();
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