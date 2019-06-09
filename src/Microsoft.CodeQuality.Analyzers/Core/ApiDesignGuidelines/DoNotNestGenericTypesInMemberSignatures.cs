// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1006: Do not nest generic types in member signatures
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotNestGenericTypesInMemberSignaturesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1006";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotNestGenericTypesInMemberSignaturesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotNestGenericTypesInMemberSignaturesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotNestGenericTypesInMemberSignaturesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1006-do-not-nest-generic-types-in-member-signatures",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method, SymbolKind.Parameter, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (!context.Symbol.IsExternallyVisible()) return;

            // This would report twice for properties; once on
            // the property type and once on the getter return,
            // so we need to filter them out
            if (context.Symbol.IsAccessorMethod()) return;

            var symbol = context.Symbol.GetMemberOrLocalOrParameterType();
            if (!(symbol is INamedTypeSymbol type)) return;

            foreach (var typeArgument in type.TypeArguments)
            {
                if (typeArgument is INamedTypeSymbol genericType && genericType.IsGenericType)
                {
                    context.ReportDiagnostic(context.Symbol.CreateDiagnostic(Rule));
                    return;
                }
            }
        }
    }
}