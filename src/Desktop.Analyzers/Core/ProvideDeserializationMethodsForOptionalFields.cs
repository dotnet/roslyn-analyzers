// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{
    /// <summary>
    /// CA2239: Provide deserialization methods for optional fields
    /// </summary>
    public abstract class ProvideDeserializationMethodsForOptionalFieldsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2239";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageOnDeserialized = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsMessageOnDeserialized), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOnDeserializing = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsMessageOnDeserializing), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ProvideDeserializationMethodsForOptionalFieldsDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        internal static DiagnosticDescriptor OnDeserializedRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageOnDeserialized,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor OnDeserializingRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageOnDeserializing,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(OnDeserializedRule, OnDeserializingRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}