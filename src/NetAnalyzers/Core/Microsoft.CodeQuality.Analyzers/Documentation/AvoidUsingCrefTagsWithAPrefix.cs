// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Documentation
{
    /// <summary>
    /// RS0010: Avoid using cref tags with a prefix
    /// </summary>
    public abstract class AvoidUsingCrefTagsWithAPrefixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1200";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidUsingCrefTagsWithAPrefixTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidUsingCrefTagsWithAPrefixMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AvoidUsingCrefTagsWithAPrefixDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Documentation,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected static void ProcessAttribute(SyntaxNodeAnalysisContext context, SyntaxTokenList textTokens)
        {
            if (!textTokens.Any())
            {
                return;
            }

            var token = textTokens.First();

            if (token.Span.Length >= 2)
            {
                var text = token.Text;

                if (text[1] == ':')
                {
                    var location = Location.Create(token.SyntaxTree, textTokens.Span);
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, text.Substring(0, 2)));
                }
            }
        }
    }
}