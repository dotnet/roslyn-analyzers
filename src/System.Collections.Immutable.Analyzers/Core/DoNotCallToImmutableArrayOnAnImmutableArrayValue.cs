// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Collections.Immutable.Analyzers
{
    /// <summary>
    /// RS0012: Do not call ToImmutableArray on an ImmutableArray value
    /// </summary>
    public abstract class DoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0012";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueTitle), SystemCollectionsImmutableAnalyzersResources.ResourceManager, typeof(SystemCollectionsImmutableAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueMessage), SystemCollectionsImmutableAnalyzersResources.ResourceManager, typeof(SystemCollectionsImmutableAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemCollectionsImmutableAnalyzersResources.DoNotCallToImmutableArrayOnAnImmutableArrayValueDescription), SystemCollectionsImmutableAnalyzersResources.ResourceManager, typeof(SystemCollectionsImmutableAnalyzersResources));

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