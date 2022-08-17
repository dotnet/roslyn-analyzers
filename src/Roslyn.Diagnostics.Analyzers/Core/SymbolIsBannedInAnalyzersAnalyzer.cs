// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.BannedApiAnalyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.Diagnostics.Analyzers
{
    using static RoslynDiagnosticsAnalyzersResources;

    internal static class SymbolIsBannedInAnalyzersAnalyzer
    {
        public static readonly DiagnosticDescriptor SymbolIsBannedRule = new(
            id: RoslynDiagnosticIds.SymbolIsBannedInAnalyzersRuleId,
            title: CreateLocalizableResourceString(nameof(SymbolIsBannedInAnalyzersTitle)),
            messageFormat: CreateLocalizableResourceString(nameof(SymbolIsBannedInAnalyzersMessage)),
            category: DiagnosticCategory.RoslynDiagnosticsReliability,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(SymbolIsBannedInAnalyzersDescription)),
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);
    }

    public abstract class SymbolIsBannedInAnalyzersAnalyzer<TSyntaxKind> : SymbolIsBannedAnalyzerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SymbolIsBannedRule);

        protected sealed override DiagnosticDescriptor SymbolIsBannedRule { get; } = SymbolIsBannedInAnalyzersAnalyzer.SymbolIsBannedRule;

#pragma warning disable RS1012 // 'compilationContext' does not register any analyzer actions. Consider moving actions registered in 'Initialize' that depend on this start action to 'compilationContext'.
        protected sealed override Dictionary<ISymbol, BanFileEntry>? ReadBannedApis(CompilationStartAnalysisContext compilationContext)
        {
            const string fileName = "Roslyn.Diagnostics.Analyzers.AnalyzerBannedSymbols.txt";
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
        }
    }
}