// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldHaveCorrectPrefixAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1715";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectPrefixTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectPrefixTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageInterfaceRule = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectPrefixMessageInterface), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        public static readonly DiagnosticDescriptor InterfaceRule = new DiagnosticDescriptor(RuleId,
                                                                                    s_localizableTitle,
                                                                                    s_localizableMessageInterfaceRule,
                                                                                    DiagnosticCategory.Naming,
                                                                                    DiagnosticSeverity.Warning,
                                                                                    isEnabledByDefault: true,
                                                                                    description: s_localizableDescription,
                                                                                    helpLinkUri: "http://msdn.microsoft.com/library/ms182243.aspx",
                                                                                    customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly LocalizableString s_localizableMessageTypeParameterRule = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldHaveCorrectPrefixMessageTypeParameter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        public static readonly DiagnosticDescriptor TypeParameterRule = new DiagnosticDescriptor(RuleId,
                                                                                      s_localizableTitle,
                                                                                      s_localizableMessageTypeParameterRule,
                                                                                      DiagnosticCategory.Naming,
                                                                                      DiagnosticSeverity.Warning,
                                                                                      isEnabledByDefault: true,
                                                                                      description: s_localizableDescription,
                                                                                      helpLinkUri: "http://msdn.microsoft.com/library/ms182243.aspx",
                                                                                      customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InterfaceRule, TypeParameterRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(
                (context) =>
            {
                switch (context.Symbol.Kind)
                {
                    case SymbolKind.NamedType:
                        AnalyzeNamedTypeSymbol((INamedTypeSymbol)context.Symbol, context.ReportDiagnostic);
                        break;

                    case SymbolKind.Method:
                        AnalyzeMethodSymbol((IMethodSymbol)context.Symbol, context.ReportDiagnostic);
                        break;
                }
            },
                SymbolKind.Method,
                SymbolKind.NamedType);
        }

        private static void AnalyzeNamedTypeSymbol(INamedTypeSymbol symbol, Action<Diagnostic> addDiagnostic)
        {
            foreach (ITypeParameterSymbol parameter in symbol.TypeParameters)
            {
                if (!HasCorrectPrefix(parameter, 'T'))
                {
                    addDiagnostic(parameter.CreateDiagnostic(TypeParameterRule, parameter.Name));
                }
            }

            if (symbol.TypeKind == TypeKind.Interface &&
                symbol.IsPublic() &&
                !HasCorrectPrefix(symbol, 'I'))
            {
                addDiagnostic(symbol.CreateDiagnostic(InterfaceRule, symbol.Name));
            }
        }

        private static void AnalyzeMethodSymbol(IMethodSymbol symbol, Action<Diagnostic> addDiagnostic)
        {
            foreach (ITypeParameterSymbol parameter in symbol.TypeParameters)
            {
                if (!HasCorrectPrefix(parameter, 'T'))
                {
                    addDiagnostic(parameter.CreateDiagnostic(TypeParameterRule, parameter.Name));
                }
            }
        }

        private static bool HasCorrectPrefix(ISymbol symbol, char prefix)
        {
            WordParser parser = new WordParser(symbol.Name, WordParserOptions.SplitCompoundWords, prefix);

            string firstWord = parser.NextWord();

            if (firstWord == null || firstWord.Length > 1)
            {
                return false;
            }

            return firstWord[0] == prefix;
        }
    }
}
