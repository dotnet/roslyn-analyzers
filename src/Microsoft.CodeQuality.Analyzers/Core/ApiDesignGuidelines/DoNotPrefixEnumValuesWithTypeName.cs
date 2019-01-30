// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using static Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.MicrosoftApiDesignGuidelinesAnalyzersResources;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1712: Do not prefix enum values with type name
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotPrefixEnumValuesWithTypeNameAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1712";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DoNotPrefixEnumValuesWithTypeNameTitle), ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(DoNotPrefixEnumValuesWithTypeNameMessage), ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DoNotPrefixEnumValuesWithTypeNameDescription), ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                RuleId,
                s_localizableTitle,
                s_localizableMessage,
                DiagnosticCategory.Naming,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: s_localizableDescription,
                helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182237.aspx",
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
            if (symbol.TypeKind == TypeKind.Enum && 
                symbol.GetMembers().Any(m => m.Kind == SymbolKind.Field && m.Name.StartsWith(symbol.Name, StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
            }
        }
    }
}
