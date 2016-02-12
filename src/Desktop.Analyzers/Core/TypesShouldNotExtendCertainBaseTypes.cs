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
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class TypesShouldNotExtendCertainBaseTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1058";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             "{0}",
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182171.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly Dictionary<string, string> s_badBaseTypesToMessage = new Dictionary<string, string>
                                                    {
                                                        {"System.ApplicationException", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemApplicationException},
                                                        {"System.Xml.XmlDocument", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemXmlXmlDocument},
                                                        {"System.Collections.CollectionBase", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsCollectionBase},
                                                        {"System.Collections.DictionaryBase", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsDictionaryBase},
                                                        {"System.Collections.Queue", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsQueue},
                                                        {"System.Collections.ReadOnlyCollectionBase", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsReadOnlyCollectionBase},
                                                        {"System.Collections.SortedList", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsSortedList},
                                                        {"System.Collections.Stack", DesktopAnalyzersResources.TypesShouldNotExtendCertainBaseTypesMessageSystemCollectionsStack},
                                                    };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            ImmutableHashSet<INamedTypeSymbol> badBaseTypes = s_badBaseTypesToMessage.Keys
                                .Select(bt => context.Compilation.GetTypeByMetadataName(bt))
                                .Where(bt => bt != null)
                                .ToImmutableHashSet();

            if (badBaseTypes.Count > 0)
            {
                context.RegisterSymbolAction((saContext) =>
                    {
                        var namedTypeSymbol = saContext.Symbol as INamedTypeSymbol;

                        if (badBaseTypes.Contains(namedTypeSymbol.BaseType))
                        {
                            string baseTypeName = namedTypeSymbol.BaseType.ToDisplayString();
                            Debug.Assert(s_badBaseTypesToMessage.ContainsKey(baseTypeName));
                            string message = string.Format(s_badBaseTypesToMessage[baseTypeName], namedTypeSymbol.ToDisplayString(), baseTypeName);
                            Diagnostic diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations.First(), namedTypeSymbol.Locations.Skip(1), message);
                            saContext.ReportDiagnostic(diagnostic);
                        }
                    }
                    , SymbolKind.NamedType);
            }
        }
    }
}