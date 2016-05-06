// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Composition.Analyzers
{
    /// <summary>
    /// RS0023: Parts exported with MEFv2 must be marked as Shared
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0023";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCompositionAnalyzersResources.PartsExportedWithMEFv2MustBeMarkedAsSharedTitle), MicrosoftCompositionAnalyzersResources.ResourceManager, typeof(MicrosoftCompositionAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCompositionAnalyzersResources.PartsExportedWithMEFv2MustBeMarkedAsSharedMessage), MicrosoftCompositionAnalyzersResources.ResourceManager, typeof(MicrosoftCompositionAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCompositionAnalyzersResources.PartsExportedWithMEFv2MustBeMarkedAsSharedDescription), MicrosoftCompositionAnalyzersResources.ResourceManager, typeof(MicrosoftCompositionAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var exportAttribute = compilationContext.Compilation.GetTypeByMetadataName("System.Composition.ExportAttribute");

                if (exportAttribute == null)
                {
                    // We don't need to check assemblies unless they're referencing both MEFv2, so we're done
                    return;
                }

                compilationContext.RegisterSymbolAction(symbolContext =>
                {
                    var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                    var namedTypeAttributes = namedType.GetApplicableAttributes();

                    var exportAttributeApplication = namedTypeAttributes.FirstOrDefault(ad => ad.AttributeClass.DerivesFrom(exportAttribute));

                    if (exportAttributeApplication != null)
                    {
                        if (!namedTypeAttributes.Any(ad => ad.AttributeClass.Name == "SharedAttribute" &&
                                                           ad.AttributeClass.ContainingNamespace.Equals(exportAttribute.ContainingNamespace)))
                        {
                            // '{0}' is exported with MEFv2 and hence must be marked as Shared
                            symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, exportAttributeApplication.ApplicationSyntaxReference.GetSyntax().GetLocation(), namedType.Name));
                        }
                    }
                }, SymbolKind.NamedType);
            });
        }
    }
}