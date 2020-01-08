// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1000: Do not declare static members on generic types
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDeclareStaticMembersOnGenericTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1000";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1000-do-not-declare-static-members-on-generic-types",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(
                symbolAnalysisContext =>
                {
                    // Fxcop compat: fire only on public static members within externally visible generic types by default.
                    ISymbol symbol = symbolAnalysisContext.Symbol;
                    if (!symbol.IsStatic ||
                        symbol.DeclaredAccessibility != Accessibility.Public ||
                        !symbol.ContainingType.IsGenericType ||
                        !symbol.ContainingType.MatchesConfiguredVisibility(symbolAnalysisContext.Options, Rule, symbolAnalysisContext.CancellationToken))
                    {
                        return;
                    }

                    // Do not flag non-ordinary methods, such as conversions, operator overloads, etc.
                    if (symbol is IMethodSymbol methodSymbol &&
                        methodSymbol.MethodKind != MethodKind.Ordinary)
                    {
                        return;
                    }

                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
                }, SymbolKind.Method, SymbolKind.Property);
        }
    }
}