﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class MarkAssembliesWithAttributesDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA1016RuleId = "CA1016";
        internal const string CA1014RuleId = "CA1014";

        private static readonly LocalizableString s_localizableMessageCA1016 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAssembliesWithAssemblyVersionTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionCA1016 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAssembliesWithAssemblyVersionDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor CA1016Rule = new DiagnosticDescriptor(CA1016RuleId,
                                                                         s_localizableMessageCA1016,
                                                                         s_localizableMessageCA1016,
                                                                         DiagnosticCategory.Design,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                         description: s_localizableDescriptionCA1016,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182155.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly LocalizableString s_localizableMessageCA1014 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAssembliesWithClsCompliantTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescriptionCA1014 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAssembliesWithClsCompliantDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        internal static DiagnosticDescriptor CA1014Rule = new DiagnosticDescriptor(CA1014RuleId,
                                                                         s_localizableMessageCA1014,
                                                                         s_localizableMessageCA1014,
                                                                         DiagnosticCategory.Design,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: false,
                                                                         description: s_localizableDescriptionCA1014,
                                                                         helpLinkUri: "http://msdn.microsoft.com/library/ms182156.aspx",
                                                                         customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX ? ImmutableArray.Create(CA1016Rule, CA1014Rule) : ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            INamedTypeSymbol assemblyVersionAttributeSymbol = WellKnownTypes.AssemblyVersionAttribute(context.Compilation);
            INamedTypeSymbol assemblyComplianceAttributeSymbol = WellKnownTypes.CLSCompliantAttribute(context.Compilation);

            if (assemblyVersionAttributeSymbol == null && assemblyComplianceAttributeSymbol == null)
            {
                return;
            }

            bool assemblyVersionAttributeFound = false;
            bool assemblyComplianceAttributeFound = false;

            // Check all assembly level attributes for the target attribute
            foreach (AttributeData attribute in context.Compilation.Assembly.GetAttributes())
            {
                if (attribute.AttributeClass.Equals(assemblyVersionAttributeSymbol))
                {
                    // Mark the version attribute as found
                    assemblyVersionAttributeFound = true;
                }
                else if (attribute.AttributeClass.Equals(assemblyComplianceAttributeSymbol))
                {
                    // Mark the compliance attribute as found
                    assemblyComplianceAttributeFound = true;
                }
            }

            if (!assemblyVersionAttributeFound && assemblyVersionAttributeSymbol != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(CA1016Rule, Location.None));
            }

            if (!assemblyComplianceAttributeFound && assemblyComplianceAttributeSymbol != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(CA1014Rule, Location.None));
            }
        }
    }
}
