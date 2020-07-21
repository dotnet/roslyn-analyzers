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
        private static readonly ImmutableArray<string> s_platformCheckMethodNames = ImmutableArray.Create("IsOSPlatformOrLater", "IsOSPlatformEarlierThan");
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
                var typeName = WellKnownTypeNames.SystemRuntimeInteropServicesRuntimeInformation;

                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName + "Helper", out var runtimeInformationType))
                {
                    runtimeInformationType = context.Compilation.GetOrCreateTypeByMetadataName(typeName);
                }
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesOSPlatform, out var osPlatformType))
                {
                    return;
                }

                var guardMethods = GetRuntimePlatformGuardMethods(runtimeInformationType!);

                context.RegisterOperationBlockStartAction(context => AnalyzerOperationBlock(context, guardMethods, osPlatformType));
            });

            static ImmutableArray<IMethodSymbol> GetRuntimePlatformGuardMethods(INamedTypeSymbol runtimeInformationType)
            {
                return runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    s_platformCheckMethodNames.Contains(m.Name) && !m.Parameters.IsEmpty).ToImmutableArray();
            }
        }

        private void AnalyzerOperationBlock(OperationBlockStartAnalysisContext context, ImmutableArray<IMethodSymbol> guardMethods, INamedTypeSymbol osPlatformType)
        {
            var parsedTfms = ParseTfm(context.Options, context.OwningSymbol, context.Compilation, context.CancellationToken);
            var platformSpecificOperations = PooledDictionary<IOperation, ImmutableArray<PlatformAttributeInfo>>.GetInstance();

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

        private static void ReportDiangosticsIfNotGuarded(IOperation operation, ImmutableArray<PlatformAttributeInfo> attributes, GlobalFlowStateAnalysisValueSet value, OperationBlockAnalysisContext context)
        {
            PlatformAttributeInfo attribute = attributes.FirstOrDefault();
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
                            if (IsOSPlatformsEqual(attribute.OsPlatformName, info.PlatformPropertyName))
                            {
                                if (info.InvokedPlatformCheckMethodName.Equals(s_platformCheckMethodNames[0], StringComparison.InvariantCulture))
                                {
                                    if (attribute.AttributeType == PlatformAttributeType.MinimumOSPlatformAttribute && AttributeVersionsMatch(attribute.AttributeType, attribute.Version, info.Version))
                                    {
                                        guarded = true;
                                    }
                                }
                                else
                                {
                                    if ((attribute.AttributeType == PlatformAttributeType.ObsoletedInOSPlatformAttribute || attribute.AttributeType == PlatformAttributeType.RemovedInOSPlatformAttribute)
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

        private static void ReportDiagnosticsForAll(PooledDictionary<IOperation, ImmutableArray<PlatformAttributeInfo>> platformSpecificOperations, OperationBlockAnalysisContext context)
        {
            foreach (var platformSpecificOperation in platformSpecificOperations)
            {
                ReportDiagnostics(platformSpecificOperation.Key, platformSpecificOperation.Value.FirstOrDefault(), context);
            }
        }

        private static void ReportDiagnostics(IOperation operation, PlatformAttributeInfo attribute, OperationBlockAnalysisContext context)
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
            List<PlatformAttributeInfo>? parsedTfms, ref PooledDictionary<IOperation, ImmutableArray<PlatformAttributeInfo>> platformSpecificOperations)
        {
            using var builder = ArrayBuilder<PlatformAttributeInfo>.GetInstance();
            AttributeData[]? attributes = null;

            if (operation is IInvocationOperation iOperation)
            {
                attributes = FindAllPlatformAttributesApplied(iOperation.TargetMethod.GetAttributes(), iOperation.TargetMethod.ContainingType);
            }
            else if (operation is IPropertyReferenceOperation pOperation)
            {
                if (!IsWithinConditionalOperation(operation))
                    attributes = FindAllPlatformAttributesApplied(pOperation.Property.GetAttributes(), pOperation.Property.ContainingType);
            }
            else if (operation is IFieldReferenceOperation fOperation)
            {
                if (!IsWithinConditionalOperation(operation))
                    attributes = FindAllPlatformAttributesApplied(fOperation.Field.GetAttributes(), fOperation.Field.ContainingType);
            }

            if (attributes != null)
            {
                foreach (AttributeData attribute in attributes)
                {
                    if (PlatformAttributeInfo.TryParseAttributeData(attribute, out PlatformAttributeInfo parsedAttribute))
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

        private static bool IsWithinConditionalOperation(IOperation pOperation) =>
         pOperation.Parent is IBinaryOperation bo &&
            (bo.OperatorKind == BinaryOperatorKind.Equals ||
            bo.OperatorKind == BinaryOperatorKind.NotEquals ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThan ||
            bo.OperatorKind == BinaryOperatorKind.LessThan ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual ||
            bo.OperatorKind == BinaryOperatorKind.LessThanOrEqual);

        private static bool IsSuppressedByTfm(List<PlatformAttributeInfo>? parsedTfms, PlatformAttributeInfo parsedAttribute)
        {
            if (parsedTfms != null)
            {
                foreach (PlatformAttributeInfo tfm in parsedTfms)
                {
                    if (IsOSPlatformsEqual(parsedAttribute.OsPlatformName, tfm.OsPlatformName))
                    {
                        return AttributeVersionsMatch(parsedAttribute, tfm);
                    }
                }
            }

            return false;
        }

        private static bool IsOSPlatformsEqual(string firstOs, string secondOs)
        {
            return firstOs.Equals(secondOs, StringComparison.OrdinalIgnoreCase)
                || (firstOs.Equals("OSX", StringComparison.OrdinalIgnoreCase) && secondOs.Equals("MACOS", StringComparison.OrdinalIgnoreCase))
                || (secondOs.Equals("OSX", StringComparison.OrdinalIgnoreCase) && firstOs.Equals("MACOS", StringComparison.OrdinalIgnoreCase));
        }

        private static DiagnosticDescriptor SwitchRule(PlatformAttributeType attributeType)
        {
            if (attributeType == PlatformAttributeType.MinimumOSPlatformAttribute)
                return MinimumOsRule;
            if (attributeType == PlatformAttributeType.ObsoletedInOSPlatformAttribute)
                return ObsoleteRule;
            return RemovedRule;
        }

        private static List<PlatformAttributeInfo>? ParseTfm(AnalyzerOptions options, ISymbol containingSymbol, Compilation compilation, CancellationToken cancellationToken)
        {
            string? tfmString = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetFramework, MinimumOsRule, containingSymbol, compilation, cancellationToken);
            if (tfmString != null)
            {
                using var builder = ArrayBuilder<PlatformAttributeInfo>.GetInstance();

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
                        if (PlatformAttributeInfo.TryParseTfmString(tokens[1], out PlatformAttributeInfo parsedTfm))
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

        private static PlatformAttributeInfo CreateWindowsOnlyPlatformInfo()
        {
            var platformInfo = new PlatformAttributeInfo();
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

        private static bool IsSuppressedByAttribute(PlatformAttributeInfo diagnosingAttribute, ISymbol containingSymbol)
        {
            while (containingSymbol != null)
            {
                var attributes = containingSymbol.GetAttributes();
                if (attributes != null)
                {
                    foreach (AttributeData attribute in attributes)
                    {
                        if (diagnosingAttribute.AttributeType.ToString() == attribute.AttributeClass.Name)
                        {
                            if (PlatformAttributeInfo.TryParseAttributeData(attribute, out PlatformAttributeInfo parsedAttribute))
                            {
                                if (IsOSPlatformsEqual(diagnosingAttribute.OsPlatformName, parsedAttribute.OsPlatformName) && AttributeVersionsMatch(diagnosingAttribute, parsedAttribute))
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

        private static bool AttributeVersionsMatch(PlatformAttributeInfo diagnosingAttribute, PlatformAttributeInfo osAttribute)
        {
            return AttributeVersionsMatch(diagnosingAttribute.AttributeType, diagnosingAttribute.Version, osAttribute.Version);
        }

        private static bool AttributeVersionsMatch(PlatformAttributeType attributeType, Version diagnosingVersion, Version suppressingVersion)
        {
            if (attributeType == PlatformAttributeType.MinimumOSPlatformAttribute)
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
                Debug.Assert(attributeType == PlatformAttributeType.ObsoletedInOSPlatformAttribute || attributeType == PlatformAttributeType.RemovedInOSPlatformAttribute);

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