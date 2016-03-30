// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1711: Identifiers should not have incorrect suffix
    /// </summary>
    public abstract class IdentifiersShouldNotHaveIncorrectSuffixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1711";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageTypeNoAlternate = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageTypeNoAlternate), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberNewerVersion = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageMemberNewerVersion), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeNewerVersion = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageTypeNewerVersion), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberWithAlternate = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageMemberWithAlternate), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor TypeNoAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeNoAlternate,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberNewerVersionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberNewerVersion,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor TypeNewerVersionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeNewerVersion,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberWithAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberWithAlternate,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TypeNoAlternateRule, MemberNewerVersionRule, TypeNewerVersionRule, MemberWithAlternateRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
        }
    }
}