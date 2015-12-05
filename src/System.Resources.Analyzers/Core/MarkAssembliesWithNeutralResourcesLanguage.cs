// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Resources.Analyzers
{                   
    /// <summary>
    /// CA1824: Mark assemblies with NeutralResourcesLanguageAttribute
    /// </summary>
    public abstract class MarkAssembliesWithNeutralResourcesLanguageAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1824";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemResourcesAnalyzersResources.MarkAssembliesWithNeutralResourcesLanguageTitle), SystemResourcesAnalyzersResources.ResourceManager, typeof(SystemResourcesAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemResourcesAnalyzersResources.MarkAssembliesWithNeutralResourcesLanguageMessage), SystemResourcesAnalyzersResources.ResourceManager, typeof(SystemResourcesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemResourcesAnalyzersResources.MarkAssembliesWithNeutralResourcesLanguageDescription), SystemResourcesAnalyzersResources.ResourceManager, typeof(SystemResourcesAnalyzersResources));
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            
        }
    }
}