// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Desktop.Analyzers
{                   
    /// <summary>
    /// CA3065: Review DataView CollectionString
    /// </summary>
    public abstract class ReviewDataViewCollectionStringAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3065";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ReviewDataViewCollectionStringTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ReviewDataViewCollectionStringMessage), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.ReviewDataViewCollectionStringDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.NoCetegory,
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