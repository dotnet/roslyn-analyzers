// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA2211: Non-constant fields should not be visible
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class NonConstantFieldsShouldNotBeVisibleAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2211";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.NonConstantFieldsShouldNotBeVisibleTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.NonConstantFieldsShouldNotBeVisibleMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.NonConstantFieldsShouldNotBeVisibleDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2211-non-constant-fields-should-not-be-visible",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(obj =>
            {
                var field = (IFieldSymbol)obj.Symbol;

                // Only report diagnostic on externally visible non-readonly static fields
                if (field.IsExternallyVisible() && !field.IsConst && field.IsStatic && !field.IsReadOnly)
                {
                    obj.ReportDiagnostic(field.CreateDiagnostic(Rule));
                }
            }, SymbolKind.Field);
        }
    }
}
