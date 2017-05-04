// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1056: Uri properties should not be strings
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UriPropertiesShouldNotBeStringsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1056";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriPropertiesShouldNotBeStringsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriPropertiesShouldNotBeStringsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriPropertiesShouldNotBeStringsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182175.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            // this is stateless analyzer, can run concurrently
            analysisContext.EnableConcurrentExecution();

            // this has no meaning on running on generated code which user can't control
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(c =>
            {
                var @string = WellKnownTypes.String(c.Compilation);
                var attribute = WellKnownTypes.Attribute(c.Compilation);
                if (@string == null || attribute == null)
                {
                    // we don't have required types
                    return;
                }

                var analyzer = new PerCompilationAnalyzer(@string, attribute);
                c.RegisterSymbolAction(analyzer.Analyze, SymbolKind.Property);
            });
        }

        private class PerCompilationAnalyzer
        {
            private readonly INamedTypeSymbol _string;
            private readonly INamedTypeSymbol _attribute;

            public PerCompilationAnalyzer(INamedTypeSymbol @string, INamedTypeSymbol attribute)
            {
                _string = @string;
                _attribute = attribute;
            }

            public void Analyze(SymbolAnalysisContext context)
            {
                var property = (IPropertySymbol)context.Symbol;

                // check basic stuff that FxCop checks. 
                if (property.IsOverride || property.IsFromMscorlib(context.Compilation))
                {
                    // Methods defined within mscorlib are excluded from this rule,
                    // since mscorlib cannot depend on System.Uri, which is defined 
                    // in System.dll
                    return;
                }

                if (property.GetResultantVisibility() != SymbolVisibility.Public)
                {
                    // only apply to methods that are exposed outside
                    return;
                }

                if (property.Type?.Equals(_string) != true)
                {
                    // not expected type
                    return;
                }

                if (property.ContainingType.DerivesFrom(_attribute, baseTypesOnly: true))
                {
                    // Attributes cannot accept System.Uri objects as positional or optional attributes
                    return;
                }

                if (!property.SymbolNameContainsUriWords(context.CancellationToken))
                {
                    // property name doesnt contain uri word
                    return;
                }

                context.ReportDiagnostic(property.CreateDiagnostic(Rule, property.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
            }
        }
    }
}