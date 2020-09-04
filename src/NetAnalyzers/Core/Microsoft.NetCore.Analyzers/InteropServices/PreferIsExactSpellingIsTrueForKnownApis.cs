// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.InteropServices
{
    /// <summary>
    /// CA1839: Prefer ExactSpelling=true for known Apis analyzer
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PreferIsExactSpellingIsTrueForKnownApisAnalyzer : DiagnosticAnalyzer
    {
        [Serializable]
#pragma warning disable CA1034 // Nested types should not be visible
        public class KnownApi //Only made public as XmlSerializer requires it
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string? Dll { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
            public string[]? Methods { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        }
        internal const string RuleId = "CA1839";

        internal static readonly Lazy<Dictionary<string, HashSet<string>>> KnownApis = new Lazy<Dictionary<string, HashSet<string>>>(() => CreateKnownApis());

        private static Dictionary<string, HashSet<string>> CreateKnownApis()
        {
            var serializer = new XmlSerializer(typeof(KnownApi[]));
            var str = MicrosoftNetCoreAnalyzersResources.ResourceManager.GetObject("KnownApiList", CultureInfo.InvariantCulture);
            using (var stringReader = new StringReader(str.ToString()))
            using (var reader = XmlReader.Create(stringReader))
            {
                var knownApis = (serializer.Deserialize(reader) as KnownApi[])!;
                return knownApis.ToDictionary(x => x.Dll!, x => new HashSet<string>(x.Methods!));
            }
        }

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionWideSuffix = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.PreferIsExactSpellingIsTrueForKnownApisWithSuffixDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
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
                                                                             s_localizableDescriptionWideSuffix,
                                                                             DiagnosticCategory.Performance,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false,
                                                                             isEnabledByDefaultInFxCopAnalyzers: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            analysisContext.RegisterSymbolAction(AnalyzeExternMethod, SymbolKind.Method);
        }

        private static void AnalyzeExternMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;
            if (!methodSymbol.IsExtern)
                return;
            var dllImportAttribute = methodSymbol
                .GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.Name.Equals("DllImportAttribute", StringComparison.Ordinal));
            if (dllImportAttribute is null)
                return;

            var hasEntryPointParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("EntryPoint", StringComparison.Ordinal));
            var hasExactSpellingParameter = dllImportAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("ExactSpelling", StringComparison.Ordinal));

            var methodName = hasEntryPointParameter.Key is null ? methodSymbol.Name : hasEntryPointParameter.Value.Value.ToString();
            var isExactSpelling = !(hasExactSpellingParameter.Key is null) && (bool)hasExactSpellingParameter.Value.Value;
            var dllName = dllImportAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? string.Empty;
            if (KnownApis.Value.TryGetValue(dllName, out var methods))
            {
                if (methods.Contains(methodName))
                {
                    Diagnostic? diagnostic = null;
                    if (methods.Contains(methodName + "W")) //Whatever exactspelling is is wrong, wide method should be preferred
                    {
                        diagnostic = Diagnostic.Create(WideRule, methodSymbol.Locations[0], methodName);
                    }
                    if (isExactSpelling) //Known method has parameter set to true
                        return;
                    diagnostic ??= Diagnostic.Create(DefaultRule, methodSymbol.Locations[0], methodName);
                    context.ReportDiagnostic(diagnostic);

                }
            }
        }
    }
}