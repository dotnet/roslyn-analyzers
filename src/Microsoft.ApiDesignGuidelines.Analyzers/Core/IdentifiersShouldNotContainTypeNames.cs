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

        internal static HashSet<string> s_types = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary> 
        /// Initializes primitive types 
        /// </summary> 
        static IdentifiersShouldNotContainTypeNames()
        {
            s_types.Add("bool");
            s_types.Add("boolean");
            s_types.Add("byte");
            s_types.Add("sbyte");
            s_types.Add("ubyte");
            s_types.Add("char");
            s_types.Add("wchar");
            s_types.Add("int8");
            s_types.Add("uint8");
            s_types.Add("short");
            s_types.Add("ushort");
            s_types.Add("int");
            s_types.Add("uint");
            s_types.Add("integer");
            s_types.Add("uinteger");
            s_types.Add("long");
            s_types.Add("ulong");
            s_types.Add("unsigned");
            s_types.Add("signed");
            s_types.Add("float");
            s_types.Add("float32");
            s_types.Add("float64");
            s_types.Add("int16");
            s_types.Add("int32");
            s_types.Add("int64");
            s_types.Add("uint16");
            s_types.Add("uint32");
            s_types.Add("uint64");
            s_types.Add("intptr");
            s_types.Add("uintptr");
            s_types.Add("ptr");
            s_types.Add("uptr");
            s_types.Add("pointer");
            s_types.Add("upointer");
            s_types.Add("single");
            s_types.Add("double");
            s_types.Add("decimal");
            s_types.Add("guid");
            s_types.Add("object");
            s_types.Add("obj");
            s_types.Add("string");
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
            analysisContext.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            analysisContext.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
        {
            var typeName = symbol.Name;
            //check if memeber contains type name 
            var isTypeName = IsViolatingIdentifierName(typeName);
            if (isTypeName)
            {
                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], typeName);
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
        
        private static bool IsViolatingIdentifierName(string identifier)
        {
            if (s_types.Contains(identifier))
                return true;

            return false;
        }

    }
}