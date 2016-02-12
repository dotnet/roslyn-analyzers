// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Microsoft.Composition.Analyzers
{
    /// <summary>
    /// RS0023: Parts exported with MEFv2 must be marked as Shared
    /// </summary>
    public abstract class PartsExportedWithMEFv2MustBeMarkedAsSharedAnalyzer : DiagnosticAnalyzer
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