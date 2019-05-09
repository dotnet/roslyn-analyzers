// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class LinkedEnumerationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableMissingMembersTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldHaveAllMembersTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMissingMembersMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldHaveAllMembersMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMissingMembersDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldHaveAllMembersDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor MissingMembersRule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.LinkedEnumerationShouldHaveAllMembersRuleId,
            s_localizableMissingMembersTitle,
            s_localizableMissingMembersMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableMissingMembersDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly LocalizableString s_localizableMismatchedValueTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchValueTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMismatchedValueMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchValueMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMismatchedValueDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchValueDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor MismatchedValueRule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.LinkedEnumerationShouldMatchValueRuleId,
            s_localizableMismatchedValueTitle,
            s_localizableMismatchedValueMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableMismatchedValueDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly LocalizableString s_localizableMismatchedNameTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchNameTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMismatchedNameMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchNameMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMismatchedNameDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.LinkedEnumerationShouldMatchNameDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor MismatchedNameRule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.LinkedEnumerationShouldMatchNameRuleId,
            s_localizableMismatchedNameTitle,
            s_localizableMismatchedNameMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableMismatchedNameDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingMembersRule, MismatchedValueRule, MismatchedNameRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var linkedEnumerationAttribute = compilationContext.Compilation.GetTypeByMetadataName("Roslyn.Utilities.LinkedEnumerationAttribute");
                var attributeUsageAttribute = WellKnownTypes.AttributeUsageAttribute(compilationContext.Compilation);
                if (linkedEnumerationAttribute is null)
                {
                    // We don't need to check assemblies unless they're referencing Roslyn, so we're done
                    return;
                }

                compilationContext.RegisterSymbolAction(symbolContext =>
                {
                    var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                    if (namedType.TypeKind != TypeKind.Enum)
                    {
                        return;
                    }

                    var namedTypeAttributes = namedType.GetApplicableAttributes(attributeUsageAttribute);
                    var sourceEnumeration = TryGetLinkedEnumerationType(namedTypeAttributes, linkedEnumerationAttribute);
                    if (sourceEnumeration is null)
                    {
                        return;
                    }

                    AnalyzeLinkedEnumeration(ref symbolContext, sourceEnumeration);
                }, SymbolKind.NamedType);
            });
        }

        private void AnalyzeLinkedEnumeration(ref SymbolAnalysisContext symbolContext, INamedTypeSymbol sourceEnumeration)
        {
            if (sourceEnumeration.TypeKind != TypeKind.Enum)
            {
                return;
            }

            var sourceEnumMembers = new Dictionary<string, object>();
            foreach (var enumMember in sourceEnumeration.GetMembers())
            {
                if (!(enumMember is IFieldSymbol field))
                {
                    continue;
                }

                sourceEnumMembers[field.Name] = field.ConstantValue;
            }

            var matchingCount = 0;
            var enumType = (ITypeSymbol)symbolContext.Symbol;
            foreach (var enumMember in enumType.GetMembers())
            {
                if (!(enumMember is IFieldSymbol field))
                {
                    continue;
                }

                if (sourceEnumMembers.TryGetValue(enumMember.Name, out var sourceValue))
                {
                    matchingCount++;
                }
                else
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(MismatchedNameRule, enumMember.Locations.First()));
                    continue;
                }

                if (!Equals(field.ConstantValue, sourceValue))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(MismatchedValueRule, enumMember.Locations.First()));
                }
            }

            if (matchingCount != sourceEnumMembers.Count)
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(MissingMembersRule, enumType.Locations.First()));
            }
        }

        internal static INamedTypeSymbol TryGetLinkedEnumerationType(IEnumerable<AttributeData> namedTypeAttributes, INamedTypeSymbol linkedEnumerationAttribute)
        {
            foreach (var attributeData in namedTypeAttributes)
            {
                if (!Equals(attributeData.AttributeClass, linkedEnumerationAttribute))
                {
                    continue;
                }

                if (attributeData.ConstructorArguments.Length != 1)
                {
                    continue;
                }

                var sourceEnumerationConstant = attributeData.ConstructorArguments[0];
                if (sourceEnumerationConstant.Kind != TypedConstantKind.Type)
                {
                    continue;
                }

                return sourceEnumerationConstant.Value as INamedTypeSymbol;
            }

            return null;
        }
    }
}
