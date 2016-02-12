// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1710: Identifiers should have correct suffix
    /// </summary>
    public abstract class IdentifiersShouldHaveCorrectSuffixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1710";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixMessageDefault), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSpecialCollection = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixMessageSpecialCollection), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SpecialCollectionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSpecialCollection,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, SpecialCollectionRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}