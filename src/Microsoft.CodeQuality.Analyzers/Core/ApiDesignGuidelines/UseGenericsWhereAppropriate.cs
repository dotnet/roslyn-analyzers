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
    /// CA1007: Use generics where appropriate
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseGenericsWhereAppropriateAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1007";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericsWhereAppropriateTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericsWhereAppropriateMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseGenericsWhereAppropriateDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Design,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1007-use-generics-where-appropriate",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (!context.Symbol.IsExternallyVisible()) return;

            var method = (IMethodSymbol)context.Symbol;

            foreach (var param in method.Parameters)
            {
                if (param.Type.SpecialType != SpecialType.System_Object || param.RefKind != RefKind.Ref)
                {
                    continue;
                }

                context.ReportDiagnostic(context.Symbol.CreateDiagnostic(Rule));
                return;
            }
        }
    }
}