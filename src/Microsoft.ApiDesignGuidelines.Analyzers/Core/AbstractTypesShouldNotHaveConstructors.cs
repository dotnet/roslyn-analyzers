// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1012: Abstract classes should not have public constructors
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AbstractTypesShouldNotHaveConstructorsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1012";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AbstractTypesShouldNotHaveConstructorsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AbstractTypesShouldNotHaveConstructorsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AbstractTypesShouldNotHaveConstructorsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Design,
                                                                         DiagnosticSeverity.Warning,
                                                                         isEnabledByDefault: false,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182126.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol as INamedTypeSymbol;
            if (symbol.IsAbstract)
            {
                // TODO: Should we also check symbol.GetResultantVisibility() == SymbolVisibility.Public?

                bool hasAnyPublicConstructors =
                    symbol.InstanceConstructors.Any(
                        (constructor) => constructor.DeclaredAccessibility == Accessibility.Public);

                if (hasAnyPublicConstructors)
                {
                    context.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
                }
            }
        }
    }
}
