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
    public sealed class AvoidByRefParametersAnalyzer : DiagnosticAnalyzer
    {
        internal const string AvoidOutRuleId = "CA1021";
        internal const string AvoidRefRuleId = "CA1045";

        private static readonly LocalizableString s_localizableTitleOut = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidOutParametersTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableTitleRef = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidRefParametersTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageOut = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidOutParametersMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageRef = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidRefParametersMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescriptionOut = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidOutParametersDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescriptionRef = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidRefParametersDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor AvoidOutRule = new DiagnosticDescriptor(AvoidOutRuleId,
            s_localizableTitleOut,
            s_localizableMessageOut,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescriptionOut,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1021-avoid-out-parameters",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        internal static DiagnosticDescriptor AvoidRefRule = new DiagnosticDescriptor(AvoidRefRuleId,
            s_localizableTitleRef,
            s_localizableMessageRef,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescriptionRef,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1045-do-not-pass-types-by-reference",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AvoidOutRule, AvoidRefRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Parameter);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (!context.Symbol.IsExternallyVisible()) return;

            var param = (IParameterSymbol)context.Symbol;

            switch (param.RefKind)
            {
                case RefKind.In:
                case RefKind.None:
                    return;
                case RefKind.Ref:
                    context.ReportDiagnostic(context.Symbol.CreateDiagnostic(AvoidRefRule));
                    break;
                case RefKind.Out:
                    context.ReportDiagnostic(context.Symbol.CreateDiagnostic(AvoidOutRule));
                    break;
            }
        }
    }
}