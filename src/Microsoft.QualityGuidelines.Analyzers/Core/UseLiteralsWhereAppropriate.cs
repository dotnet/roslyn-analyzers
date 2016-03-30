// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.QualityGuidelines.Analyzers
{
    /// <summary>
    /// CA1802: Use literals where appropriate
    /// </summary>
    public abstract class UseLiteralsWhereAppropriateAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1802";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateTitle), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateMessageDefault), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageEmptyString = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateMessageEmptyString), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.UseLiteralsWhereAppropriateDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor EmptyStringRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageEmptyString,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, EmptyStringRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}