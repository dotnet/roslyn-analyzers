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

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182139.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(
                symbolAnalysisContext =>
                {
                    // Fxcop compat: fire only on public static members within externally visible generic types.
                    ISymbol symbol = symbolAnalysisContext.Symbol;
                    if (!symbol.IsStatic ||
                        symbol.DeclaredAccessibility != Accessibility.Public ||
                        !symbol.ContainingType.IsGenericType ||
                        !symbol.ContainingType.IsExternallyVisible())
                    {
                        return;
                    }

                    if (symbol is IMethodSymbol methodSymbol &&
                        (methodSymbol.IsAccessorMethod() ||
                        (methodSymbol.MethodKind == MethodKind.UserDefinedOperator &&
                        (methodSymbol.Name == WellKnownMemberNames.EqualityOperatorName ||
                        methodSymbol.Name == WellKnownMemberNames.InequalityOperatorName)) ||
                        methodSymbol.MethodKind == MethodKind.Conversion))
                    {
                        return;
                    }

                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
                }, SymbolKind.Method, SymbolKind.Property);
        }
    }
}