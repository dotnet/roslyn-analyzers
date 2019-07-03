﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using static Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.MicrosoftApiDesignGuidelinesAnalyzersResources;
using System.Collections.Generic;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1712: Do not prefix enum values with type name
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotPrefixEnumValuesWithTypeNameAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1712";
        private const int PercentValuesPrefixedThreshold = 75; // The percent of an enum's values that must appear to be prefixed in order for a diagnostic to be reported on the enum. This value comes from the original FxCop rule's implementation.

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DoNotPrefixEnumValuesWithTypeNameTitle), ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(DoNotPrefixEnumValuesWithTypeNameMessage), ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DoNotPrefixEnumValuesWithTypeNameDescription), ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                RuleId,
                s_localizableTitle,
                s_localizableMessage,
                DiagnosticCategory.Naming,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_localizableDescription,
                helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1712-do-not-prefix-enum-values-with-type-name",
                customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            analysisContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;
            IEnumerable<ISymbol> enumValues;
            if (symbol.TypeKind != TypeKind.Enum || !(enumValues = symbol.GetMembers().Where(m => m.Kind == SymbolKind.Field)).Any())
            {
                return;
            }

            var prefixedValues = enumValues.Where(m => m.Name.StartsWith(symbol.Name, StringComparison.OrdinalIgnoreCase));
            int percentPrefixed = 100 * prefixedValues.Count() / enumValues.Count();
            if (percentPrefixed >= PercentValuesPrefixedThreshold)
            {
                foreach (var value in prefixedValues)
                {
                    context.ReportDiagnostic(value.CreateDiagnostic(Rule, symbol.Name));
                }
            }
        }
    }
}
