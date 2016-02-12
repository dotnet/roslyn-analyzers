// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA2231: Complain if the type implements Equals without overloading the equality operator.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class OverloadOperatorEqualsOnOverridingValueTypeEqualsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2231";
        private static readonly LocalizableString s_localizableMessageAndTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverloadOperatorEqualsOnOverridingValueTypeEqualsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverloadOperatorEqualsOnOverridingValueTypeEqualsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableMessageAndTitle,
                                                                         s_localizableMessageAndTitle,
                                                                         DiagnosticCategory.Usage,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: true,
                                                                         description: s_localizableDescription,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182359.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(context =>
            {
                AnalyzeSymbol((INamedTypeSymbol)context.Symbol, context.ReportDiagnostic);
            },
            SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(INamedTypeSymbol namedTypeSymbol, Action<Diagnostic> addDiagnostic)
        {
            if (namedTypeSymbol.IsValueType && namedTypeSymbol.OverridesEquals() && !namedTypeSymbol.ImplementsEqualityOperators())
            {
                addDiagnostic(namedTypeSymbol.CreateDiagnostic(Rule));
            }
        }
    }
}
