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
    /// CA1002: Do not expose generic lists
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotExposeGenericListsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1002";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotExposeGenericListsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotExposeGenericListsMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotExposeGenericListsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1002-do-not-expose-generic-lists",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method, SymbolKind.Parameter, SymbolKind.Field, SymbolKind.Property);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (!context.Symbol.IsExternallyVisible()) return;

            // This would report twice for properties; once on
            // the property type and once on the getter return,
            // so we need to filter them out
            if (context.Symbol.IsAccessorMethod()) return;

            ITypeSymbol symbol;
            switch (context.Symbol.Kind)
            {
                case SymbolKind.Method:
                    symbol = ((IMethodSymbol) context.Symbol).ReturnType;
                    break;
                case SymbolKind.Parameter:
                    symbol = ((IParameterSymbol) context.Symbol).Type;
                    break;
                case SymbolKind.Field:
                    symbol = ((IFieldSymbol) context.Symbol).Type;
                    break;
                case SymbolKind.Property:
                    symbol = ((IPropertySymbol) context.Symbol).Type;
                    break;
                default:
                    return;
            }

            if (!(symbol is INamedTypeSymbol type)) return;

            var listSymbol = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

            if (type.ConstructedFrom.Equals(listSymbol))
            {
                context.ReportDiagnostic(context.Symbol.CreateDiagnostic(Rule));
            }

        }
    }
}