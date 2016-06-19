// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1055: Uri return values should not be strings
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UriReturnValuesShouldNotBeStringsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1055";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriReturnValuesShouldNotBeStringsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriReturnValuesShouldNotBeStringsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriReturnValuesShouldNotBeStringsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182176.aspx",
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
                var uri = WellKnownTypes.Uri(c.Compilation);
                if (@string == null || uri == null)
                {
                    // we don't have required types
                    return;
                }

                var analyzer = new PerCompilationAnalyzer(@string, uri);
                c.RegisterSymbolAction(analyzer.Analyze, SymbolKind.Method);
            });
        }

        private class PerCompilationAnalyzer
        {
            private readonly INamedTypeSymbol _string;
            private readonly INamedTypeSymbol _uri;

            public PerCompilationAnalyzer(INamedTypeSymbol @string, INamedTypeSymbol uri)
            {
                _string = @string;
                _uri = uri;
            }

            public void Analyze(SymbolAnalysisContext context)
            {
                var method = (IMethodSymbol)context.Symbol;

                // check basic stuff that FxCop checks. 
                if (method.IsOverride || method.IsFromMscorlib(context.Compilation))
                {
                    // Methods defined within mscorlib are excluded from this rule,
                    // since mscorlib cannot depend on System.Uri, which is defined 
                    // in System.dll
                    return;
                }

                if (method.GetResultantVisibility() != SymbolVisibility.Public)
                {
                    // only apply to methods that are exposed outside
                    return;
                }

                if (method.IsAccessorMethod() || method.ReturnType?.Equals(_string) != true)
                {
                    // return type must be string and it must be not an accessor method
                    return;
                }

                if (method.Parameters.ContainsParameterOfType(_uri))
                {
                    // If you take a Uri, and return a string, then it's ok 
                    return;
                }

                if (!method.SymbolNameContainsUriWords(context.CancellationToken))
                {
                    // doesn't contain uri word in its name
                    return;
                }

                context.ReportDiagnostic(method.CreateDiagnostic(Rule, method.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
            }
        }
    }
}