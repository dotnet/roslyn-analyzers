// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary> 
    /// CA1720-redefined: Identifiers should not contain type names 
    /// Cause: 
    /// The name of a parameter or a member contains a language-specific data type name. 
    ///  
    /// Description: 
    /// Names of parameters and members are better used to communicate their meaning than  
    /// to describe their type, which is expected to be provided by development tools. For names of members,  
    /// if a data type name must be used, use a language-independent name instead of a language-specific one.  
    /// </summary> 

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class IdentifiersShouldNotContainTypeNames : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1720";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotContainTypeNamesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static ImmutableHashSet<string> s_types = ImmutableHashSet.CreateRange<string>(StringComparer.OrdinalIgnoreCase,
            new[]{
                "char",
                "wchar",
                "int8",
                "uint8",
                "short",
                "ushort",
                "int",
                "uint",
                "integer",
                "uinteger",
                "long",
                "ulong",
                "unsigned",
                "signed",
                "float",
                "float32",
                "float64",
                "int16",
                "int32",
                "int64",
                "uint16",
                "uint32",
                "uint64",
                "intptr",
                "uintptr",
                "ptr",
                "uptr",
                "pointer",
                "upointer",
                "single",
                "double",
                "decimal",
                "guid",
                "object",
                "obj",
                "string"
            });

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb531486.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // Analyze named types and fields.
            analysisContext.RegisterSymbolAction(symbolContext =>
            {
                AnalyzeSymbol(symbolContext.Symbol, symbolContext);
            }, SymbolKind.NamedType, SymbolKind.Field);

            // Analyze properties and methods, and their parameters.
            analysisContext.RegisterSymbolAction(symbolContext =>
            {
                AnalyzeSymbol(symbolContext.Symbol, symbolContext);

                ImmutableArray<IParameterSymbol> parameters = symbolContext.Symbol.Kind == SymbolKind.Property ?
                    ((IPropertySymbol)symbolContext.Symbol).Parameters :
                    ((IMethodSymbol)symbolContext.Symbol).Parameters;

                foreach (IParameterSymbol param in parameters)
                {
                    AnalyzeSymbol(param, symbolContext);
                }
            }, SymbolKind.Property, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
        {
            // Check if memeber contains type name 
            string identifier = symbol.Name;
            bool isTypeName = s_types.Contains(identifier);
            if (isTypeName)
            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], identifier);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}