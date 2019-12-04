// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ImportedMembersShouldNotBeReadOnly : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ImportedMembersShouldNotBeReadOnlyTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ImportedMembersShouldNotBeReadOnlyMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.ImportedMembersShouldNotBeReadOnlyDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.ImportedMembersShouldNotBeReadOnlyRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslynDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var importAttributeV1 = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemComponentModelCompositionImportAttribute);
                var importManyAttributeV1 = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemComponentModelCompositionImportManyAttribute);
                var importAttributeV2 = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCompositionImportAttribute);
                var importManyAttributeV2 = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCompositionImportManyAttribute);

                if (importAttributeV1 is null && importAttributeV2 is null)
                {
                    // We don't need to check assemblies unless they're referencing MEF, so we're done
                    return;
                }

                context.RegisterSymbolAction(context =>
                {
                    var isReadOnly = context.Symbol switch
                    {
                    IFieldSymbol fieldSymbol => fieldSymbol.IsReadOnly,
                        IPropertySymbol propertySymbol => propertySymbol.IsReadOnly,
                        _ => false,
                    };

                    if (!isReadOnly)
                    {
                        return;
                    }

                    foreach (var attributeData in context.Symbol.GetAttributes())
                    {
                        if (attributeData.AttributeClass.Inherits(importAttributeV1)
                            || attributeData.AttributeClass.Inherits(importManyAttributeV1)
                            || attributeData.AttributeClass.Inherits(importAttributeV2)
                            || attributeData.AttributeClass.Inherits(importManyAttributeV2))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, attributeData.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                        }
                    }
                }, SymbolKind.Field, SymbolKind.Property);
            });
        }
    }
}
