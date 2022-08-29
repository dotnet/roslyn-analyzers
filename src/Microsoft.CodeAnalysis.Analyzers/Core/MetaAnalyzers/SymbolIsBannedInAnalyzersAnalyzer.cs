// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.BannedApiAnalyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Analyzers
{
    using static CodeAnalysisDiagnosticsResources;

    internal static class SymbolIsBannedInAnalyzersAnalyzer
    {
        public static readonly DiagnosticDescriptor SymbolIsBannedRule = new(
            id: DiagnosticIds.SymbolIsBannedInAnalyzersRuleId,
            title: CreateLocalizableResourceString(nameof(SymbolIsBannedInAnalyzersTitle)),
            messageFormat: CreateLocalizableResourceString(nameof(SymbolIsBannedInAnalyzersMessage)),
            category: DiagnosticCategory.MicrosoftCodeAnalysisCorrectness,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(SymbolIsBannedInAnalyzersDescription)),
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        public static readonly DiagnosticDescriptor NoSettingSpecifiedSymbolIsBannedRule = new(
            id: DiagnosticIds.NoSettingSpecifiedSymbolIsBannedInAnalyzersRuleId,
            title: CreateLocalizableResourceString(nameof(NoSettingSpecifiedSymbolIsBannedInAnalyzersTitle)),
            messageFormat: CreateLocalizableResourceString(nameof(NoSettingSpecifiedSymbolIsBannedInAnalyzersMessage)),
            category: DiagnosticCategory.MicrosoftCodeAnalysisCorrectness,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(NoSettingSpecifiedSymbolIsBannedInAnalyzersDescription)),
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);
    }

    public abstract class SymbolIsBannedInAnalyzersAnalyzer<TSyntaxKind> : SymbolIsBannedAnalyzerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SymbolIsBannedInAnalyzersAnalyzer.SymbolIsBannedRule, SymbolIsBannedInAnalyzersAnalyzer.NoSettingSpecifiedSymbolIsBannedRule);

#pragma warning disable RS1012 // 'compilationContext' does not register any analyzer actions. Consider moving actions registered in 'Initialize' that depend on this start action to 'compilationContext'.
        protected sealed override bool OptedInToBannedSymbolEnforcement(CompilationStartAnalysisContext compilationContext, SyntaxNode syntax)
        {
            return compilationContext.Options.GetBoolOptionValue(
                EditorConfigOptionNames.EnforceExtendedAnalyzerRules,
                rule: null,
                syntax.SyntaxTree,
                compilationContext.Compilation,
                defaultValue: false);
        }

        protected sealed override DiagnosticDescriptor SymbolIsBannedRule { get; } = SymbolIsBannedInAnalyzersAnalyzer.SymbolIsBannedRule;

        protected sealed override Dictionary<ISymbol, BanFileEntry>? ReadBannedApis(CompilationStartAnalysisContext compilationContext)
        {
            var provider = WellKnownTypeProvider.GetOrCreate(compilationContext.Compilation);
            var diagnosticAnalyzerAttributeType = provider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisDiagnosticsDiagnosticAnalyzerAttribute);
            var generatorAttributeType = provider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisGeneratorAttribute);
            compilationContext.RegisterSymbolAction(analyzePossibleAnalyzerOrGenerator, SymbolKind.NamedType);

            const string fileName = "Microsoft.CodeAnalysis.Analyzers.AnalyzerBannedSymbols.txt";
            var stream = typeof(SymbolIsBannedInAnalyzersAnalyzer<>).Assembly.GetManifestResourceStream(fileName);
            var source = SourceText.From(stream);
            var result = new Dictionary<ISymbol, BanFileEntry>();
            foreach (var line in source.Lines)
            {
                var text = line.ToString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var entry = new BanFileEntry(text, line.Span, source, fileName);
                var symbols = DocumentationCommentId.GetSymbolsForDeclarationId(entry.DeclarationId, compilationContext.Compilation);
                if (!symbols.IsDefaultOrEmpty)
                {
                    foreach (var symbol in symbols)
                    {
                        result.Add(symbol, entry);
                    }
                }
            }

            return result;

            void analyzePossibleAnalyzerOrGenerator(SymbolAnalysisContext symbolAnalysisContext)
            {
                var symbol = symbolAnalysisContext.Symbol;

                var attributes = symbol.GetAttributes();
                if (attributes.Any(shouldReportNotSpecifiedEnforceAnalyzerBannedApisSetting))
                {
                    symbolAnalysisContext.ReportDiagnostic(symbol.Locations.CreateDiagnostic(SymbolIsBannedInAnalyzersAnalyzer.NoSettingSpecifiedSymbolIsBannedRule, symbol));
                }

                bool shouldReportNotSpecifiedEnforceAnalyzerBannedApisSetting(AttributeData attributeData)
                {
                    if (!attributeData.AttributeClass.Equals(diagnosticAnalyzerAttributeType, SymbolEqualityComparer.Default)
                        && !attributeData.AttributeClass.Equals(generatorAttributeType, SymbolEqualityComparer.Default))
                    {
                        return false;
                    }

                    var treeOptions = symbolAnalysisContext.Options.AnalyzerConfigOptionsProvider.GetOptions(attributeData.ApplicationSyntaxReference.SyntaxTree);
                    var categorizedTreeOptions = SyntaxTreeCategorizedAnalyzerConfigOptions.Create(treeOptions);
                    var enforceBannedApisIsSpecified = categorizedTreeOptions.TryGetOptionValue(
                        EditorConfigOptionNames.EnforceExtendedAnalyzerRules,
                        OptionKind.DotnetCodeQuality,
                        rule: null,
                        tryParseValue: bool.TryParse,
                        false,
                        out _);
                    return !enforceBannedApisIsSpecified;
                }
            }
        }
    }
}