// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    /// <summary>
    /// CA1416: Analyzer that informs developers when they use platform-specific APIs from call sites where the API might not be available
    /// 
    /// It finds usage of platform-specific or obsoleted or unsupported or removed APIs and diagnoses if the 
    /// API is guarded by platform check or if it is annotated with corresponding platform specific attribute.
    /// If using the platform-specific API is not safe it reports diagnostics.
    ///
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed partial class PlatformCompatabilityAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1416";
        private static readonly ImmutableArray<string> s_platformCheckMethodNames = ImmutableArray.Create(IsOSPlatformVersionAtLeast, IsOSPlatform, IsBrowser, IsLinux, IsFreeBSD, IsFreeBSDVersionAtLeast,
            IsAndroid, IsAndroidVersionAtLeast, IsIOS, IsIOSVersionAtLeast, IsMacOS, IsMacOSVersionAtLeast, IsTvOS, IsTvOSVersionAtLeast, IsWatchOS, IsWatchOSVersionAtLeast, IsWindows, IsWindowsVersionAtLeast);
        private static readonly ImmutableArray<string> s_osPlatformAttributes = ImmutableArray.Create(SupportedOSPlatformAttribute, ObsoletedInOSPlatformAttribute, UnsupportedOSPlatformAttribute);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableRequiredOsMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckRequiredOsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableRequiredOsVersionMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckRequiredOsVersionMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableObsoleteMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckObsoleteMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableUnsupportedOsMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckUnsupportedOsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableUnsupportedOsVersionMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckUnsupportedOsVersionMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
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

        internal static DiagnosticDescriptor RequiredOsVersionRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableRequiredOsVersionMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor RequiredOsRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableRequiredOsMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor ObsoleteOsRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableObsoleteMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor UnsupportedOsRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableUnsupportedOsMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor UnsupportedOsVersionRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableUnsupportedOsVersionMessage,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RequiredOsRule, RequiredOsVersionRule, ObsoleteOsRule, UnsupportedOsRule, UnsupportedOsVersionRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

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

                var runtimeIsOSPlatformMethod = runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    IsOSPlatform == m.Name &&
                    m.IsStatic &&
                    m.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    m.Parameters.Length == 1 &&
                    m.Parameters[0].Type.Equals(osPlatformType)).FirstOrDefault();

                var guardMethods = GetRuntimePlatformGuardMethods(runtimeIsOSPlatformMethod, operatingSystemType!);
                var platformSpecificMembers = new ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?>();

                context.RegisterOperationBlockStartAction(context => AnalyzeOperationBlock(context, guardMethods, osPlatformType, platformSpecificMembers));
            });

            static ImmutableArray<IMethodSymbol> GetRuntimePlatformGuardMethods(IMethodSymbol runtimeIsOSPlatformMethod, INamedTypeSymbol operatingSystemType)
            {
                return operatingSystemType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    s_platformCheckMethodNames.Contains(m.Name) &&
                    m.IsStatic &&
                    m.ReturnType.SpecialType == SpecialType.System_Boolean).ToImmutableArray().
                    Add(runtimeIsOSPlatformMethod);
            }
        }

        private void AnalyzeOperationBlock(
            OperationBlockStartAnalysisContext context,
            ImmutableArray<IMethodSymbol> guardMethods,
            INamedTypeSymbol osPlatformType,
            ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers)
        {
            var platformSpecificOperations = PooledConcurrentDictionary<IOperation, SmallDictionary<string, PlatformAttributes>>.GetInstance();

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

                    if (guardMethods.IsEmpty || !(context.OperationBlocks.GetControlFlowGraph(out var topmostBlock) is { } cfg))
                    {
                        ReportDiagnosticsForAll(platformSpecificOperations, context);
                        return;
                    }

                    var performValueContentAnalysis = ComputeNeedsValueContentAnalysis(topmostBlock!, guardMethods);
                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                    var analysisResult = GlobalFlowStateAnalysis.TryGetOrComputeResult(
                        cfg, context.OwningSymbol, CreateOperationVisitor, wellKnownTypeProvider,
                        context.Options, RequiredOsRule, performValueContentAnalysis,
                        context.CancellationToken, out var valueContentAnalysisResult);

                    if (analysisResult == null)
                    {
                        return;
                    }

                    foreach (var (platformSpecificOperation, attributes) in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Kind, platformSpecificOperation.Syntax];

                        if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Unknown)
                        {
                            if (platformSpecificOperation.TryGetContainingLocalOrLambdaFunctionSymbol(out var containingSymbol))
                            {
                                var localResults = analysisResult.TryGetInterproceduralResultByDefinition(containingSymbol);
                                if (localResults != null)
                                {
                                    var hasKnownUnguardedValue = false;
                                    foreach (var localResult in localResults)
                                    {
                                        var localValue = localResult[platformSpecificOperation.Kind, platformSpecificOperation.Syntax];
                                        if (localValue.Kind == GlobalFlowStateAnalysisValueSetKind.Known && IsKnownValueGuarded(attributes, localValue))
                                        {
                                            hasKnownUnguardedValue = true;
                                            break;
                                        }
                                    }

                                    if (hasKnownUnguardedValue)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        else if (value.Kind == GlobalFlowStateAnalysisValueSetKind.Known && IsKnownValueGuarded(attributes, value))
                        {
                            continue;
                        }

                        ReportDiagnostics(platformSpecificOperation, attributes, context);
                    }
                }
                finally
                {
                    platformSpecificOperations.Free();
                    platformSpecificMembers.Free();
                }

                return;

                OperationVisitor CreateOperationVisitor(GlobalFlowStateAnalysisContext context) => new OperationVisitor(guardMethods, osPlatformType, context);
            });
        }

        private static bool ComputeNeedsValueContentAnalysis(IBlockOperation operationBlock, ImmutableArray<IMethodSymbol> guardMethods)
        {
            foreach (var operation in operationBlock.Descendants())
            {
                if (operation is IInvocationOperation invocation &&
                    guardMethods.Contains(invocation.TargetMethod))
                {
                    // Check if any integral parameter to guard method invocation has non-constant value.
                    foreach (var argument in invocation.Arguments)
                    {
                        if (argument.Parameter.Type.SpecialType == SpecialType.System_Int32 &&
                            !argument.Value.ConstantValue.HasValue)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsKnownValueGuarded(SmallDictionary<string, PlatformAttributes> attributes, GlobalFlowStateAnalysisValueSet value)
        {
            using var capturedPlatforms = PooledSortedSet<string>.GetInstance(StringComparer.OrdinalIgnoreCase);
            using var capturedVersions = PooledDictionary<string, Version>.GetInstance(StringComparer.OrdinalIgnoreCase);
            return IsKnownValueGuarded(attributes, value, capturedPlatforms, capturedVersions);

            static bool IsKnownValueGuarded(
                SmallDictionary<string, PlatformAttributes> attributes,
                GlobalFlowStateAnalysisValueSet value,
                PooledSortedSet<string> capturedPlatforms,
                PooledDictionary<string, Version> capturedVersions)
            {
                // 'GlobalFlowStateAnalysisValueSet.AnalysisValues' represent the && of values.
                foreach (var analysisValue in value.AnalysisValues)
                {
                    if (analysisValue is RuntimeMethodValue info && attributes.TryGetValue(info.PlatformName, out var attribute))
                    {
                        if (info.Negated)
                        {
                            if (attribute.UnsupportedFirst != null)
                            {
                                if (capturedPlatforms.Contains(info.PlatformName))
                                {
                                    if (attribute.UnsupportedFirst >= info.Version)
                                    {
                                        attribute.UnsupportedFirst = null;
                                    }
                                }
                                else if (IsEmptyVersion(attribute.UnsupportedFirst) && IsEmptyVersion(info.Version))
                                {
                                    attribute.UnsupportedFirst = null;
                                }
                            }

                            if (attribute.Obsoleted != null && capturedPlatforms.Contains(info.PlatformName) && attribute.Obsoleted <= info.Version)
                            {
                                attribute.Obsoleted = null;
                            }

                            if (attribute.UnsupportedSecond != null)
                            {
                                if (capturedPlatforms.Contains(info.PlatformName))
                                {
                                    if (attribute.UnsupportedSecond <= info.Version)
                                    {
                                        attribute.UnsupportedSecond = null;
                                    }
                                }
                                else if (IsEmptyVersion(attribute.UnsupportedSecond) && IsEmptyVersion(info.Version))
                                {
                                    attribute.UnsupportedSecond = null;
                                }
                            }

                            if (!IsEmptyVersion(info.Version))
                            {
                                capturedVersions[info.PlatformName] = info.Version;
                            }
                        }
                        else
                        {
                            capturedPlatforms.Add(info.PlatformName);

                            if (capturedVersions.Any())
                            {
                                if (attribute.UnsupportedFirst != null && capturedVersions.TryGetValue(info.PlatformName, out var version) && attribute.UnsupportedFirst >= version)
                                {
                                    attribute.UnsupportedFirst = null;
                                }

                                if (attribute.Obsoleted != null && capturedVersions.TryGetValue(info.PlatformName, out version) && attribute.Obsoleted <= version)
                                {
                                    attribute.Obsoleted = null;
                                }

                                if (attribute.UnsupportedSecond != null && capturedVersions.TryGetValue(info.PlatformName, out version) && attribute.UnsupportedSecond <= version)
                                {
                                    attribute.UnsupportedSecond = null;
                                }
                            }

                            if (attribute.SupportedFirst != null && attribute.SupportedFirst <= info.Version)
                            {
                                attribute.SupportedFirst = null;
                            }

                            if (attribute.SupportedSecond != null && attribute.SupportedSecond <= info.Version)
                            {
                                attribute.SupportedSecond = null;
                            }
                        }
                    }
                }

                if (value.Parents.IsEmpty)
                {
                    foreach (var attribute in attributes)
                    {
                        // if any of the attributes is not suppressed
                        if (attribute.Value.HasAttribute())
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // 'GlobalFlowStateAnalysisValueSet.Parents' represent || of values on different flow paths.
                    // We are guarded only if values are guarded on *all flow paths**.
                    foreach (var parent in value.Parents)
                    {
                        // NOTE: IsKnownValueGuarded mutates the input values, so we pass in cloned values
                        // to ensure that evaluation of each part of || is independent of evaluation of other parts.
                        var parentAttributes = CopyOperationAttributes(attributes);
                        using var parentCapturedPlatforms = PooledSortedSet<string>.GetInstance(capturedPlatforms);
                        using var parentCapturedVersions = PooledDictionary<string, Version>.GetInstance(capturedVersions);

                        if (!IsKnownValueGuarded(parentAttributes, parent, parentCapturedPlatforms, parentCapturedVersions))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        private static bool IsEmptyVersion(Version version) => version.Major == 0 && version.Minor == 0;

        private static void ReportDiagnosticsForAll(PooledConcurrentDictionary<IOperation,
            SmallDictionary<string, PlatformAttributes>> platformSpecificOperations, OperationBlockAnalysisContext context)
        {
            foreach (var platformSpecificOperation in platformSpecificOperations)
            {
                ReportDiagnostics(platformSpecificOperation.Key, platformSpecificOperation.Value, context);
            }
        }

        private static void ReportDiagnostics(IOperation operation, SmallDictionary<string, PlatformAttributes> attributes, OperationBlockAnalysisContext context)
        {
            var symbol = operation is IObjectCreationOperation creation ? creation.Constructor.ContainingType : GetOperationSymbol(operation);

            if (symbol == null)
            {
                return;
            }

            var operationName = symbol.Name;

            foreach (var platformName in attributes.Keys)
            {
                var attribute = attributes[platformName];

                if (attribute.SupportedSecond != null)
                {
                    ReportSupportedDiagnostic(operation, context, operationName, platformName, VersionToString(attribute.SupportedSecond));
                }
                else if (attribute.SupportedFirst != null)
                {
                    ReportSupportedDiagnostic(operation, context, operationName, platformName, VersionToString(attribute.SupportedFirst));
                }

                if (attribute.UnsupportedFirst != null)
                {
                    ReportUnsupportedDiagnostic(operation, context, operationName, platformName, VersionToString(attribute.UnsupportedFirst));
                }
                else if (attribute.UnsupportedSecond != null)
                {
                    ReportUnsupportedDiagnostic(operation, context, operationName, platformName, VersionToString(attribute.UnsupportedSecond));
                }

                if (attribute.Obsoleted != null)
                {
                    context.ReportDiagnostic(operation.CreateDiagnostic(ObsoleteOsRule, operationName, platformName, attribute.Obsoleted)); ;
                }
            }
        }

        private static void ReportSupportedDiagnostic(IOperation operation, OperationBlockAnalysisContext context, string name, string platformName, string? version = null) =>
            context.ReportDiagnostic(version == null ? operation.CreateDiagnostic(RequiredOsRule, name, platformName) :
                operation.CreateDiagnostic(RequiredOsVersionRule, name, platformName, version));

        private static void ReportUnsupportedDiagnostic(IOperation operation, OperationBlockAnalysisContext context, string name, string platformName, string? version = null) =>
            context.ReportDiagnostic(version == null ? operation.CreateDiagnostic(UnsupportedOsRule, name, platformName) :
                operation.CreateDiagnostic(UnsupportedOsVersionRule, name, platformName, version));

        private static string? VersionToString(Version version) => IsEmptyVersion(version) ? null : version.ToString();

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
            ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers)
        {
            var symbol = GetOperationSymbol(operation);

            if (symbol == null)
            {
                return;
            }

            if (TryGetOrCreatePlatformAttributes(symbol, platformSpecificMembers, out var operationAttributes))
            {
                if (TryGetOrCreatePlatformAttributes(context.ContainingSymbol, platformSpecificMembers, out var callSiteAttributes))
                {
                    if (IsNotSuppressedByCallSite(operationAttributes, callSiteAttributes, out var notSuppressedAttributes))
                    {
                        platformSpecificOperations.TryAdd(operation, notSuppressedAttributes);
                    }
                }
                else
                {
                    platformSpecificOperations.TryAdd(operation, CopyOperationAttributes(operationAttributes));
                }
            }
        }

        private static SmallDictionary<string, PlatformAttributes> CopyOperationAttributes(SmallDictionary<string, PlatformAttributes> attributes)
        {
            var copy = new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);
            foreach (var attribute in attributes)
            {
                copy.Add(attribute.Key, CopyAllAttributes(new PlatformAttributes(), attribute.Value));
            }
            return copy;
        }

        /// <summary>
        /// The semantics of the platform specific attributes are :
        ///    - An API that doesn't have any of these attributes is considered supported by all platforms.
        ///    - If either [SupportedOSPlatform] or [UnsupportedOSPlatform] attributes are present, we group all attributes by OS platform identifier:
        ///        - Allow list.If the lowest version for each OS platform is a [SupportedOSPlatform] attribute, the API is considered to only be supported by the listed platforms and unsupported by all other platforms.
        ///        - Deny list. If the lowest version for each OS platform is a [UnsupportedOSPlatform] attribute, then the API is considered to only be unsupported by the listed platforms and supported by all other platforms.
        ///        - Inconsistent list. If for some platforms the lowest version attribute is [SupportedOSPlatform] while for others it is [UnsupportedOSPlatform], the analyzer will produce a warning on the API definition because the API is attributed inconsistently.
        ///    - Both attributes can be instantiated without version numbers. This means the version number is assumed to be 0.0. This simplifies guard clauses, see examples below for more details.
        ///    - [ObsoletedInOSPlatform] continuous to require a version number.
        ///    - [ObsoletedInOSPlatform] by itself doesn't imply support. However, it doesn't make sense to apply [ObsoletedInOSPlatform] unless that platform is supported.
        /// </summary>
        /// <param name="operationAttributes">Platform specific attributes applied to the invoked member</param>
        /// <param name="callSiteAttributes">Platform specific attributes applied to the call site where the member invoked</param>
        /// <returns>true if all attributes applied to the operation is suppressed, false otherwise</returns>

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
                    // If only supported for current platform
                    if (attribute.UnsupportedFirst == null || attribute.UnsupportedFirst > attribute.SupportedFirst)
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
                            var attributeToCheck = attribute.SupportedSecond ?? attribute.SupportedFirst;
                            if (!MandatoryOsVersionsSuppressed(callSiteAttribute, attributeToCheck))
                            {
                                diagnositcAttribute.SupportedSecond = (Version)attributeToCheck.Clone();
                            }

                            if (attribute.UnsupportedFirst != null &&
                                !(SuppressedByCallSiteSupported(attribute, callSiteAttribute.SupportedFirst) ||
                                  SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst)))
                            {
                                diagnositcAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                            }

                            if (attribute.Obsoleted != null)
                            {
                                if (attribute.SupportedSecond != null && attribute.SupportedSecond > attribute.Obsoleted || attribute.SupportedFirst > attribute.Obsoleted)
                                {
                                    // Can supported version be greater than obsoleted? Do we want to report diagnostic here for wrong version?
                                }
                                else if (!SuppresedByUnsupported(callSiteAttribute, attribute.Obsoleted) && !ObsoletedSuppressed(callSiteAttribute.Obsoleted, attribute.Obsoleted))
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
                            }

                            if (attribute.UnsupportedSecond != null && !UnsupportedSecondSuppressed(attribute, callSiteAttribute))
                            {
                                diagnositcAttribute.UnsupportedSecond = (Version)attribute.UnsupportedSecond.Clone();
                            }

                            if (attribute.Obsoleted != null)
                            {
                                if (attribute.SupportedSecond != null && attribute.SupportedSecond > attribute.Obsoleted || attribute.SupportedFirst > attribute.Obsoleted)
                                {
                                    // Can supported version be greater than obsoleted? Do we want to report diagnostic here for wrong version?
                                }
                                else if (!SuppresedByUnsupported(callSiteAttribute, attribute.Obsoleted) && !ObsoletedSuppressed(callSiteAttribute.Obsoleted, attribute.Obsoleted))
                                {
                                    diagnositcAttribute.Obsoleted = (Version)attribute.Obsoleted.Clone();
                                }
                            }
                        }
                        // else call site is not supporting this platform, and it is deny list, so no need to warn
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
                        // else call site is not supporting this platform, and it is deny list, so no need to warn
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

        private static bool SuppresedByUnsupported(PlatformAttributes callSiteAttribute, Version obsoleted) =>
             callSiteAttribute.UnsupportedFirst != null && callSiteAttribute.UnsupportedFirst <= obsoleted ||
             callSiteAttribute.UnsupportedSecond != null && callSiteAttribute.UnsupportedSecond <= obsoleted;

        private static PlatformAttributes CopyAllAttributes(PlatformAttributes copyTo, PlatformAttributes copyFrom)
        {
            copyTo.SupportedFirst = (Version?)copyFrom.SupportedFirst?.Clone();
            copyTo.SupportedSecond = (Version?)copyFrom.SupportedSecond?.Clone();
            copyTo.UnsupportedFirst = (Version?)copyFrom.UnsupportedFirst?.Clone();
            copyTo.UnsupportedSecond = (Version?)copyFrom.UnsupportedSecond?.Clone();
            copyTo.Obsoleted = (Version?)copyFrom.Obsoleted?.Clone();
            return copyTo;
        }

        private static bool SuppressedByCallSiteUnsupported(PlatformAttributes callSiteAttribute, Version unsupporteAttribute) =>
            callSiteAttribute.UnsupportedFirst != null && unsupporteAttribute >= callSiteAttribute.UnsupportedFirst ||
            callSiteAttribute.UnsupportedSecond != null && unsupporteAttribute >= callSiteAttribute.UnsupportedSecond;

        private static bool ObsoletedSuppressed(Version? callSiteObsoleted, Version checkingObsoleted) =>
            callSiteObsoleted != null && checkingObsoleted >= callSiteObsoleted;

        private static bool UnsupportedSecondSuppressed(PlatformAttributes attribute, PlatformAttributes callSiteAttribute) =>
            SuppressedByCallSiteSupported(attribute, callSiteAttribute.SupportedFirst) ||
            SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedSecond!);

        private static bool SuppressedByCallSiteSupported(PlatformAttributes attribute, Version? callSiteSupportedFirst) =>
            callSiteSupportedFirst != null && callSiteSupportedFirst >= attribute.SupportedFirst! &&
            attribute.SupportedSecond != null && callSiteSupportedFirst >= attribute.SupportedSecond;

        private static bool UnsupportedFirstSuppressed(PlatformAttributes attribute, PlatformAttributes callSiteAttribute) =>
            callSiteAttribute.SupportedFirst != null && callSiteAttribute.SupportedFirst >= attribute.SupportedFirst ||
            SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst!);

        // As optianal if call site supports that platform, their versions should match
        private static bool OptionalOsVersionsSuppressed(PlatformAttributes callSiteAttribute, PlatformAttributes attribute) =>
            (callSiteAttribute.SupportedFirst == null || attribute.SupportedFirst <= callSiteAttribute.SupportedFirst) &&
            (callSiteAttribute.SupportedSecond == null || attribute.SupportedFirst <= callSiteAttribute.SupportedSecond);

        private static bool MandatoryOsVersionsSuppressed(PlatformAttributes callSitePlatforms, Version checkingVersion) =>
            callSitePlatforms.SupportedFirst != null && checkingVersion <= callSitePlatforms.SupportedFirst ||
            callSitePlatforms.SupportedSecond != null && checkingVersion <= callSitePlatforms.SupportedSecond;

        // Do not warn if platform specific enum/field value is used in conditional check, like: 'if (value == FooEnum.WindowsOnlyValue)'
        private static bool IsWithinConditionalOperation(IFieldReferenceOperation pOperation) =>
            pOperation.ConstantValue.HasValue &&
            pOperation.Parent is IBinaryOperation bo &&
            (bo.OperatorKind == BinaryOperatorKind.Equals ||
            bo.OperatorKind == BinaryOperatorKind.NotEquals ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThan ||
            bo.OperatorKind == BinaryOperatorKind.LessThan ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual ||
            bo.OperatorKind == BinaryOperatorKind.LessThanOrEqual);

        private static bool TryGetOrCreatePlatformAttributes(
            ISymbol symbol,
            ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers,
            [NotNullWhen(true)] out SmallDictionary<string, PlatformAttributes>? attributes)
        {
            if (!platformSpecificMembers.TryGetValue(symbol, out attributes))
            {
                var container = symbol.ContainingSymbol;

                // Namespaces do not have attributes
                while (container is INamespaceSymbol)
                {
                    container = container.ContainingSymbol;
                }

                if (container != null &&
                    TryGetOrCreatePlatformAttributes(container, platformSpecificMembers, out var containerAttributes))
                {
                    attributes = CopyOperationAttributes(containerAttributes);
                }

                AddPlatformAttributes(symbol.GetAttributes(), ref attributes);

                attributes = platformSpecificMembers.GetOrAdd(symbol, attributes);
            }

            return attributes != null;

            static bool AddPlatformAttributes(ImmutableArray<AttributeData> immediateAttributes, [NotNullWhen(true)] ref SmallDictionary<string, PlatformAttributes>? attributes)
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
        }

        private static bool TryAddValidAttribute([NotNullWhen(true)] ref SmallDictionary<string, PlatformAttributes>? attributes, AttributeData attribute)
        {
            if (!attribute.ConstructorArguments.IsEmpty &&
                                attribute.ConstructorArguments[0] is { } argument &&
                                argument.Kind == TypedConstantKind.Primitive &&
                                argument.Type.SpecialType == SpecialType.System_String &&
                                !argument.IsNull &&
                                !argument.Value.Equals(string.Empty) &&
                                TryParsePlatformNameAndVersion(argument.Value.ToString(), out string platformName, out Version? version))
            {
                attributes ??= new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);

                if (!attributes.TryGetValue(platformName, out var existingAttributes))
                {
                    existingAttributes = new PlatformAttributes();
                    attributes[platformName] = existingAttributes;
                }

                AddAttribute(attribute.AttributeClass.Name, version, existingAttributes);
                return true;
            }

            return false;
        }

        private static void AddAttribute(string name, Version version, PlatformAttributes existingAttributes)
        {
            switch (name)
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
                    if (attributes.UnsupportedSecond != null)
                    {
                        if (attributes.UnsupportedSecond > attributes.UnsupportedFirst)
                        {
                            attributes.UnsupportedSecond = attributes.UnsupportedFirst;
                        }
                    }
                    else
                    {
                        attributes.UnsupportedSecond = attributes.UnsupportedFirst;
                    }

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
                    if (attributes.SupportedSecond != null)
                    {
                        if (attributes.SupportedSecond < attributes.SupportedFirst)
                        {
                            attributes.SupportedSecond = attributes.SupportedFirst;
                        }
                    }
                    else
                    {
                        attributes.SupportedSecond = attributes.SupportedFirst;
                    }

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
    }
}