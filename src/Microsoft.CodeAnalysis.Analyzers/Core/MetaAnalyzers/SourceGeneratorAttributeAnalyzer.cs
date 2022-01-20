// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using static Microsoft.CodeAnalysis.Analyzers.CodeAnalysisDiagnosticsResources;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class SourceGeneratorAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor DiagnosticRule = new(
            DiagnosticIds.MissingSourceGeneratorAttributeId,
            CreateLocalizableResourceString(nameof(MissingSourceGeneratorAttributeTitle)),
            CreateLocalizableResourceString(nameof(MissingSourceGeneratorAttributeTitle)),
            DiagnosticCategory.MicrosoftCodeAnalysisCorrectness,
            DiagnosticSeverity.Warning,
            description: CreateLocalizableResourceString(nameof(MissingSourceGeneratorAttributeDescription)),
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(context =>
            {
                if (context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisISourceGenerator, out var sourceGenerator) &&
                    context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisGeneratorAttribute, out var generatorAttribute))
                {
                    context.RegisterSymbolAction(c => AnalyzeSymbol(c, sourceGenerator, generatorAttribute), SymbolKind.NamedType);
                }
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext c, INamedTypeSymbol sourceGenerator, INamedTypeSymbol generatorAttribute)
        {
            var symbol = (INamedTypeSymbol)c.Symbol;

            if (symbol.IsAbstract || symbol.IsAnonymousType)
            {
                return;
            }

            if (!symbol.AllInterfaces.Contains(sourceGenerator))
            {
                return;
            }

            if (symbol.GetApplicableAttributes(null).Any(a => a.AttributeClass.Equals(generatorAttribute, SymbolEqualityComparer.Default)))
            {
                return;
            }

            c.ReportDiagnostic(symbol.CreateDiagnostic(DiagnosticRule, symbol.Name));
        }
    }
}
