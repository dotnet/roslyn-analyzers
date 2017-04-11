// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1054: Uri parameters should not be strings
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UriParametersShouldNotBeStringsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1054";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriParametersShouldNotBeStringsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriParametersShouldNotBeStringsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UriParametersShouldNotBeStringsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182174.aspx",
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
                var attribute = WellKnownTypes.Attribute(c.Compilation);
                if (@string == null || uri == null || attribute == null)
                {
                    // we don't have required types
                    return;
                }

                var analyzer = new PerCompilationAnalyzer(@string, uri, attribute);
                c.RegisterSymbolAction(analyzer.Analyze, SymbolKind.Method);
            });
        }

        private class PerCompilationAnalyzer
        {
            private readonly INamedTypeSymbol _string;
            private readonly INamedTypeSymbol _uri;
            private readonly INamedTypeSymbol _attribute;

            public PerCompilationAnalyzer(INamedTypeSymbol @string, INamedTypeSymbol uri, INamedTypeSymbol attribute)
            {
                _string = @string;
                _uri = uri;
                _attribute = attribute;
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

                var stringParameters = method.Parameters.GetParametersOfType(_string);
                if (!stringParameters.Any())
                {
                    // no string parameter. not interested.
                    return;
                }

                // now do cheap string check whether those string parameter contains uri word list we are looking for.
                if (!stringParameters.ParameterNamesContainUriWordSubstring(context.CancellationToken))
                {
                    // no string parameter that contains what we are looking for.
                    return;
                }

                if (method.ContainingType.DerivesFrom(_attribute, baseTypesOnly: true))
                {
                    // Attributes cannot accept System.Uri objects as positional or optional attributes
                    return;
                }

                // now we do more expensive word parsing to find exact parameter that contains url in parameter name
                var indices = method.GetParameterIndices(stringParameters.GetParametersThatContainUriWords(context.CancellationToken), context.CancellationToken);

                var overloads = method.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>();
                foreach (var index in indices)
                {
                    var overload = method.GetMatchingOverload(overloads, index, _uri, context.CancellationToken);
                    if (overload == null)
                    {
                        var parameter = method.Parameters[index];
                        context.ReportDiagnostic(parameter.CreateDiagnostic(Rule, parameter.Name, method.ToDisplayString(SymbolDisplayFormats.ShortSymbolDisplayFormat)));
                    }
                }
            }
        }
    }
}