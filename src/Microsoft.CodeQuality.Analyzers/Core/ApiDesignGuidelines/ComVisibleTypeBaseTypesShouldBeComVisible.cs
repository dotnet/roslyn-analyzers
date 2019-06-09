// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1405: COM visible type base types should be COM visible
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ComVisibleTypeBaseTypesShouldBeComVisibleAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1405";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComVisibleTypeBaseTypesShouldBeComVisibleTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComVisibleTypeBaseTypesShouldBeComVisibleMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComVisibleTypeBaseTypesShouldBeComVisibleDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1405-com-visible-type-base-types-should-be-com-visible",
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
            if (!context.Symbol.IsExternallyVisible()) return;

            var type = (INamedTypeSymbol)context.Symbol;

            if (type.Arity > 0) return;
            if (type.TypeKind != TypeKind.Class) return;
            if (type.BaseType.SpecialType == SpecialType.System_Object) return;

            var assemblyComVisible = type.ContainingAssembly.GetComVisibleState(context.Compilation);
            var typeComVisible = type.GetComVisibleState(context.Compilation);

            if (typeComVisible ?? assemblyComVisible ?? true)
            {
                var baseTypeComVisible = type.BaseType.GetComVisibleState(context.Compilation);
                if (!(baseTypeComVisible ?? assemblyComVisible ?? true))
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(Rule));
                }
            }
        }
    }
}