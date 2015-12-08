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
    public abstract class PropertyNamesShouldNotMatchGetMethodsAnalyzer : DiagnosticAnalyzer
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
            // Analyze properties and methods.
            analysisContext.RegisterSymbolAction(symbolContext =>
            {
                AnalyzeSymbol(symbolContext.Symbol, symbolContext);

            }, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(ISymbol symbol, SymbolAnalysisContext context)
        {
            var identifier = s_get + symbol.Name;

            var types = symbol.ContainingType.GetBaseTypesAndThis();
            foreach (var type in types)
            {
                var methodsFound = type.GetMembers(identifier);
                if (methodsFound != null && methodsFound.Length > 0)
                {
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
            // ToDo: Check whether method name in declaring class is same as property name from base types - This is not something the current FXCop rule supports yet. Will add later. 
        }
    }
}