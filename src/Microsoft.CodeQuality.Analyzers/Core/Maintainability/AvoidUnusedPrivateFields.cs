// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    /// <summary>
    /// CA1823: Avoid unused private fields
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidUnusedPrivateFieldsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1823";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUnusedPrivateFieldsTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUnusedPrivateFieldsMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidUnusedPrivateFieldsDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessage,
                                                                                      DiagnosticCategory.Performance,
                                                                                      DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                      isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                                      description: s_localizableDescription,
                                                                                      helpLinkUri: "http://msdn.microsoft.com/library/ms245042.aspx",
                                                                                      customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // TODO: Make analyzer thread safe
            //analysisContext.EnableConcurrentExecution();

            // We need to analyze generated code, but don't intend to report diagnostics for generated code fields.
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            analysisContext.RegisterCompilationStartAction(
                (compilationContext) =>
                {
                    HashSet<IFieldSymbol> unreferencedPrivateFields = new HashSet<IFieldSymbol>();
                    HashSet<IFieldSymbol> referencedPrivateFields = new HashSet<IFieldSymbol>();

                    // Fields with certain special attributes should never be considered unused.
                    ImmutableHashSet<INamedTypeSymbol> specialAttributes = GetSpecialAttributes(compilationContext.Compilation);
                    
                    compilationContext.RegisterSymbolAction(
                        (symbolContext) =>
                        {
                            IFieldSymbol field = (IFieldSymbol)symbolContext.Symbol;
                            if (field.DeclaredAccessibility == Accessibility.Private && !referencedPrivateFields.Contains(field))
                            {
                                if (!specialAttributes.IsEmpty)
                                {
                                    var fieldAttributes = field.GetAttributes();
                                    foreach (var attribute in field.GetAttributes())
                                    {
                                        if (specialAttributes.Contains(attribute.AttributeClass.OriginalDefinition))
                                        {
                                            return;
                                        }
                                    }
                                }

                                unreferencedPrivateFields.Add(field);
                            }
                        },
                        SymbolKind.Field);

                    compilationContext.RegisterOperationAction(
                        (operationContext) =>
                        {
                            IFieldSymbol field = ((IFieldReferenceOperation)operationContext.Operation).Field;
                            if (field.DeclaredAccessibility == Accessibility.Private)
                            {
                                referencedPrivateFields.Add(field);
                                unreferencedPrivateFields.Remove(field);
                            }
                        },
                        OperationKind.FieldReference);

                    compilationContext.RegisterCompilationEndAction(
                        (compilationEndContext) =>
                        {
                            foreach (IFieldSymbol unreferencedPrivateField in unreferencedPrivateFields)
                            {
                                compilationEndContext.ReportDiagnostic(Diagnostic.Create(Rule, unreferencedPrivateField.Locations[0], unreferencedPrivateField.Name));
                            }
                        });
                });
        }

        private static ImmutableHashSet<INamedTypeSymbol> GetSpecialAttributes(Compilation compilation)
        {
            var specialAttributes = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>();

            var fieldOffsetAttribute = WellKnownTypes.FieldOffsetAttribute(compilation);
            if (fieldOffsetAttribute != null)
            {
                specialAttributes.Add(fieldOffsetAttribute);
            }

            var mefV1Attribute = WellKnownTypes.MEFV1ExportAttribute(compilation);
            if (mefV1Attribute != null)
            {
                specialAttributes.Add(mefV1Attribute);
            }

            var mefV2Attribute = WellKnownTypes.MEFV2ExportAttribute(compilation);
            if (mefV2Attribute != null)
            {
                specialAttributes.Add(mefV2Attribute);
            }

            return specialAttributes.ToImmutable();
        }
    }
}