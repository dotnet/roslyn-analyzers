// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        private static readonly ImmutableArray<string> s_platformCheckMethodNames = ImmutableArray.Create("IsOSPlatformOrLater", "IsOSPlatformEarlierThan");
        private static readonly ImmutableArray<string> s_osPlatformAttributes = ImmutableArray.Create(MinimumOSPlatformAttribute, ObsoletedInOSPlatformAttribute, RemovedInOSPlatformAttribute);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableAddedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatibilityCheckAddedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableObsoleteMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckObsoleteMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableRemovedMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckRemovedMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PlatformCompatabilityCheckDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private const string MinimumOSPlatformAttribute = nameof(MinimumOSPlatformAttribute);
        private const string ObsoletedInOSPlatformAttribute = nameof(ObsoletedInOSPlatformAttribute);
        private const string RemovedInOSPlatformAttribute = nameof(RemovedInOSPlatformAttribute);
        private const string TargetPlatformAttribute = nameof(TargetPlatformAttribute);
        private const string OSX = nameof(OSPlatform.OSX);
        private const string macOS = nameof(macOS);

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
            var platformSpecificOperations = PooledDictionary<IOperation, ImmutableArray<PlatformAttributeInfo>>.GetInstance();

            context.RegisterOperationAction(context =>
            {
                AnalyzeOperation(context.Operation, context, platformSpecificOperations);
            }
            , OperationKind.MethodReference, OperationKind.EventReference, OperationKind.FieldReference,
            OperationKind.Invocation, OperationKind.ObjectCreation, OperationKind.PropertyReference);

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
                        return;
                    }

                    foreach (var platformSpecificOperation in platformSpecificOperations)
                    {
                        var value = analysisResult[platformSpecificOperation.Key.Kind, platformSpecificOperation.Key.Syntax];
                        PlatformAttributeInfo attribute = platformSpecificOperation.Value.FirstOrDefault();

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

        private static bool IsKnownValueGuarded(PlatformAttributeInfo attribute, GlobalFlowStateAnalysisValueSet value)
        {
            foreach (var analysisValue in value.AnalysisValues)
            {
                if (analysisValue is RuntimeMethodValue info)
                {
                    if (!info.Negated)
                    {
                        if (IsOSPlatformsEqual(attribute.PlatformName, info.PlatformPropertyName))
                        {
                            if (info.InvokedPlatformCheckMethodName == s_platformCheckMethodNames[0])
                            {
                                if (attribute.AttributeType == PlatformAttributeType.MinimumOSPlatformAttribute &&
                                    AttributeVersionsMatch(attribute.AttributeType, attribute.Version, info.Version))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if ((attribute.AttributeType == PlatformAttributeType.ObsoletedInOSPlatformAttribute ||
                                    attribute.AttributeType == PlatformAttributeType.RemovedInOSPlatformAttribute) &&
                                    AttributeVersionsMatch(attribute.AttributeType, attribute.Version, info.Version))
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

        private static void ReportDiagnosticsForAll(PooledDictionary<IOperation, ImmutableArray<PlatformAttributeInfo>> platformSpecificOperations, OperationBlockAnalysisContext context)
        {
            foreach (var platformSpecificOperation in platformSpecificOperations)
            {
                ReportDiagnostics(platformSpecificOperation.Key, platformSpecificOperation.Value.FirstOrDefault(), context);
            }
        }

        private static void ReportDiagnostics(IOperation operation, PlatformAttributeInfo attribute, OperationBlockAnalysisContext context)
        {
            var operationName = GetOperationSymbol(operation)?.Name;

            context.ReportDiagnostic(operation.CreateDiagnostic(SelectRule(attribute.AttributeType),
                operationName ?? string.Empty, attribute.PlatformName, attribute.Version.ToString()));
        }

        private static ISymbol? GetOperationSymbol(IOperation operation)
            => operation switch
            {
                IInvocationOperation iOperation => iOperation.TargetMethod,
                IObjectCreationOperation cOperation => cOperation.Constructor,
                IFieldReferenceOperation fOperation => IsWithinConditionalOperation(operation) ? null : fOperation.Field,
                IMemberReferenceOperation mOperation => mOperation.Member,
                _ => null,
            };

        private static void AnalyzeOperation(IOperation operation, OperationAnalysisContext context,
            PooledDictionary<IOperation, ImmutableArray<PlatformAttributeInfo>> platformSpecificOperations)
        {
            using var builder = ArrayBuilder<PlatformAttributeInfo>.GetInstance();
            var symbol = GetOperationSymbol(operation);

            if (symbol != null)
            {
                var attributes = FindAllPlatformAttributesApplied(symbol.GetAttributes(), symbol.ContainingType);

                foreach (AttributeData attribute in attributes)
                {
                    if (PlatformAttributeInfo.TryParsePlatformAttributeInfo(attribute, out PlatformAttributeInfo parsedAttribute))
                    {
                        if (!IsSuppressedByAttribute(parsedAttribute, context.ContainingSymbol))
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
            pOperation.ConstantValue.HasValue &&
            pOperation.Parent is IBinaryOperation bo &&
            (bo.OperatorKind == BinaryOperatorKind.Equals ||
            bo.OperatorKind == BinaryOperatorKind.NotEquals ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThan ||
            bo.OperatorKind == BinaryOperatorKind.LessThan ||
            bo.OperatorKind == BinaryOperatorKind.GreaterThanOrEqual ||
            bo.OperatorKind == BinaryOperatorKind.LessThanOrEqual);

        private static bool IsOSPlatformsEqual(string left, string right)
        {
            if (left.Equals(right, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                var normalizedLeft = NormalizeOSName(left);
                var normalizedRight = NormalizeOSName(right);
                return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string NormalizeOSName(string name) => name.Equals(OSX, StringComparison.OrdinalIgnoreCase)
            ? macOS
            : name;

        private static DiagnosticDescriptor SelectRule(PlatformAttributeType attributeType)
            => attributeType switch
            {
                PlatformAttributeType.MinimumOSPlatformAttribute => MinimumOsRule,
                PlatformAttributeType.ObsoletedInOSPlatformAttribute => ObsoleteRule,
                _ => RemovedRule,
            };

        private static ImmutableArray<AttributeData> FindAllPlatformAttributesApplied(ImmutableArray<AttributeData> immediateAttributes, INamedTypeSymbol parent)
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
                    if (s_osPlatformAttributes.Contains(attribute.AttributeClass.Name) &&
                        TargetPlatformAttribute != attribute.AttributeClass.Name)
                    {
                        builder.Add(attribute);
                    }
                }
                parent = parent.BaseType;
            }

            return builder.ToImmutableArray();
        }

        private static bool IsSuppressedByAttribute(PlatformAttributeInfo diagnosingAttribute, ISymbol containingSymbol)
        {
            while (containingSymbol != null)
            {
                var attributes = containingSymbol.GetAttributes();
                if (!attributes.IsEmpty)
                {
                    foreach (AttributeData attribute in attributes)
                    {
                        if (diagnosingAttribute.AttributeType.ToString() == attribute.AttributeClass.Name)
                        {
                            if (PlatformAttributeInfo.TryParsePlatformAttributeInfo(attribute, out PlatformAttributeInfo parsedAttribute))
                            {
                                if (IsOSPlatformsEqual(diagnosingAttribute.PlatformName, parsedAttribute.PlatformName) &&
                                    AttributeVersionsMatch(diagnosingAttribute, parsedAttribute))
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