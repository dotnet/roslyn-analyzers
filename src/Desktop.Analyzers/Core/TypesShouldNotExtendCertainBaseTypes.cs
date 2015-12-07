// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Desktop.Analyzers
{                   
    /// <summary>
    /// CA1058: Types should not extend certain base types
    /// </summary>
    public abstract class TypesShouldNotExtendCertainBaseTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1058";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessageSystemXmlXmlDocument = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemXmlXmlDocument), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemApplicationException = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemApplicationException), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemCollectionsCollectionBase = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsCollectionBase), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemCollectionsDictionaryBase = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsDictionaryBase), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemCollectionsQueue = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsQueue), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemCollectionsReadOnlyCollectionBase = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsReadOnlyCollectionBase), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemCollectionsSortedList = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsSortedList), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSystemCollectionsStack = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsStack), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        
        internal static DiagnosticDescriptor SystemXmlXmlDocumentRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemXmlXmlDocument,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemApplicationExceptionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemApplicationException,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsCollectionBaseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsCollectionBase,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsDictionaryBaseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsDictionaryBase,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsQueueRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsQueue,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsReadOnlyCollectionBaseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsReadOnlyCollectionBase,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsSortedListRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsSortedList,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsStackRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsStack,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SystemXmlXmlDocumentRule, SystemApplicationExceptionRule, SystemCollectionsCollectionBaseRule, SystemCollectionsDictionaryBaseRule, SystemCollectionsQueueRule, SystemCollectionsReadOnlyCollectionBaseRule, SystemCollectionsSortedListRule, SystemCollectionsStackRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            
        }
    }
}