// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace XmlDocumentationComments.Analyzers
{
    /// <summary>
    /// RS0010: Avoid using cref tags with a prefix
    /// </summary>
    public abstract class AvoidUsingCrefTagsWithAPrefixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0010";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(XmlDocumentationCommentsAnalyzersResources.AvoidUsingCrefTagsWithAPrefixTitle), XmlDocumentationCommentsAnalyzersResources.ResourceManager, typeof(XmlDocumentationCommentsAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(XmlDocumentationCommentsAnalyzersResources.AvoidUsingCrefTagsWithAPrefixMessage), XmlDocumentationCommentsAnalyzersResources.ResourceManager, typeof(XmlDocumentationCommentsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(XmlDocumentationCommentsAnalyzersResources.AvoidUsingCrefTagsWithAPrefixDescription), XmlDocumentationCommentsAnalyzersResources.ResourceManager, typeof(XmlDocumentationCommentsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Documentation,
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