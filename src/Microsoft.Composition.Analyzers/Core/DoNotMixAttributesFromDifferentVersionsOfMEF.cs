// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Microsoft.Composition.Analyzers
{
    /// <summary>
    /// RS0006: Do not mix attributes from different versions of MEF
    /// </summary>
    public abstract class DoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer : DiagnosticAnalyzer
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