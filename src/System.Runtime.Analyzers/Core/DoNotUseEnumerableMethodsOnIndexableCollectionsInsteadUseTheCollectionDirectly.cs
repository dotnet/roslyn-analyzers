// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// RS0014: Do not use Enumerable methods on indexable collections. Instead use the collection directly
    /// </summary>
    public abstract class DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0014";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotUseEnumerableMethodsOnIndexableCollectionsInsteadUseTheCollectionDirectlyDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

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