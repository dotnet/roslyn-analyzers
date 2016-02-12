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
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182253.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
                //if property then target search is to find methods that start with Get and the substring property name
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

            //get the collection of declaring and base types
            System.Collections.Generic.IEnumerable<INamedTypeSymbol> types = symbol.ContainingType.GetBaseTypesAndThis();


            //iterate thru collection to find match
            foreach (INamedTypeSymbol type in types)
            {
                ImmutableArray<ISymbol> membersFound = type.GetMembers(identifier);
                if (membersFound != null && membersFound.Length > 0)
                {
                    //found a match
                    foreach (ISymbol member in membersFound)
                    {
                        //valid matches are...
                        //when property from declaring type matches with method present in declaring type - this is covered by the LHS of OR condition below
                        //when property from declaring type matches with method present in one of the base types - this is covered by the LHS of OR condition below
                        //when method from declaring type matches with property present in one of the base types - this is covered by the RHS of OR condition below
                        if ((symbol.Kind == SymbolKind.Property && member.Kind == SymbolKind.Method) ||
                            (symbol.Kind == SymbolKind.Method && member.Kind == SymbolKind.Property && symbol.ContainingType != type))
                        {
                            //match found and break out of inner for loop
                            matchFound = true;
                            break;
                        }
                    }

                    //if no match found iterate to next in outer for loop
                    if (!matchFound)
                        continue;

                    //Reaches here only if match found. Create diagnostic
                    Diagnostic diagnostic;
                    diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, type.Name);
                    context.ReportDiagnostic(diagnostic);

                    //once a match is found exit the outer for loop
                    break;
                }
            }
        }
    }
}