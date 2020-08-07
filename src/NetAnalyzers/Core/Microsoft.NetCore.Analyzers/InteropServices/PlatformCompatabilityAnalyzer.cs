// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed partial class PlatformCompatabilityAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1416";
        private static readonly ImmutableArray<string> s_platformCheckMethodNames = ImmutableArray.Create(IsOSPlatformOrLater, IsOSPlatformEarlierThan);
        private static readonly ImmutableArray<string> s_osPlatformAttributes = ImmutableArray.Create(SupportedOSPlatformAttribute, ObsoletedInOSPlatformAttribute, UnsupportedOSPlatformAttribute);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableSupportedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckSupportedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableObsoleteMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckObsoleteMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableUnsupportedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckUnsupportedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        // We are adding the new attributes into older versions of .Net 5.0, so there could be multiple referenced assemblies each with their own 
        // version of internal attribute type which will cause ambiguity, to avoid that we are comparing the attributes by their name
        private const string SupportedOSPlatformAttribute = nameof(SupportedOSPlatformAttribute);
        private const string ObsoletedInOSPlatformAttribute = nameof(ObsoletedInOSPlatformAttribute);
        private const string UnsupportedOSPlatformAttribute = nameof(UnsupportedOSPlatformAttribute);

        // Platform guard method names
        private const string IsOSPlatformOrLater = nameof(IsOSPlatformOrLater);
        private const string IsOSPlatformEarlierThan = nameof(IsOSPlatformEarlierThan);

        internal static DiagnosticDescriptor SupportedOsRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableSupportedMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor ObsoleteOsRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableSupportedMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableObsoleteMessage,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);
        internal static DiagnosticDescriptor UnsupportedOsRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableSupportedMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableUnsupportedMessage,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SupportedOsRule, ObsoleteOsRule, UnsupportedOsRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var typeName = WellKnownTypeNames.SystemRuntimeInteropServicesRuntimeInformation;

                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName + "Helper", out var runtimeInformationType))
                {
                    // TODO: remove 'typeName + "Helper"' load after tests able to use
                    runtimeInformationType = context.Compilation.GetOrCreateTypeByMetadataName(typeName);
                }
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesOSPlatform, out var osPlatformType))
                {
                    return;
                }

                var guardMethods = GetRuntimePlatformGuardMethods(runtimeInformationType!);

                //context.RegisterSymbolAction(AnalyzeSymbolAction, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event, SymbolKind.Assembly, SymbolKind.Method, SymbolKind.NamedType);

                context.RegisterOperationBlockStartAction(context => AnalyzeOperationBlock(context, guardMethods, osPlatformType));
            });

            static ImmutableArray<IMethodSymbol> GetRuntimePlatformGuardMethods(INamedTypeSymbol runtimeInformationType)
            {
                return runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    s_platformCheckMethodNames.Contains(m.Name) && !m.Parameters.IsEmpty).ToImmutableArray();
            }
        }

        private void AnalyzeOperationBlock(OperationBlockStartAnalysisContext context, ImmutableArray<IMethodSymbol> guardMethods, INamedTypeSymbol osPlatformType)
        {
            var platformSpecificOperations = PooledConcurrentDictionary<IOperation, PlatformAttributes>.GetInstance();
            var platformSpecificMembers = PooledConcurrentDictionary<ISymbol, PlatformAttributes>.GetInstance();

            context.RegisterOperationAction(context =>
            {
                AnalyzeOperation(context.Operation, context, platformSpecificOperations, platformSpecificMembers);
            },
            OperationKind.MethodReference,
            OperationKind.EventReference,
            OperationKind.FieldReference,
            OperationKind.Invocation,
            OperationKind.ObjectCreation,
            OperationKind.PropertyReference);

            context.RegisterOperationBlockEndAction(context =>
            {
                try
                {
                    if (platformSpecificOperations.IsEmpty)
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
                        context.Options, SupportedOsRule, performValueContentAnalysis: true,
                        context.CancellationToken, out var valueContentAnalysisResult);

                    if (analysisResult == null)
                    {
                        return;
                    }

                    foreach (var platformSpecificOperation in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Key.Kind, platformSpecificOperation.Key.Syntax];
                        PlatformAttributes attribute = platformSpecificOperation.Value;

                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Unknown)
                        {
                            if (platformSpecificOperation.Key.TryGetContainingLocalOrLambdaFunctionSymbol(out var containingSymbol))
                            {
                                var localResult = analysisResult.TryGetInterproceduralResultByDefinition(containingSymbol);
                                if (localResult != null)
                                {
                                    var localValue = localResult[platformSpecificOperation.Key.Kind, platformSpecificOperation.Key.Syntax];
                                    if (localValue.Kind == GlobalFlowStateAnalysisValueSetKind.Known)
                                    {
                                        if (IsKnownValueGuarded(attribute, localValue))
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        else if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Known)
                        {
                            if (IsKnownValueGuarded(attribute, value))
                            {
                                continue;
                            }
                        }

                        ReportDiagnostics(platformSpecificOperation.Key, attribute, context);
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

        private static bool IsKnownValueGuarded(PlatformAttributes attribute, GlobalFlowStateAnalysisValueSet value)
        {
            foreach (var analysisValue in value.AnalysisValues)
            {
                if (analysisValue is RuntimeMethodValue info)
                {
                    if (!info.Negated)
                    {
                        if (attribute.SupportedPlatforms.TryGetValue(info.PlatformName, out var versions))
                        {
                            if (info.InvokedMethodName == IsOSPlatformOrLater)
                            {
                                foreach (var version in versions)
                                {
                                    if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, version, info.Version))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        if (attribute.UnsupportedPlatforms.TryGetValue(info.PlatformName, out versions))
                        {
                            if (info.InvokedMethodName == IsOSPlatformEarlierThan)
                            {
                                foreach (var version in versions)
                                {
                                    if (AttributeVersionsMatch(PlatformAttributeType.UnsupportedOSPlatformAttribute, version, info.Version))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        if (attribute.ObsoletedPlatforms.TryGetValue(info.PlatformName, out var obsoletedVersion))
                        {
                            if (info.InvokedMethodName == IsOSPlatformEarlierThan)
                            {
                                if (AttributeVersionsMatch(PlatformAttributeType.ObsoletedInOSPlatformAttribute, obsoletedVersion, info.Version))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static void ReportDiagnosticsForAll(PooledConcurrentDictionary<IOperation, PlatformAttributes> platformSpecificOperations, OperationBlockAnalysisContext context)
        {
            foreach (var platformSpecificOperation in platformSpecificOperations)
            {
                ReportDiagnostics(platformSpecificOperation.Key, platformSpecificOperation.Value, context);
            }
        }

        private static void ReportDiagnostics(IOperation operation, PlatformAttributes attributes, OperationBlockAnalysisContext context)
        {
            var operationName = GetOperationSymbol(operation)?.Name;

            if (attributes.SupportedPlatforms.Any())
            {
                foreach (var attr in attributes.SupportedPlatforms)
                {
                    foreach (var version in attr.Value)
                    {
                        context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.SupportedOSPlatformAttribute),
                        operationName ?? string.Empty, attr.Key, version.ToString()));
                    }
                }
            }
            if (attributes.UnsupportedPlatforms.Any())
            {
                foreach (var attr in attributes.UnsupportedPlatforms)
                {
                    foreach (var version in attr.Value)
                    {
                        context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.UnsupportedOSPlatformAttribute),
                        operationName ?? string.Empty, attr.Key, version.ToString()));
                    }
                }
            }
            if (attributes.ObsoletedPlatforms.Any())
            {
                foreach (var attr in attributes.ObsoletedPlatforms)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.SupportedOSPlatformAttribute),
                    operationName ?? string.Empty, attr.Key, attr.Value.ToString()));
                }
            }
        }

        private static ISymbol? GetOperationSymbol(IOperation operation)
            => operation switch
            {
                IInvocationOperation iOperation => iOperation.TargetMethod,
                IObjectCreationOperation cOperation => cOperation.Constructor,
                IFieldReferenceOperation fOperation => IsWithinConditionalOperation(fOperation) ? null : fOperation.Field,
                IMemberReferenceOperation mOperation => mOperation.Member,
                _ => null,
            };

        private static void AnalyzeOperation(IOperation operation, OperationAnalysisContext context,
            PooledConcurrentDictionary<IOperation, PlatformAttributes> platformSpecificOperations,
            PooledConcurrentDictionary<ISymbol, PlatformAttributes> platformSpecificMembers)
        {
            var symbol = GetOperationSymbol(operation);

            if (symbol == null)
            {
                return;
            }

            if (!platformSpecificMembers.TryGetValue(symbol.OriginalDefinition, out var operationAttributes))
            {
                if (FindPlatformAttributesApplied(symbol.GetAttributes(), symbol.OriginalDefinition.ContainingSymbol, out operationAttributes))
                {
                    platformSpecificMembers.TryAdd(symbol.OriginalDefinition, operationAttributes);
                }
            }

            if (operationAttributes.HasAttribute)
            {
                if (!platformSpecificMembers.TryGetValue(context.ContainingSymbol, out var callSiteAttribute))
                {
                    if (FindContainingSymbolPlatformAttributes(context.ContainingSymbol, out callSiteAttribute))
                    {
                        platformSpecificMembers.TryAdd(context.ContainingSymbol, callSiteAttribute);
                    }
                }

                if (callSiteAttribute.HasAttribute)
                {
                    if (!IsSuppressedByCallSite(operationAttributes, callSiteAttribute, out PlatformAttributes notSuppressedAttributes))
                    {
                        platformSpecificOperations.TryAdd(operation, notSuppressedAttributes);
                    }
                }
                else
                {
                    platformSpecificOperations.TryAdd(operation, operationAttributes);
                }
            }
        }

        /// <summary>
        /// If a member has any SupportedPlatforms attribute   
        /// </summary>
        /// <param name="operationAttributes">Platform specific attributes applied to the invoked member</param>
        /// <param name="callSiteAttribute">Platform specific attributes applied to the call site where the member invoked</param>
        /// <returns></returns>

        private static bool IsSuppressedByCallSite(PlatformAttributes operationAttributes, PlatformAttributes callSiteAttribute, out PlatformAttributes notSuppressedAttributes)
        {
            Debug.Assert(operationAttributes.HasAttribute && callSiteAttribute.HasAttribute);
            notSuppressedAttributes = new PlatformAttributes(new SmallDictionary<string, PooledSortedSet<Version>>(StringComparer.InvariantCultureIgnoreCase),
                new SmallDictionary<string, PooledSortedSet<Version>>(StringComparer.InvariantCultureIgnoreCase),
                new SmallDictionary<string, Version>(StringComparer.InvariantCultureIgnoreCase));
            if (operationAttributes.SupportedPlatforms.Any())
            {
                bool? mandatoryList = null;
                foreach (string key in operationAttributes.SupportedPlatforms.Keys)
                {
                    var supportedVersions = operationAttributes.SupportedPlatforms[key];

                    if (operationAttributes.UnsupportedPlatforms.TryGetValue(key, out PooledSortedSet<Version>? unsupportedVersion))
                    {
                        if (supportedVersions.Min < unsupportedVersion.Min) // only for current platform
                        {
                            if (mandatoryList.HasValue && !mandatoryList.Value)
                            {
                                // report inconsistent list diagnostic
                                return false;
                            }
                            else
                            {
                                mandatoryList = true;
                            }

                            if (!MandatoryOsVersionsSuppressed(callSiteAttribute.SupportedPlatforms, key, supportedVersions, notSuppressedAttributes.SupportedPlatforms))
                            {
                                return false;
                            }
                        }
                        else if (supportedVersions.Min == unsupportedVersion.Min)
                        {
                            // report inconsistent list diagnostic
                            return false;
                        }
                        else // supported for all platforms
                        {
                            if (mandatoryList.HasValue && mandatoryList.Value)
                            {
                                // report Inconsistent list diagnostic
                            }
                            else
                            {
                                mandatoryList = false;
                            }

                            if (!OptionalOsVersionsSuppressed(callSiteAttribute.SupportedPlatforms, key, supportedVersions, notSuppressedAttributes.SupportedPlatforms))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!MandatoryOsVersionsSuppressed(callSiteAttribute.SupportedPlatforms, key, supportedVersions, notSuppressedAttributes.SupportedPlatforms))
                        {
                            return false;
                        }
                    }
                }
            }

            if (operationAttributes.UnsupportedPlatforms.Any())
            {
                foreach (string key in operationAttributes.UnsupportedPlatforms.Keys)
                {
                    var unsupportedVersions = operationAttributes.UnsupportedPlatforms[key];

                    if (callSiteAttribute.SupportedPlatforms.TryGetValue(key, out PooledSortedSet<Version>? callSiteSupportedVersions))
                    {
                        if (!SuppressedBySupported(operationAttributes.SupportedPlatforms, key, callSiteSupportedVersions))
                        {
                            if (callSiteAttribute.UnsupportedPlatforms.TryGetValue(key, out PooledSortedSet<Version>? callSiteUnsupportedVersions))
                            {
                                foreach (var unsupportedVersion in unsupportedVersions)
                                {
                                    if (!callSiteUnsupportedVersions.Any(v => AttributeVersionsMatch(PlatformAttributeType.ObsoletedInOSPlatformAttribute, unsupportedVersion, v))) //unsupportedVersion < v))
                                    {
                                        AddToDiagnostics(key, notSuppressedAttributes.UnsupportedPlatforms, unsupportedVersion);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // No any supported OS specified means for all OS
                        if (!operationAttributes.SupportedPlatforms.Any())
                        {
                            if (callSiteAttribute.UnsupportedPlatforms.TryGetValue(key, out PooledSortedSet<Version>? callSiteUnsupportedVersions))
                            {
                                foreach (var unsupportedVersion in unsupportedVersions)
                                {
                                    if (!callSiteUnsupportedVersions.Any(v => AttributeVersionsMatch(PlatformAttributeType.ObsoletedInOSPlatformAttribute, unsupportedVersion, v))) //unsupportedVersion < v))
                                    {
                                        AddToDiagnostics(key, notSuppressedAttributes.UnsupportedPlatforms, unsupportedVersion);
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var unsupportedVersion in unsupportedVersions)
                                {
                                    AddToDiagnostics(key, notSuppressedAttributes.UnsupportedPlatforms, unsupportedVersion);
                                }
                                return false;
                            }
                        }
                    }
                }
            }

            if (operationAttributes.ObsoletedPlatforms.Any())
            {
                foreach (string key in operationAttributes.ObsoletedPlatforms.Keys)
                {
                    var obsoletedVersion = operationAttributes.ObsoletedPlatforms[key];
                    if (operationAttributes.SupportedPlatforms.TryGetValue(key, out PooledSortedSet<Version>? supportedVersion))
                    {
                        if (supportedVersion.Max < obsoletedVersion)
                        {
                            if (!callSiteAttribute.ObsoletedPlatforms.TryGetValue(key, out Version? suppressingVersion) ||
                                !AttributeVersionsMatch(PlatformAttributeType.ObsoletedInOSPlatformAttribute, obsoletedVersion, suppressingVersion)) //obsoletedVersion >= suppressingVersion)
                            {
                                notSuppressedAttributes.ObsoletedPlatforms[key] = obsoletedVersion;
                                return false;
                            }
                        }
                        else
                        {
                            // Can supported version be greater than obsoleted? Do we want to report diagnostic about wrong version?
                        }
                    }
                    else
                    {
                        if (!operationAttributes.SupportedPlatforms.Any())
                        {
                            // No any os specified means for all OS
                            if (!callSiteAttribute.ObsoletedPlatforms.TryGetValue(key, out Version? suppressingVersion) ||
                                !AttributeVersionsMatch(PlatformAttributeType.ObsoletedInOSPlatformAttribute, obsoletedVersion, suppressingVersion)) //obsoletedVersion >= suppressingVersion)
                            {
                                notSuppressedAttributes.ObsoletedPlatforms[key] = obsoletedVersion;
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static bool SuppressedBySupported(SmallDictionary<string, PooledSortedSet<Version>> supportedPlatforms, string key, PooledSortedSet<Version> callSiteSupportedVersions)
        {
            if (supportedPlatforms.TryGetValue(key, out PooledSortedSet<Version>? supportedVersions))
            {
                foreach (var calledVersion in callSiteSupportedVersions)
                {
                    if (supportedVersions.Any(v => AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, calledVersion, v))) //v <= calledVersion))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool OptionalOsVersionsSuppressed(SmallDictionary<string, PooledSortedSet<Version>> callSitePlatforms, string key,
            PooledSortedSet<Version> supportedVersions, SmallDictionary<string, PooledSortedSet<Version>> notSuppressedVersions)
        {
            if (callSitePlatforms.TryGetValue(key, out PooledSortedSet<Version>? suppressingVersions))
            {
                foreach (var supportedVersion in supportedVersions)
                {
                    if (!suppressingVersions.Any(v => AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, supportedVersion, v)))//supportedVersion <= v))
                    {
                        AddToDiagnostics(key, notSuppressedVersions, supportedVersion);

                        return false;
                    }
                }
            }
            return true;
        }

        private static void AddToDiagnostics(string key, SmallDictionary<string, PooledSortedSet<Version>> versions, Version version)
        {
            if (versions.TryGetValue(key, out var existing))
            {
                existing.Add(version);
            }
            else
            {
                var set = PooledSortedSet<Version>.GetInstance();
                set.Add(version);
                versions.Add(key, set);
            }
        }

        private static bool MandatoryOsVersionsSuppressed(SmallDictionary<string, PooledSortedSet<Version>> callSitePlatforms,
            string key, PooledSortedSet<Version> checkingVersions, SmallDictionary<string, PooledSortedSet<Version>> notSuppressedVersions)
        {
            if (callSitePlatforms.TryGetValue(key, out PooledSortedSet<Version>? suppressingVersions))
            {
                foreach (var checkingVersion in checkingVersions)
                {
                    if (!suppressingVersions.Any(v => AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, checkingVersion, v))) //checkingVersion <= v))
                    {
                        AddToDiagnostics(key, notSuppressedVersions, checkingVersion);

                        return false;
                    }
                }
            }
            else
            {
                if (!notSuppressedVersions.TryGetValue(key, out var existing))
                {
                    existing = PooledSortedSet<Version>.GetInstance();
                    notSuppressedVersions.Add(key, existing);
                }

                foreach (var checkingVersion in checkingVersions)
                {
                    existing.Add(checkingVersion);
                }
                return false;
            }
            return true;
        }

        // Do not warn for conditional checks of platfomr specific enum value; 'if (value != FooEnum.WindowsOnlyValue)'
        private static bool IsWithinConditionalOperation(IFieldReferenceOperation pOperation) =>
            pOperation.ConstantValue.HasValue &&
            pOperation.Parent is IBinaryOperation bo &&
            (bo.OperatorKind == BinaryOperatorKind.Equals ||
            bo.OperatorKind == BinaryOperatorKind.NotEquals ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThan ||
            bo.OperatorKind == BinaryOperatorKind.LessThan ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual ||
            bo.OperatorKind == BinaryOperatorKind.LessThanOrEqual);

        private static DiagnosticDescriptor SelectRule(PlatformAttributeType attributeType)
            => attributeType switch
            {
                PlatformAttributeType.SupportedOSPlatformAttribute => SupportedOsRule,
                PlatformAttributeType.ObsoletedInOSPlatformAttribute => ObsoleteOsRule,
                _ => UnsupportedOsRule,
            };

        private static bool FindPlatformAttributesApplied(ImmutableArray<AttributeData> immediateAttributes, ISymbol containingSymbol, out PlatformAttributes attributes)
        {
            attributes = new PlatformAttributes(new SmallDictionary<string, PooledSortedSet<Version>>(StringComparer.InvariantCultureIgnoreCase),
                new SmallDictionary<string, PooledSortedSet<Version>>(StringComparer.InvariantCultureIgnoreCase),
                new SmallDictionary<string, Version>(StringComparer.InvariantCultureIgnoreCase));
            AddPlatformAttributes(immediateAttributes, attributes);

            while (containingSymbol != null)
            {
                AddPlatformAttributes(containingSymbol.GetAttributes(), attributes);
                containingSymbol = containingSymbol.ContainingSymbol;
            }
            return attributes.HasAttribute;
        }

        private static void AddPlatformAttributes(ImmutableArray<AttributeData> immediateAttributes, PlatformAttributes attributes)
        {
            foreach (AttributeData attribute in immediateAttributes)
            {
                if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name))
                {
                    AddValidAttribute(attributes, attribute);
                }
            }
        }

        private static bool FindContainingSymbolPlatformAttributes(ISymbol containingSymbol, out PlatformAttributes attributes)
        {
            attributes = new PlatformAttributes(new SmallDictionary<string, PooledSortedSet<Version>>(StringComparer.InvariantCultureIgnoreCase),
                new SmallDictionary<string, PooledSortedSet<Version>>(StringComparer.InvariantCultureIgnoreCase),
                new SmallDictionary<string, Version>(StringComparer.InvariantCultureIgnoreCase));
            while (containingSymbol != null)
            {
                AddPlatformAttributes(containingSymbol.GetAttributes(), attributes);
                containingSymbol = containingSymbol.ContainingSymbol;
            }

            return attributes.HasAttribute;
        }

        private static PlatformAttributes AddValidAttribute(PlatformAttributes attributes, AttributeData attribute)
        {
            if (!attribute.ConstructorArguments.IsEmpty &&
                                attribute.ConstructorArguments[0] is { } argument &&
                                argument.Kind == TypedConstantKind.Primitive &&
                                argument.Type.SpecialType == SpecialType.System_String &&
                                !argument.IsNull &&
                                !argument.Value.Equals(string.Empty))
            {
                if (TryParsePlatformNameAndVersion(argument.Value.ToString(), out string platformName, out Version? version))
                {
                    if (attribute.AttributeClass.Name == ObsoletedInOSPlatformAttribute)
                    {
                        attributes.ObsoletedPlatforms[platformName] = version;
                    }
                    else
                    {
                        AddOrSetVersion(SwitchAttrributes(attribute.AttributeClass.Name, attributes), platformName, version);
                    }
                }
                // else report diagnostic = Diagnostic.Create(PlatformNameNullOrEmptyRule, osAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation());
            }
            else
            {
                // report Diagnostic.Create(InvalidPlatformVersionRule, osAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation());
            }

            return attributes;
        }

        private static SmallDictionary<string, PooledSortedSet<Version>> SwitchAttrributes(string osAttributeName, PlatformAttributes attributes)
                => osAttributeName switch
                {
                    SupportedOSPlatformAttribute => attributes.SupportedPlatforms,
                    UnsupportedOSPlatformAttribute => attributes.UnsupportedPlatforms,
                    _ => throw new NotImplementedException(),
                };

        private static void AddOrSetVersion(SmallDictionary<string, PooledSortedSet<Version>> dictionary, string platformName, Version version)
        {
            if (dictionary.TryGetValue(platformName, out PooledSortedSet<Version>? existingVersion))
            {
                existingVersion.Add(version);
            }
            else
            {
                dictionary[platformName] = PooledSortedSet<Version>.GetInstance();
                dictionary[platformName].Add(version);
            }
        }

        private static bool AttributeVersionsMatch(PlatformAttributeType attributeType, Version diagnosingVersion, Version suppressingVersion)
        {
            if (attributeType == PlatformAttributeType.SupportedOSPlatformAttribute)
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
                Debug.Assert(attributeType == PlatformAttributeType.ObsoletedInOSPlatformAttribute || attributeType == PlatformAttributeType.UnsupportedOSPlatformAttribute);

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

                return diagnosingVersion.Revision >= suppressingVersion.Revision;
            }
        }
    }
}