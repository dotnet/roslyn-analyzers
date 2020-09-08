// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    internal static class PreferIsExactSpellingIsTrueForKownApisHelpers
    {
        internal static string GetActualExternName(this IMethodSymbol methodSymbol, AttributeData attribte)
        {
            var hasEntryPointParameter = attribte.NamedArguments.FirstOrDefault(x => x.Key.Equals("EntryPoint", StringComparison.Ordinal));
            return hasEntryPointParameter.Key is null ? methodSymbol.Name : hasEntryPointParameter.Value.Value.ToString();
        }
    }

    /// <summary>
    /// CA1839: Prefer ExactSpelling=true for known Apis analyzer
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PreferIsExactSpellingIsTrueForKnownApisAnalyzer : DiagnosticAnalyzer
    {
        [Serializable]
#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class KnownApi //Only made public as XmlSerializer requires it
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string? Dll { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
            public string[]? Methods { get; set; } // DTO object may ignore this warning
#pragma warning restore CA1819 // Properties should not return arrays
        }

        internal const string RuleId = "CA1839";

        internal static readonly Lazy<Dictionary<string, HashSet<string>>> KnownApis = new Lazy<Dictionary<string, HashSet<string>>>(() => CreateKnownApis());

        private static Dictionary<string, HashSet<string>> CreateKnownApis()
        {
            var serializer = new XmlSerializer(typeof(KnownApi[]));
            var str = MicrosoftNetCoreAnalyzersResources.ResourceManager.GetObject("KnownApiList", CultureInfo.InvariantCulture);
            using var stringReader = new StringReader(str.ToString());
            using var reader = XmlReader.Create(stringReader);
            var knownApis = (KnownApi[])serializer.Deserialize(reader);
            return knownApis.ToDictionary(x => x.Dll!, x => new HashSet<string>(x.Methods!));
        }

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageWideSuffix = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisWithSuffixMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Performance,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false,
                                                                             isEnabledByDefaultInFxCopAnalyzers: true);

        internal static DiagnosticDescriptor WideRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageWideSuffix,
                                                                             DiagnosticCategory.Performance,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false,
                                                                             isEnabledByDefaultInFxCopAnalyzers: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, WideRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            analysisContext.RegisterCompilationStartAction(ctx =>
            {

                if (!ctx.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesDllImportAttribute, out var dllImportType))
                {
                    return;
                }

                ctx.RegisterSymbolAction(x => AnalyzeExternMethod(x, dllImportType), SymbolKind.Method);
            });
        }

        private static void AnalyzeExternMethod(SymbolAnalysisContext context, INamedTypeSymbol dllImportType)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            if (!methodSymbol.IsExtern)
            {
                return;
            }

            var dllImportAttribute = methodSymbol
                .GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.Equals(dllImportType));

            if (dllImportAttribute is null)
            {
                return;
            }

            var hasExactSpellingParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("ExactSpelling", StringComparison.Ordinal));
            var hasCharSetParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("CharSet", StringComparison.Ordinal));

            var methodName = methodSymbol.GetActualExternName(dllImportAttribute);
            var isExactSpelling = hasExactSpellingParameter.Key is not null && hasExactSpellingParameter.Value.Kind != TypedConstantKind.Array && bool.TryParse(hasExactSpellingParameter.Value.Value?.ToString(), out var isExact) && isExact;
            var isCharSetUnicode = hasCharSetParameter.Key is not null && hasCharSetParameter.Value.Kind != TypedConstantKind.Array && Enum.TryParse<CharSet>(hasCharSetParameter.Value.Value?.ToString(), out var actualCharSet) && actualCharSet == CharSet.Unicode;
            var dllName = dllImportAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;

            if (!KnownApis.Value.TryGetValue(dllName, out var methods))
            {
                return;
            }

            if (methods.Contains(methodName))
            {
                if (isCharSetUnicode && !isExactSpelling && methodName.EndsWith("W", StringComparison.Ordinal))
                {
                    var diagnostic = methodSymbol.CreateDiagnostic(WideRule, methodName);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                if (!isCharSetUnicode && !isExactSpelling && methodName.EndsWith("A", StringComparison.Ordinal))
                {
                    var diagnostic = methodSymbol.CreateDiagnostic(DefaultRule, methodName);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

            if (methods.Contains(methodName + "A") && !isCharSetUnicode)
            {
                if (isExactSpelling) //Known method has parameter set to true
                    return;
                var diagnostic = methodSymbol.CreateDiagnostic(DefaultRule, methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}