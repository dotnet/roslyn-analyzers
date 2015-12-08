// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1721: Property names should not match get methods
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PropertyNamesShouldNotMatchGetMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1721";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDeclaringTypeMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessageSameType), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableBaseTypeMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessageBaseType), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor DeclaringTypeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableDeclaringTypeMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182253.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor BaseTypeRule = new DiagnosticDescriptor(RuleId,
                                                                     s_localizableTitle,
                                                                     s_localizableBaseTypeMessage,
                                                                     DiagnosticCategory.Design,
                                                                     DiagnosticSeverity.Warning,
                                                                     isEnabledByDefault: true,
                                                                     description: s_localizableDescription,
                                                                     helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182253.aspx",
                                                                     customTags: WellKnownDiagnosticTags.Telemetry);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DeclaringTypeRule, BaseTypeRule);

        internal const string s_get = "Get";

        public override void Initialize(AnalysisContext analysisContext)
        {
            // Analyze properties, methods 
            analysisContext.RegisterSymbolAction(symbolContext =>
            {
                AnalyzeSymbol(symbolContext.Symbol, symbolContext);

            }, SymbolKind.Property, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
        {
            string identifier;

            if (symbol.Kind == SymbolKind.Property)
            {
                //if property then target search is to find methods that start with Get and have the property name
                identifier = s_get + symbol.Name;
            }
            else if (symbol.Kind == SymbolKind.Method && symbol.Name.StartsWith(s_get))
            {
                //if method starts with Get then target search is to find properties that have the method name sans Get
                identifier = symbol.Name.Substring(3);
            }
            else
            {
                //if method name doesn't start with Get exit
                return;
            }

            //boolean variable used to exit out of the inner and outer for loops
            var matchFound = false;

            //get the collection of the containing type plus all the derived types
            var types = symbol.ContainingType.GetBaseTypesAndThis();


            foreach (var type in types)
            {
                var membersFound = type.GetMembers(identifier);
                if (membersFound != null && membersFound.Length > 0)
                {
                    foreach (var member in membersFound)
                    {
                        if ((symbol.Kind == SymbolKind.Property && member.Kind == SymbolKind.Method) ||
                            (symbol.Kind == SymbolKind.Method && member.Kind == SymbolKind.Property && symbol.ContainingType != type))
                        {
                            matchFound = true;
                            break;
                        }
                    }
                    if (!matchFound)
                        continue;

                    Diagnostic diagnostic;
                    if (symbol.ContainingType != type)
                    {
                        diagnostic = Diagnostic.Create(BaseTypeRule, symbol.Locations[0], symbol.Name, type.Name);
                    }
                    else
                    {
                        diagnostic = Diagnostic.Create(DeclaringTypeRule, symbol.Locations[0], symbol.Name, type.Name);
                    }
                    context.ReportDiagnostic(diagnostic);
                    break;
                }
            }
        }
    }
}