﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1402: Avoid overloads in COM visible interfaces
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidOverloadsInComVisibleInterfacesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1402";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidOverloadsInComVisibleInterfacesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidOverloadsInComVisibleInterfacesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidOverloadsInComVisibleInterfacesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1402-avoid-overloads-in-com-visible-interfaces",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (!type.IsValidComExport()) return;
            if (type.TypeKind != TypeKind.Class) return;

            if (type.ComVisibleIsApplied(context.Compilation))
            {
                // Remove any methods that aren't ComVisible, and
                // group all remaining methods into method groups
                var groups = from method in type.GetMembers().OfType<IMethodSymbol>()
                             where method.Arity == 0 && method.ComVisibleIsApplied(context.Compilation)
                             group method by method.Name;

                foreach (var group in groups)
                {
                    if (group.Count() > 1)
                    {
                        context.ReportDiagnostic(type.CreateDiagnostic(Rule, group.First().Name, type.Name));
                    }
                }
            }
        }
    }
}