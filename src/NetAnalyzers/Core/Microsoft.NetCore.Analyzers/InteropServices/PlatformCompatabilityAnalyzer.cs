// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        private static readonly ImmutableArray<string> s_platformCheckMethodNames = ImmutableArray.Create(IsOSPlatformVersionAtLeast, IsOSPlatform, IsBrowser, IsLinux, IsFreeBSD, IsFreeBSDVersionAtLeast,
            IsAndroid, IsAndroidVersionAtLeast, IsIOS, IsIOSVersionAtLeast, IsMacOS, IsMacOSVersionAtLeast, IsTvOS, IsTvOSVersionAtLeast, IsWatchOS, IsWatchOSVersionAtLeast, IsWindows, IsWindowsVersionAtLeast);
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
        private const string IsOSPlatformVersionAtLeast = nameof(IsOSPlatformVersionAtLeast);
        private const string IsOSPlatform = nameof(IsOSPlatform);
        private const string IsBrowser = nameof(IsBrowser);
        private const string IsLinux = nameof(IsLinux);
        private const string IsFreeBSD = nameof(IsFreeBSD);
        private const string IsFreeBSDVersionAtLeast = nameof(IsFreeBSDVersionAtLeast);
        private const string IsAndroid = nameof(IsAndroid);
        private const string IsAndroidVersionAtLeast = nameof(IsAndroidVersionAtLeast);
        private const string IsIOS = nameof(IsIOS);
        private const string IsIOSVersionAtLeast = nameof(IsIOSVersionAtLeast);
        private const string IsMacOS = nameof(IsMacOS);
        private const string IsMacOSVersionAtLeast = nameof(IsMacOSVersionAtLeast);
        private const string IsTvOS = nameof(IsTvOS);
        private const string IsTvOSVersionAtLeast = nameof(IsTvOSVersionAtLeast);
        private const string IsWatchOS = nameof(IsWatchOS);
        private const string IsWatchOSVersionAtLeast = nameof(IsWatchOSVersionAtLeast);
        private const string IsWindows = nameof(IsWindows);
        private const string IsWindowsVersionAtLeast = nameof(IsWindowsVersionAtLeast);

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
                var typeName = WellKnownTypeNames.SystemOperatingSystem;

                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName + "Helper", out var operatingSystemType))
                {
                    // TODO: remove 'typeName + "Helper"' after tests able to consume the real new APIs
                    operatingSystemType = context.Compilation.GetOrCreateTypeByMetadataName(typeName);
                }
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesOSPlatform, out var osPlatformType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesRuntimeInformation, out var runtimeInformationType))
                {
                    return;
                }

                var guardMethods = GetRuntimePlatformGuardMethods(runtimeInformationType, operatingSystemType!);

                context.RegisterOperationBlockStartAction(context => AnalyzeOperationBlock(context, guardMethods, osPlatformType));
            });

            static ImmutableArray<IMethodSymbol> GetRuntimePlatformGuardMethods(INamedTypeSymbol runtimeInformationType, INamedTypeSymbol operatingSystemType)
            {
                return operatingSystemType.GetMembers().OfType<IMethodSymbol>().Where(m => s_platformCheckMethodNames.Contains(m.Name)).ToImmutableArray().
                    Add(runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m => IsOSPlatform == m.Name).FirstOrDefault());
            }
        }

        private void AnalyzeOperationBlock(OperationBlockStartAnalysisContext context, ImmutableArray<IMethodSymbol> guardMethods, INamedTypeSymbol osPlatformType)
        {
            var platformSpecificOperations = PooledConcurrentDictionary<IOperation, SmallDictionary<string, PlatformAttributes>>.GetInstance();
            var platformSpecificMembers = PooledConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>>.GetInstance();

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
                        var attribute = platformSpecificOperation.Value;

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

        private static bool IsKnownValueGuarded(SmallDictionary<string, PlatformAttributes> attributes, GlobalFlowStateAnalysisValueSet value)
        {
            foreach (var analysisValue in value.AnalysisValues)
            {
                if (analysisValue is RuntimeMethodValue info)
                {
                    if (attributes.TryGetValue(info.PlatformName, out var attribute))
                    {
                        if (!info.Negated)
                        {
                            if (attribute.SupportedFirst != null)
                            {
                                if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, attribute.SupportedFirst, info.Version))
                                {
                                    attribute.SupportedFirst = null;
                                    continue;
                                }
                            }

                            if (attribute.SupportedSecond != null)
                            {
                                if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, attribute.SupportedSecond, info.Version))
                                {
                                    attribute.SupportedSecond = null;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (attribute.UnsupportedFirst != null)
                            {
                                if (AttributeVersionsMatch(PlatformAttributeType.UnsupportedOSPlatformAttribute, attribute.UnsupportedFirst, info.Version))
                                {
                                    attribute.UnsupportedFirst = null;
                                    continue;
                                }
                            }

                            if (attribute.Obsoleted != null)
                            {
                                if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, attribute.Obsoleted, info.Version))
                                {
                                    attribute.Obsoleted = null;
                                    continue;
                                }
                            }

                            if (attribute.UnsupportedSecond != null)
                            {
                                if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, attribute.UnsupportedSecond, info.Version))
                                {
                                    attribute.UnsupportedSecond = null;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var attribute in attributes)
            {
                if (attribute.Value.HasAttribute())
                {
                    return false;
                }
            }

            return true;
        }

        private static void ReportDiagnosticsForAll(PooledConcurrentDictionary<IOperation, SmallDictionary<string, PlatformAttributes>> platformSpecificOperations, OperationBlockAnalysisContext context)
        {
            foreach (var platformSpecificOperation in platformSpecificOperations)
            {
                ReportDiagnostics(platformSpecificOperation.Key, platformSpecificOperation.Value, context);
            }
        }

        private static void ReportDiagnostics(IOperation operation, SmallDictionary<string, PlatformAttributes> attributes, OperationBlockAnalysisContext context)
        {
            var operationName = GetOperationSymbol(operation)?.Name ?? string.Empty;

            foreach (var platformName in attributes.Keys)
            {
                var attribute = attributes[platformName];

                if (attribute.SupportedFirst != null)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.SupportedOSPlatformAttribute),
                       operationName, platformName, attribute.SupportedFirst));
                }
                if (attribute.SupportedSecond != null)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.SupportedOSPlatformAttribute),
                       operationName, platformName, attribute.SupportedSecond));
                }
                if (attribute.UnsupportedFirst != null)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.UnsupportedOSPlatformAttribute),
                       operationName, platformName, attribute.UnsupportedFirst));
                }
                if (attribute.UnsupportedSecond != null)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.UnsupportedOSPlatformAttribute),
                       operationName, platformName, attribute.UnsupportedSecond));
                }
                if (attribute.Obsoleted != null)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(PlatformAttributeType.ObsoletedInOSPlatformAttribute),
                       operationName, platformName, attribute.Obsoleted));
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
            PooledConcurrentDictionary<IOperation, SmallDictionary<string, PlatformAttributes>> platformSpecificOperations,
            PooledConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>> platformSpecificMembers)
        {
            var symbol = GetOperationSymbol(operation);

            if (symbol == null)
            {
                return;
            }

            if (!platformSpecificMembers.TryGetValue(symbol.OriginalDefinition, out var operationAttributes))
            {
                if (TryFindPlatformAttributesApplied(symbol.GetAttributes(), symbol.OriginalDefinition.ContainingSymbol, out operationAttributes))
                {
                    platformSpecificMembers.TryAdd(symbol.OriginalDefinition, operationAttributes);
                }
            }

            if (operationAttributes != null && operationAttributes.Any())
            {
                if (!platformSpecificMembers.TryGetValue(context.ContainingSymbol.OriginalDefinition, out var callSiteAttribute))
                {
                    if (TryFindContainingSymbolPlatformAttributes(context.ContainingSymbol, out callSiteAttribute))
                    {
                        platformSpecificMembers.TryAdd(context.ContainingSymbol.OriginalDefinition, callSiteAttribute);
                    }
                }

                if (callSiteAttribute != null && callSiteAttribute.Any())
                {
                    if (IsNotSuppressedByCallSite(operationAttributes, callSiteAttribute, out var notSuppressedAttributes))
                    {
                        platformSpecificOperations.TryAdd(operation, notSuppressedAttributes);
                    }
                }
                else
                {
                    var copy = CopyOperationAttributes(operationAttributes);
                    platformSpecificOperations.TryAdd(operation, copy);
                }
            }
        }

        private static SmallDictionary<string, PlatformAttributes> CopyOperationAttributes(SmallDictionary<string, PlatformAttributes> attributes)
        {
            var copy = new SmallDictionary<string, PlatformAttributes>();
            foreach (var attribute in attributes)
            {
                copy.Add(attribute.Key, CopyAllAttributes(new PlatformAttributes(), attribute.Value));
            }
            return copy;
        }

        /// <summary>
        /// If a member has any SupportedPlatforms attribute   
        /// </summary>
        /// <param name="operationAttributes">Platform specific attributes applied to the invoked member</param>
        /// <param name="callSiteAttributes">Platform specific attributes applied to the call site where the member invoked</param>
        /// <returns></returns>

        private static bool IsNotSuppressedByCallSite(SmallDictionary<string, PlatformAttributes> operationAttributes, SmallDictionary<string, PlatformAttributes> callSiteAttributes, out SmallDictionary<string, PlatformAttributes> notSuppressedAttributes)
        {
            notSuppressedAttributes = new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);
            bool? supportedOnlyList = null;
            foreach (string key in operationAttributes.Keys)
            {
                var attribute = operationAttributes[key];
                var diagnositcAttribute = new PlatformAttributes();

                if (attribute.SupportedFirst != null)
                {
                    if (attribute.UnsupportedFirst == null || attribute.UnsupportedFirst > attribute.SupportedFirst) // only for current platform
                    {
                        if (supportedOnlyList.HasValue && !supportedOnlyList.Value)
                        {
                            // report inconsistent list diagnostic
                            return true; // do not need to add this API to the list
                        }
                        else
                        {
                            supportedOnlyList = true;
                        }

                        if (callSiteAttributes.TryGetValue(key, out var callSiteAttribute))
                        {
                            if (attribute.SupportedSecond != null)
                            {
                                if (!MandatoryOsVersionsSuppressed(callSiteAttribute, attribute.SupportedSecond))
                                {
                                    diagnositcAttribute.SupportedSecond = (Version)attribute.SupportedSecond.Clone();
                                }
                            }
                            else
                            {
                                if (!MandatoryOsVersionsSuppressed(callSiteAttribute, attribute.SupportedFirst))
                                {
                                    diagnositcAttribute.SupportedFirst = (Version)attribute.SupportedFirst.Clone();
                                }
                            }

                            if (attribute.UnsupportedFirst != null)
                            {
                                if (!SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst))
                                {
                                    diagnositcAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                                }
                            }

                            if (attribute.Obsoleted != null)
                            {
                                if (attribute.SupportedSecond != null && attribute.SupportedSecond > attribute.Obsoleted || attribute.SupportedFirst > attribute.Obsoleted)
                                {
                                    // Can supported version be greater than obsoleted? Do we want to report diagnostic about wrong version here?
                                }
                                else if (!ObsoletedSuppressed(callSiteAttribute.Obsoleted, attribute.Obsoleted))
                                {
                                    diagnositcAttribute.Obsoleted = (Version)attribute.Obsoleted.Clone();
                                }
                            }
                        }
                        else
                        {
                            CopyAllAttributes(diagnositcAttribute, attribute);
                        }
                    }
                    else if (attribute.UnsupportedFirst != null) // also means Unsupported < Supported, optional list
                    {
                        if (supportedOnlyList.HasValue && supportedOnlyList.Value)
                        {
                            // report inconsistent list diagnostic
                            return true; // do not need to add this API to the list
                        }
                        else
                        {
                            supportedOnlyList = false;
                        }

                        if (callSiteAttributes.TryGetValue(key, out var callSiteAttribute))
                        {
                            if (!OptionalOsVersionsSuppressed(callSiteAttribute, attribute))
                            {
                                diagnositcAttribute.SupportedFirst = (Version)attribute.SupportedFirst.Clone();
                            }

                            if (!UnsupportedFirstSuppressed(attribute, callSiteAttribute))
                            {
                                diagnositcAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                                continue;
                            }

                            if (attribute.UnsupportedSecond != null && !UnsupportedSecondSuppressed(attribute, callSiteAttribute))
                            {
                                diagnositcAttribute.UnsupportedSecond = (Version)attribute.UnsupportedSecond.Clone();
                            }
                        }
                        else
                        {
                            CopyAllAttributes(diagnositcAttribute, attribute);
                        }
                    }
                }
                else
                {
                    if (supportedOnlyList.HasValue && supportedOnlyList.Value)
                    {
                        // report Inconsistent list diagnostic
                    }
                    else
                    {
                        supportedOnlyList = false;
                    }

                    if (attribute.UnsupportedFirst != null) // Unsupported for this but supported all other
                    {
                        if (callSiteAttributes.TryGetValue(key, out var callSiteAttribute))
                        {
                            if (!SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst))
                            {
                                diagnositcAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                            }

                            if (attribute.UnsupportedSecond != null && !SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedSecond))
                            {
                                diagnositcAttribute.UnsupportedSecond = (Version)attribute.UnsupportedSecond.Clone();
                            }
                        }
                        else
                        {
                            diagnositcAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                            diagnositcAttribute.UnsupportedSecond = (Version?)attribute.UnsupportedSecond?.Clone();
                        }
                    }

                    if (attribute.Obsoleted != null)
                    {
                        // When no supported attribute exist, obsoleted not expected, reoport diagnostic
                    }
                }

                if (diagnositcAttribute.HasAttribute())
                {
                    notSuppressedAttributes[key] = diagnositcAttribute;
                }
            }

            return notSuppressedAttributes.Any();
        }

        private static PlatformAttributes CopyAllAttributes(PlatformAttributes copyTo, PlatformAttributes copyFrom)
        {
            copyTo.SupportedFirst = (Version?)copyFrom.SupportedFirst?.Clone();
            copyTo.SupportedSecond = (Version?)copyFrom.SupportedSecond?.Clone();
            copyTo.UnsupportedFirst = (Version?)copyFrom.UnsupportedFirst?.Clone();
            copyTo.UnsupportedSecond = (Version?)copyFrom.UnsupportedSecond?.Clone();
            copyTo.Obsoleted = (Version?)copyFrom.Obsoleted?.Clone();
            return copyTo;
        }

        private static bool SuppressedByCallSiteUnsupported(PlatformAttributes callSiteAttribute, Version unsupporteAttribute)
        {
            if (callSiteAttribute.UnsupportedFirst != null && AttributeVersionsMatch(PlatformAttributeType.UnsupportedOSPlatformAttribute, unsupporteAttribute, callSiteAttribute.UnsupportedFirst) ||
                callSiteAttribute.UnsupportedSecond != null && AttributeVersionsMatch(PlatformAttributeType.UnsupportedOSPlatformAttribute, unsupporteAttribute, callSiteAttribute.UnsupportedSecond))
            {
                return true;
            }
            return false;
        }

        private static bool ObsoletedSuppressed(Version? callSiteObsoleted, Version checkingObsoleted) => callSiteObsoleted != null
                && AttributeVersionsMatch(PlatformAttributeType.ObsoletedInOSPlatformAttribute, checkingObsoleted, callSiteObsoleted);

        private static bool UnsupportedSecondSuppressed(PlatformAttributes attribute, PlatformAttributes callSiteAttribute)
        {
            if (callSiteAttribute.SupportedFirst != null)
            {
                if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, callSiteAttribute.SupportedFirst, attribute.SupportedFirst!) ||
                    attribute.SupportedSecond != null && AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, callSiteAttribute.SupportedFirst, attribute.SupportedSecond))
                {
                    return true;
                }
            }

            return SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedSecond!);
        }

        private static bool UnsupportedFirstSuppressed(PlatformAttributes attribute, PlatformAttributes callSiteAttribute)
        {
            if (callSiteAttribute.SupportedFirst != null)
            {
                if (AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, callSiteAttribute.SupportedFirst, attribute.SupportedFirst!) ||
                    attribute.SupportedSecond != null && AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, callSiteAttribute.SupportedFirst, attribute.SupportedSecond))
                {
                    return true;
                }
            }

            return SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst!);
        }

        private static bool OptionalOsVersionsSuppressed(PlatformAttributes callSiteAttribute, PlatformAttributes attribute)
        {
            // Optianal supported attribute, if call site supports it, its versions should match
            if (callSiteAttribute.SupportedFirst != null &&
                !(attribute.SupportedFirst <= callSiteAttribute.SupportedFirst ||
                 (callSiteAttribute.SupportedSecond != null && attribute.SupportedFirst <= callSiteAttribute.SupportedSecond)))
            {

                return false;
            }

            // if call site not suppors it, no problem
            return true;
        }

        private static bool MandatoryOsVersionsSuppressed(PlatformAttributes callSitePlatforms, Version checkingVersion)
        {
            if ((callSitePlatforms.SupportedFirst != null && AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, checkingVersion, callSitePlatforms.SupportedFirst)) ||
               (callSitePlatforms.SupportedSecond != null && AttributeVersionsMatch(PlatformAttributeType.SupportedOSPlatformAttribute, checkingVersion, callSitePlatforms.SupportedSecond)))
            {
                return true;
            }
            return false;
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

        private static bool TryFindPlatformAttributesApplied(ImmutableArray<AttributeData> immediateAttributes, ISymbol containingSymbol, [NotNullWhen(true)] out SmallDictionary<string, PlatformAttributes>? attributes)
        {
            attributes = null;
            AddPlatformAttributes(immediateAttributes, ref attributes);

            while (containingSymbol != null)
            {
                AddPlatformAttributes(containingSymbol.GetAttributes(), ref attributes);
                containingSymbol = containingSymbol.ContainingSymbol;
            }
            return attributes != null;
        }

        private static bool AddPlatformAttributes(ImmutableArray<AttributeData> immediateAttributes, [NotNullWhen(true)] ref SmallDictionary<string, PlatformAttributes>? attributes)
        {
            foreach (AttributeData attribute in immediateAttributes)
            {
                if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name))
                {
                    TryAddValidAttribute(ref attributes, attribute);
                }
            }
            return attributes != null;
        }

        private static bool TryFindContainingSymbolPlatformAttributes(ISymbol containingSymbol, [NotNullWhen(true)] out SmallDictionary<string, PlatformAttributes>? attributes)
        {
            attributes = null;
            while (containingSymbol != null)
            {
                AddPlatformAttributes(containingSymbol.GetAttributes(), ref attributes);
                containingSymbol = containingSymbol.ContainingSymbol;
            }

            return attributes != null;
        }

        private static bool TryAddValidAttribute([NotNullWhen(true)] ref SmallDictionary<string, PlatformAttributes>? attributes, AttributeData attribute)
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
                    attributes ??= new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);

                    if (!attributes.TryGetValue(platformName, out var existingAttributes))
                    {
                        existingAttributes = new PlatformAttributes();
                        attributes[platformName] = existingAttributes;
                    }

                    switch (attribute.AttributeClass.Name)
                    {
                        case ObsoletedInOSPlatformAttribute:
                            AddOrUpdateObsoletedAttribute(existingAttributes, version);
                            break;
                        case SupportedOSPlatformAttribute:
                            AddOrUpdateSupportedAttribute(existingAttributes, version);
                            break;
                        case UnsupportedOSPlatformAttribute:
                            AddOrUpdateUnsupportedAttribute(existingAttributes, version);
                            break;
                    }
                    return true;
                }
                // else report diagnostic = Diagnostic.Create(PlatformNameNullOrEmptyRule, osAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation());
            }
            else
            {
                // report Diagnostic.Create(InvalidPlatformVersionRule, osAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation());
            }

            return false;
        }

        private static void AddOrUpdateObsoletedAttribute(PlatformAttributes attributes, Version version)
        {
            if (attributes.Obsoleted != null)
            {
                if (attributes.Obsoleted > version)
                {
                    attributes.Obsoleted = version;
                }
            }
            else
            {
                attributes.Obsoleted = version;
            }
        }

        private static void AddOrUpdateUnsupportedAttribute(PlatformAttributes attributes, Version version)
        {
            if (attributes.UnsupportedFirst != null)
            {
                if (attributes.UnsupportedFirst > version)
                {
                    attributes.UnsupportedFirst = version;
                }
                else
                {
                    if (attributes.UnsupportedSecond != null)
                    {
                        if (attributes.UnsupportedSecond > version)
                        {
                            attributes.UnsupportedSecond = version;
                        }
                    }
                    else
                    {
                        attributes.UnsupportedSecond = version;
                    }
                }
            }
            else
            {
                attributes.UnsupportedFirst = version;
            }
        }

        private static void AddOrUpdateSupportedAttribute(PlatformAttributes attributes, Version version)
        {
            if (attributes.SupportedFirst != null)
            {
                if (attributes.SupportedFirst > version)
                {
                    attributes.SupportedFirst = version;
                }
                else
                {
                    if (attributes.SupportedSecond != null)
                    {
                        if (attributes.SupportedSecond < version)
                        {
                            attributes.SupportedSecond = version;
                        }
                    }
                    else
                    {
                        attributes.SupportedSecond = version;
                    }
                }
            }
            else
            {
                attributes.SupportedFirst = version;
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