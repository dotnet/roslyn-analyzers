// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Microsoft.NetCore.Analyzers.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseDefaultDllImportSearchPathsAttribute : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA5392";
        private static readonly LocalizableString s_Title = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseDefaultDllImportSearchPathsAttribute),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Message = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseDefaultDllImportSearchPathsAttributeMessage),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));
        private static readonly LocalizableString s_Description = new LocalizableResourceString(
            nameof(SystemSecurityCryptographyResources.UseDefaultDllImportSearchPathsAttributeDescription),
            SystemSecurityCryptographyResources.ResourceManager,
            typeof(SystemSecurityCryptographyResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId,
                s_Title,
                s_Message,
                DiagnosticCategory.Security,
                DiagnosticHelpers.DefaultDiagnosticSeverity,
                isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                description: s_Description,
                helpLinkUri: null,
                customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            // Security analyzer - analyze and report diagnostics on generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var compilation = compilationStartAnalysisContext.Compilation;
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilation);

                if (!wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesDllImportAttribute, out INamedTypeSymbol dllImportAttributeTypeSymbol) ||
                    !wellKnownTypeProvider.TryGetTypeByMetadataName(WellKnownTypeNames.SystemRuntimeInteropServicesDefaultDllImportSearchPathsAttribute, out INamedTypeSymbol defaultDllImportSearchPathsAttributeTypeSymbol))
                {
                    return;
                }

                var hasDefaultDllImportSearchPathsAttribute = false;

                if (compilation.Assembly.GetAttributes().Select(o => o.AttributeClass).Contains(defaultDllImportSearchPathsAttributeTypeSymbol))
                {
                    hasDefaultDllImportSearchPathsAttribute = true;
                }

                compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    var symbol = symbolAnalysisContext.Symbol;

                    if (!symbol.IsExtern)
                    {
                        return;
                    }

                    var attributeClasses = symbol.GetAttributes().Select(o => o.AttributeClass);

                    if (attributeClasses.Contains(dllImportAttributeTypeSymbol) ||
                        (!attributeClasses.Contains(defaultDllImportSearchPathsAttributeTypeSymbol) &&
                        !hasDefaultDllImportSearchPathsAttribute))
                    {
                        symbolAnalysisContext.ReportDiagnostic(
                            symbol.CreateDiagnostic(
                                Rule,
                                symbol.Name));
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
