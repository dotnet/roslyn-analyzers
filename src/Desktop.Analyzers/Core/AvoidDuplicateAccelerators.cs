// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA1301: Avoid duplicate accelerators
    /// </summary>
    public abstract class AvoidDuplicateAcceleratorsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1301";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.AvoidDuplicateAcceleratorsTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(DesktopAnalyzersResources.AvoidDuplicateAcceleratorsMessage), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.AvoidDuplicateAcceleratorsDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Globalization,
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