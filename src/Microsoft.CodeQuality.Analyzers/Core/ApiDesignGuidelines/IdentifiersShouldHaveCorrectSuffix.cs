// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1710: Identifiers should have correct suffix
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldHaveCorrectSuffixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1710";
        internal const string Uri = "https://msdn.microsoft.com/en-us/library/ms182244.aspx";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixMessageDefault), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSpecialCollection = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixMessageSpecialCollection), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectSuffixDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor DefaultRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SpecialCollectionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSpecialCollection,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, SpecialCollectionRule);

        // Tuple says <TypeInheritedOrImplemented, AppropriateSuffix, Bool value saying if the suffix can `Collection` or the `AppropriateSuffix`>s
        // The bool values are as mentioned in the Uri
        private static readonly List<Tuple<string, string, bool>> s_baseTypesAndTheirSuffix = new List<Tuple<string, string, bool>>()
                                                    {
                                                        //Tuple.Create("TypeName", "Suffix", CanSuffixBeCollection)
                                                        Tuple.Create("System.Attribute", "Attribute", false),
                                                        Tuple.Create("System.EventArgs", "EventArgs", false),
                                                        Tuple.Create("System.Exception", "Exception", false),
                                                        Tuple.Create("System.Collections.ICollection", "Collection", false),
                                                        Tuple.Create("System.Collections.IDictionary", "Dictionary", false),
                                                        Tuple.Create("System.Collections.IEnumerable", "Collection", false),
                                                        Tuple.Create("System.Collections.Queue", "Queue", true),
                                                        Tuple.Create("System.Collections.Stack", "Stack", true),
                                                        Tuple.Create("System.Collections.Generic.Queue`1", "Queue", true),
                                                        Tuple.Create("System.Collections.Generic.Stack`1", "Stack", true),
                                                        Tuple.Create("System.Collections.Generic.ICollection`1", "Collection", false),
                                                        Tuple.Create("System.Collections.Generic.IDictionary`2", "Dictionary", false),
                                                        Tuple.Create("System.Data.DataSet", "DataSet", false),
                                                        Tuple.Create("System.Data.DataTable", "DataTable", true),
                                                        Tuple.Create("System.IO.Stream", "Stream", false),
                                                        Tuple.Create("System.Security.IPermission","Permission", false),
                                                        Tuple.Create("System.Security.Policy.IMembershipCondition", "Condition", false)
                                                    };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var baseTypeSuffixMapBuilder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, SuffixInfo>();
            var interfaceTypeSuffixMapBuilder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, SuffixInfo>();

            foreach (var tuple in s_baseTypesAndTheirSuffix)
            {
                var wellKnownNamedType = context.Compilation.GetTypeByMetadataName(tuple.Item1);

                if (wellKnownNamedType != null && wellKnownNamedType.OriginalDefinition != null)
                {
                    // If the type is interface
                    if (wellKnownNamedType.OriginalDefinition.TypeKind == TypeKind.Interface)
                    {
                        interfaceTypeSuffixMapBuilder.Add(wellKnownNamedType.OriginalDefinition, SuffixInfo.Create(tuple.Item2, tuple.Item3));
                    }
                    else
                    {
                        baseTypeSuffixMapBuilder.Add(wellKnownNamedType.OriginalDefinition, SuffixInfo.Create(tuple.Item2, tuple.Item3));
                    }
                }
            }

            if (baseTypeSuffixMapBuilder.Count > 0 || interfaceTypeSuffixMapBuilder.Count > 0)
            {
                var baseTypeSuffixMap = baseTypeSuffixMapBuilder.ToImmutable();
                var interfaceTypeSuffixMap = interfaceTypeSuffixMapBuilder.ToImmutable();
                context.RegisterSymbolAction((saContext) =>
                {
                    var namedTypeSymbol = (INamedTypeSymbol)saContext.Symbol;
                    if (!namedTypeSymbol.IsExternallyVisible())
                    {
                        return;
                    }

                    var baseType = namedTypeSymbol.GetBaseTypes().FirstOrDefault(bt => baseTypeSuffixMap.ContainsKey(bt.OriginalDefinition));
                    if (baseType != null)
                    {
                        var suffixInfo = baseTypeSuffixMap[baseType.OriginalDefinition];

                        // SpecialCollectionRule - Rename 'LastInFirstOut<T>' to end in either 'Collection' or 'Stack'.
                        // DefaultRule - Rename 'MyStringObjectHashtable' to end in 'Dictionary'.
                        var rule = suffixInfo.CanSuffixBeCollection ? SpecialCollectionRule : DefaultRule;
                        if ((suffixInfo.CanSuffixBeCollection && !namedTypeSymbol.Name.EndsWith("Collection", StringComparison.Ordinal) && !namedTypeSymbol.Name.EndsWith(suffixInfo.Suffix, StringComparison.Ordinal)) ||
                            (!suffixInfo.CanSuffixBeCollection && !namedTypeSymbol.Name.EndsWith(suffixInfo.Suffix, StringComparison.Ordinal)))
                        {

                            saContext.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(rule, namedTypeSymbol.ToDisplayString(), suffixInfo.Suffix));
                        }

                        return;
                    }

                    var implementedInterface = namedTypeSymbol.AllInterfaces.FirstOrDefault(i => interfaceTypeSuffixMap.ContainsKey(i.OriginalDefinition));
                    if (implementedInterface != null)
                    {
                        var suffixInfo = interfaceTypeSuffixMap[implementedInterface.OriginalDefinition];
                        if (!namedTypeSymbol.Name.EndsWith(suffixInfo.Suffix, StringComparison.Ordinal))
                        {
                            saContext.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(DefaultRule, namedTypeSymbol.ToDisplayString(), suffixInfo.Suffix));
                        }
                    }
                }
                , SymbolKind.NamedType);

                context.RegisterSymbolAction((saContext) =>
                {
                    const string eventHandlerString = "EventHandler";
                    var eventSymbol = saContext.Symbol as IEventSymbol;
                    if (!eventSymbol.Type.Name.EndsWith(eventHandlerString, StringComparison.Ordinal))
                    {
                        saContext.ReportDiagnostic(eventSymbol.CreateDiagnostic(DefaultRule, eventSymbol.Type.Name, eventHandlerString));
                    }
                },
                SymbolKind.Event);
            }
        }
    }

    class SuffixInfo
    {
        public string Suffix { get; private set; }
        public bool CanSuffixBeCollection { get; private set; }

        private SuffixInfo(
            string suffix,
            bool canSuffixBeCollection)
        {
            Suffix = suffix;
            CanSuffixBeCollection = canSuffixBeCollection;
        }

        internal static SuffixInfo Create(string suffix, bool canSuffixBeCollection)
        {
            return new SuffixInfo(suffix, canSuffixBeCollection);
        }
    }
}