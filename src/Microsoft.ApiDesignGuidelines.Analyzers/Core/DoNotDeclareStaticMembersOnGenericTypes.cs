// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1000: Do not declare static members on generic types
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDeclareStaticMembersOnGenericTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1000";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDeclareStaticMembersOnGenericTypesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182139.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(
                symbolAnalysisContext =>
                {
                    ISymbol symbol = symbolAnalysisContext.Symbol;
                    if (!symbol.ContainingType.IsGenericType ||
                        symbol.DeclaredAccessibility != Accessibility.Public ||
                        !symbol.IsStatic)
                    {
                        return;
                    }

                    var methodSymbol = symbol as IMethodSymbol;
                    if (methodSymbol != null &&
                        (methodSymbol.IsAccessorMethod() ||
                         (methodSymbol.MethodKind == MethodKind.UserDefinedOperator &&
                          (methodSymbol.Name == WellKnownMemberNames.EqualityOperatorName ||
                           methodSymbol.Name == WellKnownMemberNames.InequalityOperatorName))))
                    {
                        return;
                    }

                    symbolAnalysisContext.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
                }, SymbolKind.Method, SymbolKind.Property);
        }
    }
}