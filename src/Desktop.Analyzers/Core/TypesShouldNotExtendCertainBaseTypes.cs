// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemApplicationExceptionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemApplicationException,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsCollectionBaseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsCollectionBase,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsDictionaryBaseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsDictionaryBase,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsQueueRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsQueue,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsReadOnlyCollectionBaseRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsReadOnlyCollectionBase,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsSortedListRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsSortedList,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SystemCollectionsStackRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSystemCollectionsStack,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SystemXmlXmlDocumentRule, SystemApplicationExceptionRule, SystemCollectionsCollectionBaseRule, SystemCollectionsDictionaryBaseRule, SystemCollectionsQueueRule, SystemCollectionsReadOnlyCollectionBaseRule, SystemCollectionsSortedListRule, SystemCollectionsStackRule);

        private static readonly Dictionary<string, DiagnosticDescriptor> s_badBaseTypesToDescriptor = new Dictionary<string, DiagnosticDescriptor>
                                                    {
                                                        {"System.ApplicationException", SystemApplicationExceptionRule},
                                                        {"System.Xml.XmlDocument", SystemXmlXmlDocumentRule},
                                                        {"System.Collections.CollectionBase", SystemCollectionsCollectionBaseRule},
                                                        {"System.Collections.DictionaryBase", SystemCollectionsDictionaryBaseRule},
                                                        {"System.Collections.Queue", SystemCollectionsQueueRule},
                                                        {"System.Collections.ReadOnlyCollectionBase", SystemCollectionsReadOnlyCollectionBaseRule},
                                                        {"System.Collections.SortedList", SystemCollectionsSortedListRule},
                                                        {"System.Collections.Stack", SystemCollectionsStackRule},
                                                    };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var badBaseTypes = s_badBaseTypesToDescriptor.Keys
                                .Select(bt => context.Compilation.GetTypeByMetadataName(bt))
                                .Where(bt => bt != null)
                                .ToList();

            context.RegisterSymbolAction((saContext) =>
                {
                    var namedTypeSymbol = saContext.Symbol as INamedTypeSymbol;
                    if (!namedTypeSymbol.Locations.Any(l => l.IsInSource))
                    {
                        return;
                    }

                    var containedBadBaseTypes = badBaseTypes.Where(bbt => bbt.Equals(namedTypeSymbol.BaseType));

                    if (containedBadBaseTypes.Any())
                    {
                        Debug.Assert(s_badBaseTypesToDescriptor.ContainsKey(namedTypeSymbol.BaseType.ToDisplayString()));
                        saContext.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(s_badBaseTypesToDescriptor[namedTypeSymbol.BaseType.ToDisplayString()], namedTypeSymbol.ToDisplayString(), containedBadBaseTypes.First().ToDisplayString()));
                    }
                }
                , SymbolKind.NamedType);

        }
    }
}