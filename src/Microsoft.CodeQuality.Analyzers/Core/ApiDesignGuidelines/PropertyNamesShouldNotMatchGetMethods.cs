// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1721: Property names should not match get methods
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PropertyNamesShouldNotMatchGetMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1721";
        internal const string Get = "Get";

        private static readonly LocalizableString LocalizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString LocalizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString LocalizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertyNamesShouldNotMatchGetMethodsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             LocalizableTitle,
                                                                             LocalizableMessage,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: LocalizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182253.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Analyze properties, methods 
            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            string identifier;
            var symbol = context.Symbol;

            // Bail out if the method/property is not protected or public
            if (!(symbol.DeclaredAccessibility == Accessibility.Protected ||
                  symbol.DeclaredAccessibility == Accessibility.Public))
            {
                return;
            }

            if (symbol.Kind == SymbolKind.Property)
            {
                // Want to look for methods named the same as the property with a 'Get' prefix
                identifier = Get + symbol.Name;
            }
            else if (symbol.Kind == SymbolKind.Method && symbol.Name.StartsWith(Get, StringComparison.Ordinal))
            {
                // Want to look for properties named the same as the method sans 'Get'
                identifier = symbol.Name.Substring(3);
            }
            else
            {
                // Exit if the method name doesn't start with 'Get'
                return;
            }

            // Iterate through all declared types, including base
            foreach (INamedTypeSymbol type in symbol.ContainingType.GetBaseTypesAndThis())
            {                
                Diagnostic diagnostic = null;

                // We only want to check against protected or public methods/properties
                var publicMembers = type.GetMembers(identifier).Where(member => 
                    member.DeclaredAccessibility == Accessibility.Protected || 
                    member.DeclaredAccessibility == Accessibility.Public);

                foreach (ISymbol member in publicMembers)
                {
                    // If the declared type is a property, was a matching method found?
                    if (symbol.Kind == SymbolKind.Property && member.Kind == SymbolKind.Method)
                    {
                        diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, identifier);
                        break;
                    }

                    // If the declared type is a method, was a matching property found? Although this
                    // check seems redundant, it's the only way to catch violations of this rule when the
                    // method is declared in a more derived implementation.
                    if (symbol.Kind == SymbolKind.Method && 
                        member.Kind == SymbolKind.Property &&
                        !symbol.ContainingType.Equals(type))
                    {
                        diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], identifier, symbol.Name);
                        break;
                    }
                }

                if (diagnostic == null)
                {
                    continue;
                }

                context.ReportDiagnostic(diagnostic);

                // Once a match is found, exit the outer for loop
                break;
            }
        }
    }
}