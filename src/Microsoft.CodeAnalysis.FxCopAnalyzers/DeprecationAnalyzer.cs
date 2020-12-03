// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

#if CODE_QUALITY_ANALYZERS
using AnalyzerResources = Microsoft.CodeQuality.Analyzers.MicrosoftCodeQualityAnalyzersResources;
#elif NET_CORE_ANALYZERS
using AnalyzerResources = Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;
#elif NET_FRAMEWORK_ANALYZERS
using AnalyzerResources = Microsoft.NetFramework.Analyzers.MicrosoftNetFrameworkAnalyzersResources;
#endif

namespace Microsoft.CodeAnalysis.FxCopAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DeprecationAnalyzer : DiagnosticAnalyzer
    {
        private const string RuleId = "CA9998";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(AnalyzerResources.AnalyzerPackageDeprecationTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
        private static readonly LocalizableString s_localizableMessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.AnalyzerPackageDeprecationMessage), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));

#pragma warning disable RS0030 // Do not used banned APIs - This is a special analyzer for package deprecation.
        public static readonly DiagnosticDescriptor Rule = new(
                                                        RuleId,
                                                        s_localizableTitle,
                                                        s_localizableMessageFormat,
                                                        DiagnosticCategory.Reliability,
                                                        DiagnosticSeverity.Warning,
                                                        isEnabledByDefault: true,
                                                        helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers",
                                                        description: null);
#pragma warning restore RS0030 // Do not used banned APIs

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationAction(context => context.ReportNoLocationDiagnostic(Rule));
        }
    }
}
