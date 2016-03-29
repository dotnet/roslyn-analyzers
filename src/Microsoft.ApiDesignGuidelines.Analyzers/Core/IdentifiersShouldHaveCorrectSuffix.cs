// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1710: Identifiers should have correct suffix
    /// </summary>
    public abstract class IdentifiersShouldHaveCorrectSuffixAnalyzer : DiagnosticAnalyzer
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
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor SpecialCollectionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageSpecialCollection,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: Uri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule, SpecialCollectionRule);

        private const string EventHandlerString = "EventHandler";
        private const string CollectionString = "Collection";

        // Tuple says <TypeInheritedOrImplemented, IsInterface, AppropriateSuffix, Bool value saying if the suffix can `Collection` or the `AppropriateSuffix`>s
        private static readonly List<Tuple<string, bool, string, bool>> s_baseTypesAndTheirSuffix = new List<Tuple<string, bool, string, bool>>()
                                                    {
                                                        //Tuple.Create("TypeName", IsInterface, "Suffix", CanSuffixBeCollection)
                                                        Tuple.Create("System.Attribute", false, "Attribute", false),
                                                        Tuple.Create("System.EventArgs", false, "EventArgs", false),
                                                        Tuple.Create("System.Exception", false, "Exception", false),
                                                        Tuple.Create("System.Collections.ICollection", true, "Collection", false),
                                                        Tuple.Create("System.Collections.IDictionary", true, "Dictionary", false),
                                                        Tuple.Create("System.Collections.IEnumerable", true, "Collection", false),
                                                        Tuple.Create("System.Collections.Queue", false, "Queue", true),
                                                        Tuple.Create("System.Collections.Stack", false, "Stack", true),
                                                        Tuple.Create("System.Collections.Generic.Queue`1", false, "Queue", true),
                                                        Tuple.Create("System.Collections.Generic.Stack`1", false, "Stack", true),
                                                        Tuple.Create("System.Collections.Generic.ICollection`1", true,"Collection", false),
                                                        Tuple.Create("System.Collections.Generic.IDictionary`2", true, "Dictionary", false),
                                                        Tuple.Create("System.Data.DataSet", false, "DataSet", false),
                                                        Tuple.Create("System.Data.DataTable", false, "DataTable", true),
                                                        Tuple.Create("System.IO.Stream", false, "Stream", false),
                                                        Tuple.Create("System.Security.IPermission", true,"Permission", false),
                                                        Tuple.Create("System.Security.Policy.IMembershipCondition", true, "Condition", false)
                                                    };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var baseTypeSuffixMap = new Dictionary<INamedTypeSymbol, SuffixInfo>();
            var interfaceTypeSuffixMap = new Dictionary<INamedTypeSymbol, SuffixInfo>();

            foreach (var tuple in s_baseTypesAndTheirSuffix)
            {
                var wellKnownNamedType = context.Compilation.GetTypeByMetadataName(tuple.Item1);

                if (wellKnownNamedType != null && wellKnownNamedType.OriginalDefinition != null)
                {
                    // If the type is interface
                    if (tuple.Item2)
                    {
                        interfaceTypeSuffixMap.Add(wellKnownNamedType.OriginalDefinition, SuffixInfo.Create(tuple.Item3, tuple.Item4));
                    }
                    else
                    {
                        baseTypeSuffixMap.Add(wellKnownNamedType.OriginalDefinition, SuffixInfo.Create(tuple.Item3, tuple.Item4));
                    }
                }
            }

            if (baseTypeSuffixMap.Count > 0 || interfaceTypeSuffixMap.Count > 0)
            {
                context.RegisterSymbolAction((saContext) =>
                {
                    var namedTypeSymbol = saContext.Symbol as INamedTypeSymbol;
                    if (namedTypeSymbol.GetResultantVisibility() != SymbolVisibility.Public)
                    {
                        return;
                    }

                    var baseType = namedTypeSymbol.GetBaseTypes().FirstOrDefault(bt => baseTypeSuffixMap.Keys.Contains(bt.OriginalDefinition));
                    if (baseType != null)
                    {
                        var suffixInfo = baseTypeSuffixMap[baseType.OriginalDefinition];
                        var rule = suffixInfo.CanSuffixBeCollection ? SpecialCollectionRule : DefaultRule;
                        if ((suffixInfo.CanSuffixBeCollection && !namedTypeSymbol.Name.EndsWith(CollectionString) && !namedTypeSymbol.Name.EndsWith(suffixInfo.Suffix)) ||
                            (!suffixInfo.CanSuffixBeCollection && !namedTypeSymbol.Name.EndsWith(suffixInfo.Suffix)))
                        {
                            saContext.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(rule, namedTypeSymbol.ToDisplayString(), suffixInfo.Suffix));
                        }

                        return;
                    }

                    var implementedInterface = namedTypeSymbol.AllInterfaces.FirstOrDefault(i => interfaceTypeSuffixMap.Keys.Contains(i.OriginalDefinition));
                    if (implementedInterface != null)
                    {
                        var suffixInfo = interfaceTypeSuffixMap[implementedInterface.OriginalDefinition];
                        if (!namedTypeSymbol.Name.EndsWith(suffixInfo.Suffix))
                        {
                            saContext.ReportDiagnostic(namedTypeSymbol.CreateDiagnostic(DefaultRule, namedTypeSymbol.ToDisplayString(), suffixInfo.Suffix));
                        }
                    }
                }
                , SymbolKind.NamedType);

                context.RegisterSymbolAction((saContext) =>
                {
                    var eventSymbol = saContext.Symbol as IEventSymbol;
                    if (!eventSymbol.Type.Name.EndsWith(EventHandlerString))
                    {
                        saContext.ReportDiagnostic(eventSymbol.CreateDiagnostic(DefaultRule, eventSymbol.Type.Name, EventHandlerString));
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