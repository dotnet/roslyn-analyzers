// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    public abstract class EnumShouldNotHaveDuplicatedValues : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1069";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.EnumShouldNotHaveDuplicatedValuesTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageRuleDuplicatedValue = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.EnumShouldNotHaveDuplicatedValuesMessageDuplicatedValue), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        internal static DiagnosticDescriptor RuleDuplicatedValue = new DiagnosticDescriptor(RuleId,
                                                                                            s_localizableTitle,
                                                                                            s_localizableMessageRuleDuplicatedValue,
                                                                                            DiagnosticCategory.Design,
                                                                                            DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                            isEnabledByDefault: false,
                                                                                            helpLinkUri: null,
                                                                                            customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly LocalizableString s_localizableMessageRuleDuplicatedBitwiseValuePart = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.EnumShouldNotHaveDuplicatedValuesMessageDuplicatedBitwiseValuePart), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        internal static DiagnosticDescriptor RuleDuplicatedBitwiseValuePart = new DiagnosticDescriptor(RuleId,
                                                                                                   s_localizableTitle,
                                                                                                   s_localizableMessageRuleDuplicatedBitwiseValuePart,
                                                                                                   DiagnosticCategory.Design,
                                                                                                   DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                                   isEnabledByDefault: false,
                                                                                                   helpLinkUri: null,
                                                                                                   customTags: WellKnownDiagnosticTags.Telemetry);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleDuplicatedValue, RuleDuplicatedBitwiseValuePart);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(symbolContext =>
            {
                var typeSymbol = (INamedTypeSymbol)symbolContext.Symbol;
                if (typeSymbol.TypeKind != TypeKind.Enum)
                {
                    return;
                }

                var enumMemberValues = typeSymbol.GetMembers()
                    .Where(m => m.Kind == SymbolKind.Field)
                    .Cast<IFieldSymbol>()
                    .Where(m => m.HasConstantValue && m.DeclaringSyntaxReferences.Length == 1)
                    .Select(m => (Value: m.ConstantValue, DeclaringSyntax: m.DeclaringSyntaxReferences[0].GetSyntax(symbolContext.CancellationToken)))
                    .GroupBy(ev => ev.Value, ev => ev.DeclaringSyntax)
                    .ToDictionary(ev => ev.Key, ev => ev.ToList());

                AnalyzeEnumMemberValues(enumMemberValues, symbolContext);
            }, SymbolKind.NamedType);
        }

        protected abstract void AnalyzeEnumMemberValues(Dictionary<object, List<SyntaxNode>> membersByValue, SymbolAnalysisContext context);
    }
}
