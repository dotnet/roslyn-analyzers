// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA2240: Implement ISerializable correctly
    /// </summary>
    public abstract class ImplementISerializableCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2240";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementISerializableCorrectlyTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementISerializableCorrectlyMessageDefault), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeVisible = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementISerializableCorrectlyMessageMakeVisible), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeOverridable = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementISerializableCorrectlyMessageMakeOverridable), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ImplementISerializableCorrectlyDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MakeVisibleRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeVisible,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MakeOverridableRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeOverridable,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, MakeVisibleRule, MakeOverridableRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}