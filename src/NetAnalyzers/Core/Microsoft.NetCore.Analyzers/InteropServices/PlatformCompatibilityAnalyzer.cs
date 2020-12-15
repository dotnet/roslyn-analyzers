﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    /// <summary>
    /// CA1416: Analyzer that informs developers when they use platform-specific APIs from call sites where the API might not be available
    /// 
    /// It finds usage of platform-specific or unsupported APIs and diagnoses if the 
    /// API is guarded by platform check or if it is annotated with corresponding platform specific attribute.
    /// If using the platform-specific API is not safe it reports diagnostics.
    ///
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed partial class PlatformCompatibilityAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1416";
        private static readonly ImmutableArray<string> s_osPlatformAttributes = ImmutableArray.Create(SupportedOSPlatformAttribute, UnsupportedOSPlatformAttribute);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableOnlySupportedCsAllPlatform = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityOnlySupportedCsAllPlatformMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableOnlySupporteCsReachable = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityOnlySupportedCsReachableMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableOnlySupporteCsUnreachable = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityOnlySupportedCsUnreachableMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizablSupporteCsAllPlatform = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilitySupportedCsAllPlatformMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizablSupporteCsReachable = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilitySupportedCsReachableMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableUnsupportedCsAllPlatform = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityUnsupportedCsAllPlatformMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableUnsupportedCsReachable = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityUnsupportedCsReachableMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        // We are adding the new attributes into older versions of .Net 5.0, so there could be multiple referenced assemblies each with their own 
        // version of internal attribute type which will cause ambiguity, to avoid that we are comparing the attributes by their name
        private const string SupportedOSPlatformAttribute = nameof(SupportedOSPlatformAttribute);
        private const string UnsupportedOSPlatformAttribute = nameof(UnsupportedOSPlatformAttribute);

        // Platform guard method name, prefix, suffix
        private const string IsOSPlatform = nameof(IsOSPlatform);
        private const string IsPrefix = "Is";
        private const string OptionalSuffix = "VersionAtLeast";
        private const string Net = "net";

        internal static DiagnosticDescriptor OnlySupportedCsReachable = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableOnlySupporteCsReachable,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor OnlySupportedCsUnreachable = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableOnlySupporteCsUnreachable,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor OnlySupportedCsAllPlatforms = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableOnlySupportedCsAllPlatform,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor SupportedCsAllPlatforms = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizablSupporteCsAllPlatform,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor SupportedCsReachable = DiagnosticDescriptorHelper.Create(RuleId,
                                                                              s_localizableTitle,
                                                                              s_localizablSupporteCsReachable,
                                                                              DiagnosticCategory.Interoperability,
                                                                              RuleLevel.BuildWarning,
                                                                              description: s_localizableDescription,
                                                                              isPortedFxCopRule: false,
                                                                              isDataflowRule: false);

        internal static DiagnosticDescriptor UnsupportedCsAllPlatforms = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableUnsupportedCsAllPlatform,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        internal static DiagnosticDescriptor UnsupportedCsReachable = DiagnosticDescriptorHelper.Create(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableUnsupportedCsReachable,
                                                                                      DiagnosticCategory.Interoperability,
                                                                                      RuleLevel.BuildWarning,
                                                                                      description: s_localizableDescription,
                                                                                      isPortedFxCopRule: false,
                                                                                      isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(OnlySupportedCsReachable, OnlySupportedCsUnreachable,
            OnlySupportedCsAllPlatforms, SupportedCsAllPlatforms, SupportedCsReachable, UnsupportedCsAllPlatforms, UnsupportedCsReachable);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(context =>
            {
                if (!PlatformAnalysisAllowed(context.Options, context.Compilation, context.CancellationToken))
                {
                    return;
                }

                var typeName = WellKnownTypeNames.SystemOperatingSystem;

                // TODO: remove 'typeName + "Helper"' after tests able to consume the real new APIs
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName + "Helper", out var operatingSystemType))
                {
                    if (!context.Compilation.TryGetOrCreateTypeByMetadataName(typeName, out operatingSystemType))
                    {
                        return;
                    }
                }
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesOSPlatform, out var osPlatformType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesRuntimeInformation, out var runtimeInformationType))
                {
                    return;
                }

                var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);
                if (stringType == null)
                {
                    return;
                }

                var msBuildPlatforms = GetSupportedPlatforms(context.Options, context.Compilation, context.CancellationToken);
                var runtimeIsOSPlatformMethod = runtimeInformationType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    IsOSPlatform == m.Name &&
                    m.IsStatic &&
                    m.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    m.Parameters.Length == 1 &&
                    m.Parameters[0].Type.Equals(osPlatformType)).FirstOrDefault();

                var guardMethods = GetOperatingSystemGuardMethods(runtimeIsOSPlatformMethod, operatingSystemType!);
                var platformSpecificMembers = new ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?>();
                var osPlatformTypeArray = ImmutableArray.Create(osPlatformType);
                var osPlatformCreateMethod = osPlatformType.GetMembers("Create").OfType<IMethodSymbol>().Where(m =>
                    m.IsStatic &&
                    m.ReturnType.Equals(osPlatformType) &&
                    m.Parameters.Length == 1 &&
                    m.Parameters[0].Type.SpecialType == SpecialType.System_String).FirstOrDefault();
                var notSupportedExceptionType = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemNotSupportedException);

                context.RegisterOperationBlockStartAction(
                    context => AnalyzeOperationBlock(context, guardMethods, runtimeIsOSPlatformMethod, osPlatformCreateMethod,
                                    osPlatformTypeArray, stringType, platformSpecificMembers, msBuildPlatforms, notSupportedExceptionType));
            });

            static ImmutableArray<IMethodSymbol> GetOperatingSystemGuardMethods(IMethodSymbol? runtimeIsOSPlatformMethod, INamedTypeSymbol operatingSystemType)
            {
                var methods = operatingSystemType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                    m.IsStatic &&
                    m.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    (IsOSPlatform == m.Name) || NameAndParametersValid(m)).
                    ToImmutableArray();

                if (runtimeIsOSPlatformMethod != null)
                {
                    return methods.Add(runtimeIsOSPlatformMethod);
                }

                return methods;
            }

            static ImmutableArray<string> GetSupportedPlatforms(AnalyzerOptions options, Compilation compilation, CancellationToken cancellationToken) =>
                options.GetMSBuildItemMetadataValues(MSBuildItemOptionNames.SupportedPlatform, compilation, cancellationToken);

            static bool NameAndParametersValid(IMethodSymbol method) => method.Name.StartsWith(IsPrefix, StringComparison.Ordinal) &&
                    (method.Parameters.Length == 0 || method.Name.EndsWith(OptionalSuffix, StringComparison.Ordinal));
        }

        private static bool PlatformAnalysisAllowed(AnalyzerOptions options, Compilation compilation, CancellationToken token)
        {
            var tfmString = options.GetMSBuildPropertyValue(MSBuildPropertyOptionNames.TargetFramework, compilation, token);

            if (tfmString?.Length >= 4 &&
                tfmString.StartsWith(Net, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(tfmString[3].ToString(), out var major) &&
                major >= 5)
            {
                return true;
            }
            else
            {
                return LowerTargetsEnabled(options, compilation, token);
            }
        }

        private static bool LowerTargetsEnabled(AnalyzerOptions options, Compilation compilation, CancellationToken cancellationToken) =>
            compilation.SyntaxTrees.FirstOrDefault() is { } tree &&
            options.GetBoolOptionValue(EditorConfigOptionNames.EnablePlatformAnalyzerOnPreNet5Target, SupportedCsAllPlatforms, tree, compilation, false, cancellationToken);

        private void AnalyzeOperationBlock(
            OperationBlockStartAnalysisContext context,
            ImmutableArray<IMethodSymbol> guardMethods,
            IMethodSymbol? runtimeIsOSPlatformMethod,
            IMethodSymbol? osPlatformCreateMethod,
            ImmutableArray<INamedTypeSymbol> osPlatformTypeArray,
            INamedTypeSymbol stringType,
            ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers,
            ImmutableArray<string> msBuildPlatforms,
            ITypeSymbol? notSupportedExceptionType)
        {
            if (context.IsMethodNotImplementedOrSupported(checkPlatformNotSupported: true))
            {
                return;
            }

            var osPlatformType = osPlatformTypeArray[0];
            var platformSpecificOperations = PooledConcurrentDictionary<IOperation, (SmallDictionary<string, PlatformAttributes> attributes,
                SmallDictionary<string, PlatformAttributes>? csAttributes)>.GetInstance();

            context.RegisterOperationAction(context =>
            {
                AnalyzeOperation(context.Operation, context, platformSpecificOperations, platformSpecificMembers, msBuildPlatforms, notSupportedExceptionType);
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
                        ReportDiagnosticsForAll(platformSpecificOperations, context, platformSpecificMembers);
                        return;
                    }

                    var performValueContentAnalysis = ComputeNeedsValueContentAnalysis(cfg.OriginalOperation, guardMethods, runtimeIsOSPlatformMethod, osPlatformType);
                    var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                    var analysisResult = GlobalFlowStateAnalysis.TryGetOrComputeResult(
                        cfg, context.OwningSymbol, CreateOperationVisitor, wellKnownTypeProvider,
                        context.Options, SupportedCsAllPlatforms, performValueContentAnalysis,
                        pessimisticAnalysis: false,
                        context.CancellationToken, out var valueContentAnalysisResult,
                        additionalSupportedValueTypes: osPlatformTypeArray,
                        getValueContentValueForAdditionalSupportedValueTypeOperation: GetValueContentValue);

                    if (analysisResult == null)
                    {
                        return;
                    }

                    foreach (var (platformSpecificOperation, pair) in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Kind, platformSpecificOperation.Syntax];
                        var csAttributes = pair.csAttributes != null ? CopyAttributes(pair.csAttributes) : null;

                        if ((value.Kind == GlobalFlowStateAnalysisValueSetKind.Known && IsKnownValueGuarded(pair.attributes, ref csAttributes, value)) ||
                           (value.Kind == GlobalFlowStateAnalysisValueSetKind.Unknown && HasGuardedLambdaOrLocalFunctionResult(platformSpecificOperation,
                            pair.attributes, ref csAttributes, analysisResult)))
                        {
                            continue;
                        }

                        ReportDiagnostics(platformSpecificOperation, pair.attributes, csAttributes, context, platformSpecificMembers);
                    }
                }
                finally
                {
                    // Workaround for https://github.com/dotnet/roslyn/issues/46859
                    // Do not free in presence of cancellation.
                    if (!context.CancellationToken.IsCancellationRequested)
                    {
                        platformSpecificOperations.Free(context.CancellationToken);
                    }
                }

                return;

                OperationVisitor CreateOperationVisitor(GlobalFlowStateAnalysisContext context) => new OperationVisitor(guardMethods, osPlatformType, context);

                ValueContentAbstractValue GetValueContentValue(IOperation operation)
                {
                    Debug.Assert(operation.Type.Equals(osPlatformType));
                    if (operation is IInvocationOperation invocation &&
                        invocation.TargetMethod.Equals(osPlatformCreateMethod) &&
                        invocation.Arguments.Length == 1 &&
                        invocation.Arguments[0].Value is { } argument &&
                        argument.ConstantValue.HasValue &&
                        argument.ConstantValue.Value is string platformName &&
                        platformName.Length > 0)
                    {
                        return ValueContentAbstractValue.Create(platformName, stringType);
                    }

                    return ValueContentAbstractValue.MayBeContainsNonLiteralState;
                }
            });
        }

        private static bool HasGuardedLambdaOrLocalFunctionResult(IOperation platformSpecificOperation, SmallDictionary<string, PlatformAttributes> attributes,
            ref SmallDictionary<string, PlatformAttributes>? csAttributes, DataFlowAnalysisResult<GlobalFlowStateBlockAnalysisResult, GlobalFlowStateAnalysisValueSet> analysisResult)
        {
            if (!platformSpecificOperation.IsWithinLambdaOrLocalFunction(out var containingLambdaOrLocalFunctionOperation))
            {
                return false;
            }

            var results = analysisResult.TryGetLambdaOrLocalFunctionResults(containingLambdaOrLocalFunctionOperation);
            Debug.Assert(results.Any(), "Expected at least one analysis result for lambda/local function");

            foreach (var localResult in results)
            {
                Debug.Assert(localResult.ControlFlowGraph.OriginalOperation == containingLambdaOrLocalFunctionOperation);

                var localValue = localResult[platformSpecificOperation.Kind, platformSpecificOperation.Syntax];

                // Value must be known and guarded in all analysis contexts.
                // NOTE: IsKnownValueGuarded mutates the input values, so we pass in cloned values
                // to ensure that evaluation of each result is independent of evaluation of other parts.
                if (localValue.Kind != GlobalFlowStateAnalysisValueSetKind.Known ||
                    !IsKnownValueGuarded(CopyAttributes(attributes), ref csAttributes, localValue))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ComputeNeedsValueContentAnalysis(IOperation operationBlock, ImmutableArray<IMethodSymbol> guardMethods, IMethodSymbol? runtimeIsOSPlatformMethod, INamedTypeSymbol osPlatformType)
        {
            Debug.Assert(runtimeIsOSPlatformMethod == null || guardMethods.Contains(runtimeIsOSPlatformMethod));

            foreach (var operation in operationBlock.Descendants())
            {
                if (operation is IInvocationOperation invocation)
                {
                    if (invocation.TargetMethod.Equals(runtimeIsOSPlatformMethod))
                    {
                        if (invocation.Arguments.Length == 1 &&
                            invocation.Arguments[0].Value is IPropertyReferenceOperation propertyReference &&
                            propertyReference.Property.ContainingType.Equals(osPlatformType))
                        {
                            // "OSPlatform.Platform" property reference does not need value content analysis.
                            continue;
                        }

                        return true;
                    }
                    else if (guardMethods.Contains(invocation.TargetMethod))
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
            }

            return false;
        }

        private static bool IsKnownValueGuarded(SmallDictionary<string, PlatformAttributes> attributes,
                ref SmallDictionary<string, PlatformAttributes>? csAttributes, GlobalFlowStateAnalysisValueSet value)
        {
            using var capturedVersions = PooledDictionary<string, Version>.GetInstance(StringComparer.OrdinalIgnoreCase);
            return IsKnownValueGuarded(attributes, ref csAttributes, value, capturedVersions);

            static bool IsKnownValueGuarded(
                SmallDictionary<string, PlatformAttributes> attributes,
                ref SmallDictionary<string, PlatformAttributes>? csAttributes,
                GlobalFlowStateAnalysisValueSet value,
                PooledDictionary<string, Version> capturedVersions)
            {
                // 'GlobalFlowStateAnalysisValueSet.AnalysisValues' represent the && of values.
                foreach (var analysisValue in value.AnalysisValues)
                {
                    if (analysisValue is PlatformMethodValue info)
                    {
                        if (attributes.TryGetValue(info.PlatformName, out var attribute))
                        {
                            if (info.Negated)
                            {
                                if (attribute.UnsupportedFirst != null)
                                {
                                    if (attribute.UnsupportedFirst >= info.Version)
                                    {
                                        if (DenyList(attribute))
                                        {
                                            attribute.SupportedFirst = null;
                                            attribute.SupportedSecond = null;
                                            attribute.UnsupportedSecond = null;
                                        }
                                        attribute.UnsupportedFirst = null;
                                    }
                                }

                                if (attribute.UnsupportedSecond != null)
                                {
                                    if (attribute.UnsupportedSecond <= info.Version)
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
                                if (capturedVersions.Any())
                                {
                                    if (attribute.UnsupportedFirst != null &&
                                        capturedVersions.TryGetValue(info.PlatformName, out var version) &&
                                        attribute.UnsupportedFirst >= version)
                                    {
                                        attribute.UnsupportedFirst = null;
                                    }

                                    if (attribute.UnsupportedSecond != null &&
                                        capturedVersions.TryGetValue(info.PlatformName, out version) &&
                                        attribute.UnsupportedSecond <= version)
                                    {
                                        attribute.UnsupportedSecond = null;
                                    }
                                }

                                if (attribute.SupportedFirst != null &&
                                    attribute.SupportedFirst <= info.Version)
                                {
                                    attribute.SupportedFirst = null;
                                    RemoveUnsupportedWithLessVersion(info.Version, attribute);
                                    RemoveOtherSupportsOnDifferentPlatforms(attributes, info.PlatformName);
                                }

                                if (attribute.SupportedSecond != null &&
                                    attribute.SupportedSecond <= info.Version)
                                {
                                    attribute.SupportedSecond = null;
                                    RemoveUnsupportedWithLessVersion(info.Version, attribute);
                                    RemoveOtherSupportsOnDifferentPlatforms(attributes, info.PlatformName);
                                }

                                csAttributes = SetAsCallSiteSupportedAttribute(csAttributes, info);
                                RemoveUnsupportsOnDifferentPlatforms(attributes, info.PlatformName);
                            }
                        }
                        else
                        {
                            if (!info.Negated)
                            {
                                // it is checking one exact platform, other unsupported should be suppressed
                                RemoveUnsupportsOnDifferentPlatforms(attributes, info.PlatformName);
                                csAttributes = SetAsCallSiteSupportedAttribute(csAttributes, info);
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
                        var parentAttributes = CopyAttributes(attributes);
                        using var parentCapturedVersions = PooledDictionary<string, Version>.GetInstance(capturedVersions);

                        if (!IsKnownValueGuarded(parentAttributes, ref csAttributes, parent, parentCapturedVersions))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            static SmallDictionary<string, PlatformAttributes> SetAsCallSiteSupportedAttribute(SmallDictionary<string, PlatformAttributes>? csAttributes, PlatformMethodValue info)
            {
                if (csAttributes == null)
                {
                    csAttributes = new SmallDictionary<string, PlatformAttributes>();
                }
                if (csAttributes.TryGetValue(info.PlatformName, out var attributes))
                {
                    if (attributes.SupportedFirst == null)
                    {
                        attributes.SupportedFirst = info.Version;
                    }
                    else
                    {
                        attributes.SupportedSecond = info.Version;
                    }
                }
                else
                {
                    csAttributes.Add(info.PlatformName, new PlatformAttributes() { SupportedFirst = info.Version });
                }
                return csAttributes;
            }

            static void RemoveUnsupportsOnDifferentPlatforms(SmallDictionary<string, PlatformAttributes> attributes, string platformName)
            {
                foreach (var (name, attribute) in attributes)
                {
                    if (!name.Equals(platformName, StringComparison.OrdinalIgnoreCase) &&
                        DenyList(attribute))
                    {
                        attribute.UnsupportedFirst = null;
                        attribute.UnsupportedSecond = null;
                        attribute.SupportedFirst = null;
                        attribute.SupportedSecond = null;
                    }
                }
            }

            static void RemoveUnsupportedWithLessVersion(Version supportedVersion, PlatformAttributes attribute)
            {
                if (attribute.UnsupportedFirst != null &&
                    attribute.UnsupportedFirst <= supportedVersion)
                {
                    attribute.UnsupportedFirst = null;
                }
            }

            static void RemoveOtherSupportsOnDifferentPlatforms(SmallDictionary<string, PlatformAttributes> attributes, string platformName)
            {
                foreach (var (name, attribute) in attributes)
                {
                    if (!name.Equals(platformName, StringComparison.OrdinalIgnoreCase))
                    {
                        attribute.SupportedFirst = null;
                        attribute.SupportedSecond = null;
                    }
                }
            }
        }

        private static bool IsEmptyVersion(Version version) => version.Major == 0 && version.Minor == 0;

        private static void ReportDiagnosticsForAll(PooledConcurrentDictionary<IOperation,
            (SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? csAttributes)> platformSpecificOperations,
            OperationBlockAnalysisContext context, ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers)
        {
            foreach (var operation in platformSpecificOperations)
            {
                ReportDiagnostics(operation.Key, operation.Value.attributes, operation.Value.csAttributes, context, platformSpecificMembers);
            }
        }

        private static void ReportDiagnostics(IOperation operation, SmallDictionary<string, PlatformAttributes> attributes,
            SmallDictionary<string, PlatformAttributes>? csAttributes, OperationBlockAnalysisContext context,
            ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers)
        {
            var symbol = GetOperationSymbol(operation);

            if (symbol == null)
            {
                return;
            }

            if (symbol is IPropertySymbol property)
            {
                symbol = GetAccessorMethod(platformSpecificMembers, symbol, GetPropertyAccessors(property, operation));
            }

            if (symbol is IEventSymbol iEvent)
            {
                var accessor = GetEventAccessor(iEvent, operation);
                if (accessor != null)
                {
                    symbol = accessor;
                }
            }

            var originalAttributes = platformSpecificMembers[symbol] ?? attributes;

            if (operation is IObjectCreationOperation creation)
            {
                symbol = creation.Constructor.ContainingType;
            }

            var operationName = symbol.ToDisplayString(GetLanguageSpecificFormat(operation));

            foreach (var attribute in originalAttributes.Values)
            {
                if (AllowList(attribute))
                {
                    ReportSupportedDiagnostic(operation, context, operationName, attributes, csAttributes);
                }
                else
                {
                    ReportUnsupportedDiagnostic(operation, context, operationName, attributes, csAttributes);
                }
                break;
            }

            static void ReportSupportedDiagnostic(IOperation operation, OperationBlockAnalysisContext context, string operationName,
                 SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? callsiteAttributes)
            {
                var supportedRule = GetSupportedPlatforms(attributes, callsiteAttributes, out var platformNames);
                var platforms = string.Join(MicrosoftNetCoreAnalyzersResources.CommaSeparator, platformNames);
                var callSitePlatforms = string.Join(MicrosoftNetCoreAnalyzersResources.CommaSeparator,
                    GetCallsitePlatforms(attributes, callsiteAttributes, out var callsite, supported: supportedRule));
                var rule = supportedRule ? SwitchSupportedRule(callsite) : SwitchRule(callsite, true);

                context.ReportDiagnostic(operation.CreateDiagnostic(rule, operationName, platforms, callSitePlatforms));

                static DiagnosticDescriptor SwitchSupportedRule(Callsite callsite)
                    => callsite switch
                    {
                        Callsite.AllPlatforms => OnlySupportedCsAllPlatforms,
                        Callsite.Reachable => OnlySupportedCsReachable,
                        Callsite.Unreachable => OnlySupportedCsUnreachable,
                        _ => throw new NotImplementedException()
                    };

                static bool GetSupportedPlatforms(SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? csAttributes, out List<string> platformNames)
                {
                    var supportedRule = true;
                    platformNames = new List<string>();
                    foreach (var (pName, pAttribute) in attributes)
                    {
                        if (pAttribute.SupportedFirst != null)
                        {
                            var supportedVersion = pAttribute.SupportedSecond ?? pAttribute.SupportedFirst;
                            if (pAttribute.UnsupportedFirst != null && !IsEmptyVersion(pAttribute.UnsupportedFirst))
                            {
                                if (IsEmptyVersion(supportedVersion))
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndBefore,
                                        pName, pAttribute.UnsupportedFirst));
                                }
                                else
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityFromVersionToVersion,
                                        pName, supportedVersion, pAttribute.UnsupportedFirst));
                                }
                            }
                            else if (IsEmptyVersion(supportedVersion))
                            {
                                if (csAttributes != null && HasSameVersionedPlatformSupport(csAttributes, pName, checkSupport: false))
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityAllVersions, pName));
                                    continue;
                                }
                                platformNames.Add($"'{pName}'");
                            }
                            else
                            {
                                platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                    MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater, pName, supportedVersion));
                            }
                        }
                        else if (pAttribute.UnsupportedFirst != null)
                        {
                            if (IsEmptyVersion(pAttribute.UnsupportedFirst))
                            {
                                if (csAttributes != null && HasSameVersionedPlatformSupport(csAttributes, pName, checkSupport: true))
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityAllVersions, pName));
                                    continue;
                                }
                                platformNames.Add($"'{pName}'");
                            }
                            else
                            {
                                platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater,
                                    pName, pAttribute.UnsupportedFirst));
                            }
                            supportedRule = false;
                        }
                    }
                    return supportedRule;
                }
            }

            static DiagnosticDescriptor SwitchRule(Callsite callsite, bool unsupported)
            {
                if (unsupported)
                {
                    return callsite switch
                    {
                        Callsite.AllPlatforms => UnsupportedCsAllPlatforms,
                        Callsite.Reachable => UnsupportedCsReachable,
                        _ => throw new NotImplementedException()
                    };
                }
                else
                {
                    return callsite switch
                    {
                        Callsite.AllPlatforms => SupportedCsAllPlatforms,
                        Callsite.Reachable => SupportedCsReachable,
                        _ => throw new NotImplementedException()
                    };
                }
            }

            static void ReportUnsupportedDiagnostic(IOperation operation, OperationBlockAnalysisContext context, string operationName,
                SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? callsiteAttributes)
            {
                var unsupportedRule = GetPlatformNames(attributes, callsiteAttributes, out var platformNames);
                var platforms = string.Join(MicrosoftNetCoreAnalyzersResources.CommaSeparator, platformNames);
                var callSitePlatforms = string.Join(MicrosoftNetCoreAnalyzersResources.CommaSeparator,
                    GetCallsitePlatforms(attributes, callsiteAttributes, out var callsite, supported: !unsupportedRule));
                context.ReportDiagnostic(operation.CreateDiagnostic(SwitchRule(callsite, unsupportedRule), operationName, platforms, callSitePlatforms));

                static bool GetPlatformNames(SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? csAttributes, out List<string> platformNames)
                {
                    platformNames = new List<string>();
                    var unsupportedRule = true;
                    foreach (var (pName, pAttribute) in attributes)
                    {
                        var unsupportedVersion = pAttribute.UnsupportedSecond ?? pAttribute.UnsupportedFirst;
                        var supportedVersion = pAttribute.SupportedSecond ?? pAttribute.SupportedFirst;

                        if (unsupportedVersion != null)
                        {
                            if (supportedVersion != null)
                            {
                                if (supportedVersion > unsupportedVersion)
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater,
                                        pName, supportedVersion));
                                }
                                else
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityFromVersionToVersion,
                                        pName, supportedVersion, unsupportedVersion));
                                }
                                unsupportedRule = false;
                            }
                            else
                            {
                                if (IsEmptyVersion(unsupportedVersion))
                                {
                                    if (csAttributes != null && HasSameVersionedPlatformSupport(csAttributes, pName, checkSupport: true))
                                    {
                                        platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityAllVersions, pName));
                                        continue;
                                    }
                                    platformNames.Add($"'{pName}'");
                                }
                                else
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                        MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater, pName, unsupportedVersion));
                                }
                            }
                        }
                        else if (supportedVersion != null)
                        {
                            platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater, pName, supportedVersion));
                            unsupportedRule = false;
                        }
                    }
                    return unsupportedRule;
                }
            }

            static List<string> GetCallsitePlatforms(SmallDictionary<string, PlatformAttributes> attributes,
                SmallDictionary<string, PlatformAttributes>? callsiteAttributes, out Callsite callsite, bool supported)
            {
                callsite = Callsite.AllPlatforms;
                var platformNames = new List<string>();
                if (callsiteAttributes != null)
                {
                    foreach (var (pName, csAttribute) in callsiteAttributes)
                    {
                        var supportedVersion = csAttribute.SupportedSecond ?? csAttribute.SupportedFirst;
                        if (supportedVersion != null)
                        {
                            callsite = Callsite.Reachable;
                            if (csAttribute.UnsupportedFirst != null && !IsEmptyVersion(csAttribute.UnsupportedFirst))
                            {
                                if (IsEmptyVersion(supportedVersion))
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                        MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndBefore, pName, csAttribute.UnsupportedFirst));
                                }
                                else
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                        MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityFromVersionToVersion, pName, supportedVersion, csAttribute.UnsupportedFirst));
                                }
                            }
                            else if (IsEmptyVersion(supportedVersion))
                            {
                                if (HasSameVersionedPlatformSupport(attributes, pName, supported))
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityAllVersions, pName));
                                    continue;
                                }
                                platformNames.Add($"'{pName}'");
                            }
                            else
                            {
                                platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                    MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater, pName, supportedVersion));
                            }
                        }
                        else
                        {
                            var unsupportedVersion = csAttribute.UnsupportedSecond ?? csAttribute.UnsupportedFirst;
                            if (unsupportedVersion != null && attributes.Keys.Contains(pName))
                            {
                                callsite = Callsite.Unreachable;
                                if (IsEmptyVersion(unsupportedVersion))
                                {
                                    if (HasSameVersionedPlatformSupport(attributes, pName, supported))
                                    {
                                        platformNames.Add(string.Format(CultureInfo.InvariantCulture, MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityAllVersions, pName));
                                        continue;
                                    }
                                    platformNames.Add($"'{pName}'");
                                }
                                else
                                {
                                    platformNames.Add(string.Format(CultureInfo.InvariantCulture,
                                        MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityVersionAndLater, pName, unsupportedVersion));
                                }
                            }
                        }
                    }
                }
                return platformNames;
            }

            static SymbolDisplayFormat GetLanguageSpecificFormat(IOperation operation) =>
                operation.Language == LanguageNames.CSharp ? SymbolDisplayFormat.CSharpShortErrorMessageFormat : SymbolDisplayFormat.VisualBasicShortErrorMessageFormat;

            static ISymbol GetAccessorMethod(ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers, ISymbol symbol, IEnumerable<ISymbol> accessors)
            {
                foreach (var accessor in accessors)
                {
                    if (accessor != null && platformSpecificMembers.TryGetValue(accessor, out var attribute) && attribute != null)
                    {
                        return accessor;
                    }
                }

                return symbol;
            }

            static bool HasSameVersionedPlatformSupport(SmallDictionary<string, PlatformAttributes> attributes, string pName, bool checkSupport)
            {
                if (attributes.TryGetValue(pName, out var attribute))
                {
                    var version = attribute.UnsupportedSecond ?? attribute.UnsupportedFirst;
                    if (checkSupport)
                    {
                        var supportedVersion = attribute.SupportedSecond ?? attribute.SupportedFirst;
                        if (supportedVersion != null)
                        {
                            version = version == null || supportedVersion >= version ? supportedVersion : version;
                        }
                        else
                        {
                            version = supportedVersion;
                        }
                    }
                    if (version != null && !IsEmptyVersion(version))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private enum Callsite
        {
            AllPlatforms,
            Reachable,
            Unreachable
        }

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

        private static IEnumerable<ISymbol> GetPropertyAccessors(IPropertySymbol property, IOperation operation)
        {
            var usageInfo = operation.GetValueUsageInfo(property.ContainingSymbol);

            // not checking/using ValueUsageInfo.Reference related values as property cannot be used as ref or out parameter
            // not using ValueUsageInfo.Name too, it only use name of the property
            if (usageInfo == ValueUsageInfo.ReadWrite)
            {
                yield return property.GetMethod;
                yield return property.SetMethod;
            }
            else if (usageInfo.IsWrittenTo())
            {
                yield return property.SetMethod;
            }
            else if (usageInfo.IsReadFrom())
            {
                yield return property.GetMethod;
            }
            else
            {
                yield return property;
            }
        }

        private static ISymbol GetEventAccessor(IEventSymbol iEvent, IOperation operation)
        {
            if (operation.Parent is IEventAssignmentOperation eventAssignment)
            {
                if (eventAssignment.Adds)
                    return iEvent.AddMethod;
                else
                    return iEvent.RemoveMethod;
            }
            return iEvent;
        }

        private static void AnalyzeOperation(IOperation operation, OperationAnalysisContext context, PooledConcurrentDictionary<IOperation,
            (SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? csAttributes)> platformSpecificOperations,
            ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers, ImmutableArray<string> msBuildPlatforms,
            ITypeSymbol? notSupportedExceptionType)
        {
            if (operation.Parent is IArgumentOperation argumentOperation && UsedInCreatingNotSupportedException(argumentOperation, notSupportedExceptionType))
            {
                return;
            }

            var symbol = GetOperationSymbol(operation);

            if (symbol == null)
            {
                return;
            }

            if (symbol is IPropertySymbol property)
            {
                foreach (var accessor in GetPropertyAccessors(property, operation))
                {
                    if (accessor != null)
                    {
                        CheckOperationAttributes(operation, context, platformSpecificOperations, platformSpecificMembers, msBuildPlatforms, accessor);
                    }
                }
            }
            else if (symbol is IEventSymbol iEvent)
            {
                var accessor = GetEventAccessor(iEvent, operation);

                if (accessor != null)
                {
                    CheckOperationAttributes(operation, context, platformSpecificOperations, platformSpecificMembers, msBuildPlatforms, accessor);
                }
                else
                {
                    CheckOperationAttributes(operation, context, platformSpecificOperations, platformSpecificMembers, msBuildPlatforms, iEvent);
                }
            }
            else
            {
                CheckOperationAttributes(operation, context, platformSpecificOperations, platformSpecificMembers, msBuildPlatforms, symbol);
            }

            static void CheckOperationAttributes(IOperation operation, OperationAnalysisContext context, PooledConcurrentDictionary<IOperation,
                 (SmallDictionary<string, PlatformAttributes> attributes, SmallDictionary<string, PlatformAttributes>? csAttributes)> platformSpecificOperations,
                ConcurrentDictionary<ISymbol, SmallDictionary<string, PlatformAttributes>?> platformSpecificMembers, ImmutableArray<string> msBuildPlatforms, ISymbol symbol)
            {
                if (TryGetOrCreatePlatformAttributes(symbol, platformSpecificMembers, out var operationAttributes))
                {
                    if (TryGetOrCreatePlatformAttributes(context.ContainingSymbol, platformSpecificMembers, out var callSiteAttributes))
                    {
                        if (IsNotSuppressedByCallSite(operationAttributes, callSiteAttributes, msBuildPlatforms, out var notSuppressedAttributes))
                        {
                            platformSpecificOperations.TryAdd(operation, (notSuppressedAttributes, callSiteAttributes));
                        }
                    }
                    else
                    {
                        if (TryCopyAttributesNotSuppressedByMsBuild(operationAttributes, msBuildPlatforms, out var copiedAttributes))
                        {
                            platformSpecificOperations.TryAdd(operation, (copiedAttributes, null));
                        }
                    }
                }
            }
        }

        private static bool UsedInCreatingNotSupportedException(IArgumentOperation operation, ITypeSymbol? notSupportedExceptionType)
        {
            if (operation.Parent is IObjectCreationOperation creation &&
                operation.Parameter.Type.SpecialType == SpecialType.System_String &&
                creation.Type.DerivesFrom(notSupportedExceptionType, baseTypesOnly: true, checkTypeParameterConstraints: false))
            {
                return true;
            }

            return false;
        }

        private static bool TryCopyAttributesNotSuppressedByMsBuild(SmallDictionary<string, PlatformAttributes> operationAttributes,
            ImmutableArray<string> msBuildPlatforms, out SmallDictionary<string, PlatformAttributes> copiedAttributes)
        {
            copiedAttributes = new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);
            foreach (var (platformName, attributes) in operationAttributes)
            {
                if (AllowList(attributes) || msBuildPlatforms.IndexOf(platformName, 0, StringComparer.OrdinalIgnoreCase) != -1)
                {
                    copiedAttributes.Add(platformName, CopyAllAttributes(new PlatformAttributes(), attributes));
                }
            }

            return copiedAttributes.Any();
        }

        private static SmallDictionary<string, PlatformAttributes> CopyAttributes(SmallDictionary<string, PlatformAttributes> copyAttributes)
        {
            var copy = new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);
            foreach (var (platformName, attributes) in copyAttributes)
            {
                copy.Add(platformName, CopyAllAttributes(new PlatformAttributes(), attributes));
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
        /// </summary>
        /// <param name="operationAttributes">Platform specific attributes applied to the invoked member</param>
        /// <param name="callSiteAttributes">Platform specific attributes applied to the call site where the member invoked</param>
        /// <returns>true if all attributes applied to the operation is suppressed, false otherwise</returns>

        private static bool IsNotSuppressedByCallSite(SmallDictionary<string, PlatformAttributes> operationAttributes,
            SmallDictionary<string, PlatformAttributes> callSiteAttributes, ImmutableArray<string> msBuildPlatforms,
            out SmallDictionary<string, PlatformAttributes> notSuppressedAttributes)
        {
            notSuppressedAttributes = new SmallDictionary<string, PlatformAttributes>(StringComparer.OrdinalIgnoreCase);
            bool? mandatorySupportFound = null;
            using var supportedOnlyPlatforms = PooledHashSet<string>.GetInstance(StringComparer.OrdinalIgnoreCase);
            foreach (var (platformName, attribute) in operationAttributes)
            {
                var diagnosticAttribute = new PlatformAttributes();

                if (attribute.SupportedFirst != null)
                {
                    if (attribute.UnsupportedFirst == null || attribute.UnsupportedFirst > attribute.SupportedFirst)
                    {
                        // If only supported for current platform
                        supportedOnlyPlatforms.Add(platformName);
                        mandatorySupportFound ??= false;

                        if (callSiteAttributes.TryGetValue(platformName, out var callSiteAttribute))
                        {
                            var attributeToCheck = attribute.SupportedSecond ?? attribute.SupportedFirst;
                            if (MandatoryOsVersionsSuppressed(callSiteAttribute, attributeToCheck))
                            {
                                mandatorySupportFound = true;
                            }
                            else
                            {
                                diagnosticAttribute.SupportedSecond = (Version)attributeToCheck.Clone();
                            }

                            if (attribute.UnsupportedFirst != null &&
                                !(SuppressedByCallSiteSupported(attribute, callSiteAttribute.SupportedFirst) ||
                                  SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst)))
                            {
                                diagnosticAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                            }
                        }
                    }
                    else if (attribute.UnsupportedFirst != null) // also means Unsupported < Supported, allow list
                    {
                        if (callSiteAttributes.TryGetValue(platformName, out var callSiteAttribute))
                        {
                            if (callSiteAttribute.SupportedFirst != null)
                            {
                                if (!OptionalOsSupportSuppressed(callSiteAttribute, attribute))
                                {
                                    diagnosticAttribute.SupportedFirst = (Version)attribute.SupportedFirst.Clone();
                                }

                                if (!UnsupportedFirstSuppressed(attribute, callSiteAttribute))
                                {
                                    diagnosticAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                                }

                                if (attribute.UnsupportedSecond != null &&
                                    !UnsupportedSecondSuppressed(attribute, callSiteAttribute))
                                {
                                    diagnosticAttribute.UnsupportedSecond = (Version)attribute.UnsupportedSecond.Clone();
                                }
                            }
                        }
                        else
                        {
                            // Call site has no attributes for this platform, check if MsBuild list has it, 
                            // then if call site has deny list, it should support its later support
                            if (msBuildPlatforms.Contains(platformName) &&
                                callSiteAttributes.Any(ca => DenyList(ca.Value)))
                            {
                                diagnosticAttribute.SupportedFirst = (Version)attribute.SupportedFirst.Clone();
                            }
                        }
                    }
                }
                else
                {
                    if (attribute.UnsupportedFirst != null) // Unsupported for this but supported all other
                    {
                        if (callSiteAttributes.TryGetValue(platformName, out var callSiteAttribute))
                        {
                            if (callSiteAttribute.SupportedFirst != null)
                            {
                                if (!SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst))
                                {
                                    diagnosticAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                                }

                                if (attribute.UnsupportedSecond != null &&
                                    !SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedSecond))
                                {
                                    diagnosticAttribute.UnsupportedSecond = (Version)attribute.UnsupportedSecond.Clone();
                                }
                            }
                        }
                        else if (msBuildPlatforms.Contains(platformName) &&
                            !callSiteAttributes.Values.Any(v => v.SupportedFirst != null))
                        {
                            // if MsBuild list contain the platform and call site has no any other supported attribute it means global, so need to warn
                            diagnosticAttribute.UnsupportedFirst = (Version)attribute.UnsupportedFirst.Clone();
                        }
                    }
                }

                if (diagnosticAttribute.HasAttribute())
                {
                    notSuppressedAttributes[platformName] = diagnosticAttribute;
                }
            }

            if (mandatorySupportFound.HasValue)
            {
                if (!mandatorySupportFound.Value)
                {
                    foreach (var (name, attributes) in operationAttributes)
                    {
                        if (attributes.SupportedFirst != null)
                        {
                            if (!notSuppressedAttributes.TryGetValue(name, out var diagnosticAttribute))
                            {
                                diagnosticAttribute = new PlatformAttributes();
                            }
                            CopyAllAttributes(diagnosticAttribute, attributes);
                            notSuppressedAttributes[name] = diagnosticAttribute;
                        }
                    }
                }

                // if supportedOnlyList then call site should not have any platform not listed in the support list
                foreach (var (platform, csAttributes) in callSiteAttributes)
                {
                    if (csAttributes.SupportedFirst != null &&
                        !supportedOnlyPlatforms.Contains(platform))
                    {
                        foreach (var (name, version) in operationAttributes)
                        {
                            AddOrUpdatedDiagnostic(operationAttributes[name], notSuppressedAttributes, name);
                        }
                    }
                }
            }
            return notSuppressedAttributes.Any();

            static void AddOrUpdatedDiagnostic(PlatformAttributes operationAttributes,
                SmallDictionary<string, PlatformAttributes> notSuppressedAttributes, string name)
            {
                if (operationAttributes.SupportedFirst != null)
                {
                    if (!notSuppressedAttributes.TryGetValue(name, out var diagnosticAttribute))
                    {
                        diagnosticAttribute = new PlatformAttributes();
                    }
                    diagnosticAttribute.SupportedFirst = (Version)operationAttributes.SupportedFirst.Clone();
                    notSuppressedAttributes[name] = diagnosticAttribute;
                }
            }

            static bool UnsupportedSecondSuppressed(PlatformAttributes attribute, PlatformAttributes callSiteAttribute) =>
                SuppressedByCallSiteSupported(attribute, callSiteAttribute.SupportedFirst) ||
                SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedSecond!);

            static bool SuppressedByCallSiteUnsupported(PlatformAttributes callSiteAttribute, Version unsupporteAttribute) =>
                callSiteAttribute.UnsupportedFirst != null && unsupporteAttribute >= callSiteAttribute.UnsupportedFirst ||
                callSiteAttribute.UnsupportedSecond != null && unsupporteAttribute >= callSiteAttribute.UnsupportedSecond;

            static bool SuppressedByCallSiteSupported(PlatformAttributes attribute, Version? callSiteSupportedFirst) =>
                callSiteSupportedFirst != null && callSiteSupportedFirst >= attribute.SupportedFirst! &&
                attribute.SupportedSecond != null && callSiteSupportedFirst >= attribute.SupportedSecond;

            static bool UnsupportedFirstSuppressed(PlatformAttributes attribute, PlatformAttributes callSiteAttribute) =>
                callSiteAttribute.SupportedFirst != null && callSiteAttribute.SupportedFirst >= attribute.SupportedFirst ||
                SuppressedByCallSiteUnsupported(callSiteAttribute, attribute.UnsupportedFirst!);

            // As optianal if call site supports that platform, their versions should match
            static bool OptionalOsSupportSuppressed(PlatformAttributes callSiteAttribute, PlatformAttributes attribute) =>
                (callSiteAttribute.SupportedFirst == null || attribute.SupportedFirst <= callSiteAttribute.SupportedFirst) &&
                (callSiteAttribute.SupportedSecond == null || attribute.SupportedFirst <= callSiteAttribute.SupportedSecond);

            static bool MandatoryOsVersionsSuppressed(PlatformAttributes callSitePlatforms, Version checkingVersion) =>
                callSitePlatforms.SupportedFirst != null && checkingVersion <= callSitePlatforms.SupportedFirst ||
                callSitePlatforms.SupportedSecond != null && checkingVersion <= callSitePlatforms.SupportedSecond;
        }

        private static PlatformAttributes CopyAllAttributes(PlatformAttributes copyTo, PlatformAttributes copyFrom)
        {
            copyTo.SupportedFirst = (Version?)copyFrom.SupportedFirst?.Clone();
            copyTo.SupportedSecond = (Version?)copyFrom.SupportedSecond?.Clone();
            copyTo.UnsupportedFirst = (Version?)copyFrom.UnsupportedFirst?.Clone();
            copyTo.UnsupportedSecond = (Version?)copyFrom.UnsupportedSecond?.Clone();
            return copyTo;
        }

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

                if (symbol is IMethodSymbol method && method.IsAccessorMethod())
                {
                    // Add attributes for the associated Property
                    container = method.AssociatedSymbol;
                }

                // Namespaces do not have attributes
                while (container is INamespaceSymbol)
                {
                    container = container.ContainingSymbol;
                }

                if (container != null &&
                    TryGetOrCreatePlatformAttributes(container, platformSpecificMembers, out var containerAttributes))
                {
                    attributes = CopyAttributes(containerAttributes);
                }

                MergePlatformAttributes(symbol.GetAttributes(), ref attributes);

                attributes = platformSpecificMembers.GetOrAdd(symbol, attributes);
            }

            return attributes != null;

            static void MergePlatformAttributes(ImmutableArray<AttributeData> immediateAttributes, ref SmallDictionary<string, PlatformAttributes>? parentAttributes)
            {
                SmallDictionary<string, PlatformAttributes>? childAttributes = null;
                foreach (AttributeData attribute in immediateAttributes)
                {
                    if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name))
                    {
                        TryAddValidAttribute(ref childAttributes, attribute);
                    }
                }

                if (childAttributes == null)
                {
                    return;
                }

                if (parentAttributes != null && parentAttributes.Any())
                {
                    foreach (var (platform, attributes) in parentAttributes)
                    {
                        if (DenyList(attributes) &&
                            !parentAttributes.Any(ca => AllowList(ca.Value)))
                        {
                            // if all are deny list then we can add the child attributes
                            foreach (var (name, childAttribute) in childAttributes)
                            {
                                if (parentAttributes.TryGetValue(name, out var existing))
                                {
                                    // but don't override existing unless narrowing the support
                                    if (childAttribute.UnsupportedFirst != null &&
                                        childAttribute.UnsupportedFirst < attributes.UnsupportedFirst)
                                    {
                                        attributes.UnsupportedFirst = childAttribute.UnsupportedFirst;
                                    }
                                }
                                else
                                {
                                    parentAttributes[name] = childAttribute;
                                }
                            }
                            // merged all attributes, no need to continue looping
                            return;
                        }
                        else if (AllowList(attributes))
                        {
                            // only attributes with same platform matter, could narrow the list
                            if (childAttributes.TryGetValue(platform, out var childAttribute))
                            {
                                // only later versions could narrow, other versions ignored 
                                if (childAttribute.SupportedFirst > attributes.SupportedFirst)
                                {
                                    attributes.SupportedSecond = childAttribute.SupportedFirst;
                                }

                                if (childAttribute.UnsupportedFirst != null)
                                {
                                    if (childAttribute.UnsupportedFirst <= attributes.SupportedFirst)
                                    {
                                        attributes.SupportedFirst = null;
                                        attributes.SupportedSecond = null;
                                    }
                                    else if (childAttribute.UnsupportedFirst <= attributes.SupportedSecond)
                                    {
                                        attributes.SupportedSecond = null;
                                    }

                                    attributes.UnsupportedFirst = childAttribute.UnsupportedFirst;
                                }
                            }
                            // other platform attributes are ignored as the list couldn't be extended
                        }
                    }
                }
                else
                {
                    parentAttributes = childAttributes;
                }
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
                if (!attributes.TryGetValue(platformName, out var _))
                {
                    attributes[platformName] = new PlatformAttributes();
                }

                AddAttribute(attribute.AttributeClass.Name, version, attributes[platformName]);
                return true;
            }

            return false;
        }

        private static bool TryParsePlatformNameAndVersion(string osString, out string osPlatformName, [NotNullWhen(true)] out Version? version)
        {
            version = null;
            osPlatformName = string.Empty;
            for (int i = 0; i < osString.Length; i++)
            {
                if (char.IsDigit(osString[i]))
                {
                    if (i > 0 && Version.TryParse(osString[i..], out Version? parsedVersion))
                    {
                        osPlatformName = osString.Substring(0, i);
                        version = parsedVersion;
                        return true;
                    }

                    return false;
                }
            }

            osPlatformName = osString;
            version = new Version(0, 0);
            return true;
        }

        private static void AddAttribute(string name, Version version, PlatformAttributes attributes)
        {
            if (name == SupportedOSPlatformAttribute)
            {
                AddOrUpdateSupportedAttribute(attributes, version);
            }
            else
            {
                Debug.Assert(name == UnsupportedOSPlatformAttribute);
                AddOrUpdateUnsupportedAttribute(attributes, version);
            }

            static void AddOrUpdateUnsupportedAttribute(PlatformAttributes attributes, Version version)
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
                            if (attributes.SupportedFirst != null && attributes.SupportedFirst < version)
                            {
                                // We should ignore second attribute in case like [UnsupportedOSPlatform(""windows""), 
                                // [UnsupportedOSPlatform(""windows11.0"")] which doesn't have supported in between
                                attributes.UnsupportedSecond = version;
                            }
                        }
                    }
                }
                else
                {
                    attributes.UnsupportedFirst = version;
                }
            }

            static void AddOrUpdateSupportedAttribute(PlatformAttributes attributes, Version version)
            {
                if (attributes.SupportedFirst != null)
                {
                    if (attributes.SupportedFirst > version)
                    {
                        attributes.SupportedFirst = version;
                    }
                    // only keep lowest version, ignore other versions
                }
                else
                {
                    attributes.SupportedFirst = version;
                }
            }
        }

        /// <summary>
        /// Determines if the attributes supported only for the platform (allow list)
        /// </summary>
        /// <param name="attributes">PlatformAttributes being checked</param>
        /// <returns>true if it is allow list</returns>
        private static bool AllowList(PlatformAttributes attributes) =>
            attributes.SupportedFirst != null &&
            (attributes.UnsupportedFirst == null || attributes.SupportedFirst <= attributes.UnsupportedFirst);

        /// <summary>
        /// Determines if the attributes unsupported only for the platform (deny list)
        /// </summary>
        /// <param name="attributes">PlatformAttributes being checked</param>
        /// <returns>true if it is deny list</returns>
        private static bool DenyList(PlatformAttributes attributes) =>
            attributes.UnsupportedFirst != null &&
            (attributes.SupportedFirst == null || attributes.UnsupportedFirst < attributes.SupportedFirst);
    }
}
