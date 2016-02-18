// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1308: Normalize strings to uppercase
    /// </summary>
    public abstract class NormalizeStringsToUppercaseAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1308";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageToUpperInvariant = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseMessageToUpperInvariant), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageToUpper = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseMessageToUpper), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.NormalizeStringsToUppercaseDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor ToUpperInvariantRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageToUpperInvariant,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor ToUpperRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageToUpper,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ToUpperInvariantRule, ToUpperRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}