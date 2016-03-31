// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1725: Parameter names should match base declaration
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ParameterNamesShouldMatchBaseDeclarationAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1725";
        internal const string NewNamePropertyName = "NewName";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ParameterNamesShouldMatchBaseDeclarationTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ParameterNamesShouldMatchBaseDeclarationMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ParameterNamesShouldMatchBaseDeclarationDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly Action<SymbolAnalysisContext> AnalyzeMethodSymbolAction = AnalyzeMethodSymbol;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            analysisContext.EnableConcurrentExecution();

            analysisContext.RegisterSymbolAction(AnalyzeMethodSymbolAction, SymbolKind.Method);
        }

        private static void AnalyzeMethodSymbol(SymbolAnalysisContext analysisContext)
        {
            var methodSymbol = analysisContext.Symbol as IMethodSymbol;

            if (!(methodSymbol.CanBeReferencedByName || methodSymbol.IsImplementationOfAnyExplicitInterfaceMember())
                || !methodSymbol.Locations.Any(x => x.IsInSource)
                || string.IsNullOrWhiteSpace(methodSymbol.Name))
            {
                return;
            }

            if (!methodSymbol.IsOverride && !methodSymbol.IsImplementationOfAnyImplicitInterfaceMember())
            {
                return;
            }

            ImmutableArray<IMethodSymbol> originalDefinitions = GetOriginalDefinitions(methodSymbol);
            if (originalDefinitions.Length == 0)
            {
                // We did not find any original definitions so we don't have to do anything.
                // This can happen when the method has an override modifier,
                // but does not have any valid method it is overriding.
                return;
            }

            IMethodSymbol bestMatch = null;
            int bestMatchScore = -1;

            foreach (var originalDefinition in originalDefinitions)
            {
                // always prefer the method override, if it is available
                // (the overridden method will always be the first item in the list.)
                if (originalDefinition.ContainingType.TypeKind != TypeKind.Interface)
                {
                    bestMatch = originalDefinition;
                    break;
                }

                int currentMatchScore = 0;
                for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                {
                    IParameterSymbol currentParameter = methodSymbol.Parameters[i];
                    IParameterSymbol originalParameter = originalDefinition.Parameters[i];

                    if (currentParameter.Name == originalParameter.Name)
                    {
                        currentMatchScore++;
                    }
                }

                if (currentMatchScore > bestMatchScore)
                {
                    bestMatch = originalDefinition;
                    bestMatchScore = currentMatchScore;
                }
            }

            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                IParameterSymbol currentParameter = methodSymbol.Parameters[i];
                IParameterSymbol bestMatchParameter = bestMatch.Parameters[i];

                if (currentParameter.Name != bestMatchParameter.Name)
                {
                    var properties = ImmutableDictionary<string, string>.Empty.SetItem(NewNamePropertyName, bestMatchParameter.Name);

                    analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, currentParameter.Locations.First(), properties, null));
                }
            }
        }

        private static ImmutableArray<IMethodSymbol> GetOriginalDefinitions(IMethodSymbol methodSymbol)
        {
            ImmutableArray<IMethodSymbol>.Builder originalDefinitionsBuilder = ImmutableArray.CreateBuilder<IMethodSymbol>();

            if (methodSymbol.IsOverride && (methodSymbol.OverriddenMethod != null))
            {
                originalDefinitionsBuilder.Add(methodSymbol.OverriddenMethod);
            }

            if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
            {
                originalDefinitionsBuilder.AddRange(methodSymbol.ExplicitInterfaceImplementations);
            }

            var typeSymbol = methodSymbol.ContainingType;

            originalDefinitionsBuilder.AddRange(typeSymbol.AllInterfaces
                .SelectMany(m => m.GetMembers(methodSymbol.Name))
                .Where(m => typeSymbol.FindImplementationForInterfaceMember(m) != null)
                .Cast<IMethodSymbol>());

            return originalDefinitionsBuilder.ToImmutable();
        }
    }
}