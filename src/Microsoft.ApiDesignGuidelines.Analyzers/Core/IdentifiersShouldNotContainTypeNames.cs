// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Collections.Generic;
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
            new []{
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
            analysisContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
            analysisContext.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            analysisContext.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            analysisContext.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
        {
            var identifier = symbol.Name;
            //check if memeber contains type name 
            var isTypeName = s_types.Contains(identifier);
            if (isTypeName)
            {
                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], identifier);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary> 
        /// Retrieve the name of the class/struct/enum and store it in 
        /// a userdefinedtypes hashset to ensure no member or parameter uses that userdefined typename 
        /// Also verify that these members are not violating the CA1720 rule 
        /// </summary> 
        /// <param name="context"></param> 
        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            AnalyzeSymbol(context.Symbol, context);
        }

        private static void AnalyzeField(SymbolAnalysisContext context)
        {
            AnalyzeSymbol(context.Symbol, context);
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            AnalyzeSymbol(context.Symbol, context);
            var parameters = ((IPropertySymbol)context.Symbol).Parameters;
            foreach (var param in parameters)
            {
                AnalyzeSymbol(param, context);
            }
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            AnalyzeSymbol(context.Symbol, context);
            var parameters = ((IMethodSymbol)context.Symbol).Parameters;
            foreach (var param in parameters)
            {
                AnalyzeSymbol(param, context);
            }
        }
    }
}