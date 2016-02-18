// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Maintainability.Analyzers
{
    /// <summary>
    /// CA1500: Variable names should not match field names
    /// </summary>
    public abstract class VariableNamesShouldNotMatchFieldNamesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1500";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.VariableNamesShouldNotMatchFieldNamesTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageLocal = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.VariableNamesShouldNotMatchFieldNamesMessageLocal), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageParameter = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.VariableNamesShouldNotMatchFieldNamesMessageParameter), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.VariableNamesShouldNotMatchFieldNamesDescription), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor LocalRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageLocal,
                                                                             DiagnosticCategory.Maintainability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageParameter,
                                                                             DiagnosticCategory.Maintainability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(LocalRule, ParameterRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}