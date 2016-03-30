// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1714: Flags enums should have plural names
    /// CA1717: Only Flags enums should have plural names
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class EnumsShouldHavePluralNamesAnalyzer : DiagnosticAnalyzer
    {
        #region CA1714
        internal const string RuleId_Plural = "CA1714";

        private static readonly LocalizableString s_localizableTitle_CA1714 =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.FlagsEnumsShouldHavePluralNamesTitle),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage_CA1714 =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.FlagsEnumsShouldHavePluralNamesMessage),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription_CA1714 =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.FlagsEnumsShouldHavePluralNamesDescription),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule_CA1714 =
            new DiagnosticDescriptor(
                RuleId_Plural,
                s_localizableTitle_CA1714,
                s_localizableMessage_CA1714,
                DiagnosticCategory.Naming,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: s_localizableDescription_CA1714,
                helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb264486.aspx",
                customTags: WellKnownDiagnosticTags.Telemetry);

        #endregion

        #region CA1717
        internal const string RuleId_NoPlural = "CA1717";

        private static readonly LocalizableString s_localizableTitle_CA1717 =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OnlyFlagsEnumsShouldHavePluralNamesTitle),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage_CA1717 =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OnlyFlagsEnumsShouldHavePluralNamesMessage),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription_CA1717 =
            new LocalizableResourceString(
                nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OnlyFlagsEnumsShouldHavePluralNamesDescription),
                MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
                typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule_CA1717 =
            new DiagnosticDescriptor(
                RuleId_NoPlural,
                s_localizableTitle_CA1717,
                s_localizableMessage_CA1717,
                DiagnosticCategory.Naming,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: s_localizableDescription_CA1717,
                helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb264487.aspx",
                customTags: WellKnownDiagnosticTags.Telemetry);

        #endregion

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule_CA1714, Rule_CA1717);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol flagsAttribute = WellKnownTypes.FlagsAttribute(compilationContext.Compilation);
                if (flagsAttribute == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, flagsAttribute), SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol flagsAttribute)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;
            if (symbol.TypeKind != TypeKind.Enum)
            {
                return;
            }

            bool hasFlagsAttribute = symbol.GetAttributes().Any(a => a.AttributeClass.Equals(flagsAttribute));
            if (hasFlagsAttribute)
            {
                if (!symbol.Name.IsPlural()) // Checking Rule CA1714
                {
                    context.ReportDiagnostic(symbol.CreateDiagnostic(Rule_CA1714, symbol.OriginalDefinition.Locations.First(), symbol.Name));
                }
            }
            else
            {
                if (symbol.Name.IsPlural()) // Checking Rule CA1717
                {
                    context.ReportDiagnostic(symbol.CreateDiagnostic(Rule_CA1717, symbol.OriginalDefinition.Locations.First(), symbol.Name));
                }
            }
        }
    }
}

