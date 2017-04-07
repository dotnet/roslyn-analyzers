// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Composition.Analyzers
{
    /// <summary>
    /// RS0006: Do not mix attributes from different versions of MEF
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0006";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCompositionAnalyzersResources.DoNotMixAttributesFromDifferentVersionsOfMEFTitle), MicrosoftCompositionAnalyzersResources.ResourceManager, typeof(MicrosoftCompositionAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCompositionAnalyzersResources.DoNotMixAttributesFromDifferentVersionsOfMEFMessage), MicrosoftCompositionAnalyzersResources.ResourceManager, typeof(MicrosoftCompositionAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCompositionAnalyzersResources.DoNotMixAttributesFromDifferentVersionsOfMEFDescription), MicrosoftCompositionAnalyzersResources.ResourceManager, typeof(MicrosoftCompositionAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly string[] s_mefNamespaces = new[] { "System.ComponentModel.Composition", "System.Composition" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var exportAttributes = new List<INamedTypeSymbol>();

                foreach (var mefNamespace in s_mefNamespaces)
                {
                    var exportAttribute = compilationContext.Compilation.GetTypeByMetadataName(mefNamespace + ".ExportAttribute");

                    if (exportAttribute == null)
                    {
                        // We don't need to check assemblies unless they're referencing both versions of MEF, so we're done
                        return;
                    }

                    exportAttributes.Add(exportAttribute);
                }

                compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, exportAttributes), SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext symbolContext, IEnumerable<INamedTypeSymbol> exportAttributes)
        {
            var namedType = (INamedTypeSymbol)symbolContext.Symbol;
            var namedTypeAttributes = namedType.GetApplicableAttributes();

            // Figure out which export attributes are being used here
            var appliedExportAttributes = exportAttributes.Where(e => namedTypeAttributes.Any(ad => ad.AttributeClass.DerivesFrom(e))).ToList();

            // If we have no exports we're done
            if (appliedExportAttributes.Count == 0)
            {
                return;
            }

            var badNamespaces = exportAttributes.Except(appliedExportAttributes).Select(s => s.ContainingNamespace).ToSet();

            // Now look at all attributes and see if any are metadata attributes
            foreach (var namedTypeAttribute in namedTypeAttributes)
            {
                if (namedTypeAttribute.AttributeClass.GetApplicableAttributes().Any(ad => badNamespaces.Contains(ad.AttributeClass.ContainingNamespace) &&
                                                                                                          ad.AttributeClass.Name == "MetadataAttributeAttribute"))
                {
                    ReportDiagnostic(symbolContext, namedType, namedTypeAttribute);
                }
            }

            // Also look through all members and their attributes, and see if any are using from bad places
            foreach (var member in namedType.GetMembers())
            {
                foreach (var attribute in member.GetAttributes())
                {
                    if (badNamespaces.Contains(attribute.AttributeClass.ContainingNamespace))
                    {
                        ReportDiagnostic(symbolContext, namedType, attribute);
                    }
                }

                // if it's a constructor, we should also check parameters since they may have [ImportMany]

                if (member is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Constructor)
                {
                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        foreach (var attribute in parameter.GetAttributes())
                        {
                            if (badNamespaces.Contains(attribute.AttributeClass.ContainingNamespace))
                            {
                                ReportDiagnostic(symbolContext, namedType, attribute);
                            }
                        }
                    }
                }
            }
        }

        private static void ReportDiagnostic(SymbolAnalysisContext symbolContext, INamedTypeSymbol exportedType, AttributeData problematicAttribute)
        {
            // Attribute '{0}' comes from a different version of MEF than the export attribute on '{1}'
            var diagnostic = Diagnostic.Create(Rule, problematicAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation(), problematicAttribute.AttributeClass.Name, exportedType.Name);
            symbolContext.ReportDiagnostic(diagnostic);
        }
    }
}