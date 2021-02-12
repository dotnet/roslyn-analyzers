// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseValidPlatformString : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1418";
        private static readonly ImmutableArray<SymbolKind> s_symbols = ImmutableArray.Create(SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, SymbolKind.Event);
        private static readonly ImmutableArray<string> methodNames = ImmutableArray.Create("IsOSPlatform", "IsOSPlatformVersionAtLeast");
        private const string IsPrefix = "Is";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseValidPlatformStringTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableUnknownPlatform = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseValidPlatformStringUnknownPlatform), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableInvalidVersion = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseValidPlatformStringInvalidVersion), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseValidPlatformStringDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor UnknownPlatform = DiagnosticDescriptorHelper.Create(RuleId,
                                                                              s_localizableTitle,
                                                                              s_localizableUnknownPlatform,
                                                                              DiagnosticCategory.Interoperability,
                                                                              RuleLevel.BuildWarning,
                                                                              description: s_localizableDescription,
                                                                              isPortedFxCopRule: false,
                                                                              isDataflowRule: false);

        internal static DiagnosticDescriptor InvalidVersion = DiagnosticDescriptorHelper.Create(RuleId,
                                                                              s_localizableTitle,
                                                                              s_localizableInvalidVersion,
                                                                              DiagnosticCategory.Interoperability,
                                                                              RuleLevel.BuildWarning,
                                                                              description: s_localizableDescription,
                                                                              isPortedFxCopRule: false,
                                                                              isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UnknownPlatform, InvalidVersion);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemOperatingSystem, out var operatingSystemType) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeVersioningSupportedOSPlatformAttribute, out var supportedAttriubte) ||
                    !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeVersioningUnsupportedOSPlatformAttribute, out var unsupportedAttribute))
                {
                    return;
                }

                var knownPlatforms = context.Options.GetMSBuildItemMetadataValues(MSBuildItemOptionNames.SupportedPlatform, context.Compilation, context.CancellationToken);
                knownPlatforms = knownPlatforms.AddRange(GetPlatformNamesFromGuardMethods(operatingSystemType));

                context.RegisterOperationAction(context => AnalyzeOperation(context.Operation, context, knownPlatforms), OperationKind.Invocation);
                context.RegisterSymbolAction(context => AnalyzeSymbol(context.ReportDiagnostic, context.Symbol,
                    supportedAttriubte, unsupportedAttribute, knownPlatforms, context.CancellationToken), s_symbols);
                context.RegisterCompilationEndAction(context => AnalyzeSymbol(context.ReportDiagnostic, context.Compilation.Assembly,
                    supportedAttriubte, unsupportedAttribute, knownPlatforms, context.CancellationToken));
            });

            static IEnumerable<string> GetPlatformNamesFromGuardMethods(INamedTypeSymbol operatingSystemType)
            {
                return operatingSystemType.GetMembers().OfType<IMethodSymbol>().Where(m =>
                        m.IsStatic &&
                        m.ReturnType.SpecialType == SpecialType.System_Boolean &&
                        NameAndParametersValid(m)).Select(m => m.Name[2..]);
            }

            static bool NameAndParametersValid(IMethodSymbol method) =>
                method.Name.StartsWith(IsPrefix, StringComparison.Ordinal) &&
                method.Parameters.Length == 0;
        }

        private static void AnalyzeOperation(IOperation operation, OperationAnalysisContext context, ImmutableArray<string> knownPlatforms)
        {
            if (operation is IInvocationOperation invocation &&
                methodNames.Contains(invocation.TargetMethod.Name) &&
                invocation.Arguments.Length > 0 &&
                invocation.Arguments[0].Value is { } argument &&
                argument.ConstantValue.HasValue &&
                argument.ConstantValue.Value is string platformName &&
                IsNotKnownPlatform(knownPlatforms, platformName))
            {
                context.ReportDiagnostic(argument.Syntax.CreateDiagnostic(UnknownPlatform, platformName));
            }
        }

        private static void AnalyzeSymbol(Action<Diagnostic> reportDiagnostic, ISymbol symbol, INamedTypeSymbol supportedAttrbute,
            INamedTypeSymbol unsupportedAttribute, ImmutableArray<string> knownPlatforms, CancellationToken token)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (supportedAttrbute.Equals(attribute.AttributeClass.OriginalDefinition, SymbolEqualityComparer.Default) ||
                    unsupportedAttribute.Equals(attribute.AttributeClass.OriginalDefinition, SymbolEqualityComparer.Default))
                {
                    AnalyzeAttribute(reportDiagnostic, attribute, knownPlatforms, token);
                }
            }
        }

        private static void AnalyzeAttribute(Action<Diagnostic> reportDiagnostic, AttributeData attributeData, ImmutableArray<string> knownPlatforms, CancellationToken token)
        {
            var constructorArguments = attributeData.ConstructorArguments;

            if (constructorArguments.Length == 1)
            {
                if (constructorArguments[0].Value is string value)
                {
                    AnalyzeStringParameter(reportDiagnostic, attributeData.ApplicationSyntaxReference.GetSyntax(token), knownPlatforms, value);
                }
                else
                {
                    reportDiagnostic(attributeData.ApplicationSyntaxReference.GetSyntax(token).CreateDiagnostic(UnknownPlatform, "null"));
                }
            }

            static void AnalyzeStringParameter(Action<Diagnostic> reportDiagnostic, SyntaxNode syntax, ImmutableArray<string> knownPlatforms, string value)
            {
                if (TryParsePlatformNameAndVersion(value, out var platformName, out var _))
                {
                    if (IsNotKnownPlatform(knownPlatforms, platformName))
                    {
                        reportDiagnostic(syntax.CreateDiagnostic(UnknownPlatform, platformName));
                    }
                }
                else
                {
                    // version were not parsable, invalid version set in platformName
                    reportDiagnostic(syntax.CreateDiagnostic(InvalidVersion, platformName));
                }
            }
        }

        private static bool IsNotKnownPlatform(ImmutableArray<string> knownPlatforms, string platformName) =>
            platformName.Length == 0 || !knownPlatforms.Contains(platformName, StringComparer.OrdinalIgnoreCase);

        private static bool TryParsePlatformNameAndVersion(string osString, out string osPlatformName, [NotNullWhen(true)] out Version? version)
        {
            version = null;

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

                    // setting to the invalid version part for reporting in the diagnostics message
                    osPlatformName = osString[i..];
                    return false;
                }
            }

            osPlatformName = osString;
            version = new Version(0, 0);
            return true;
        }
    }
}
