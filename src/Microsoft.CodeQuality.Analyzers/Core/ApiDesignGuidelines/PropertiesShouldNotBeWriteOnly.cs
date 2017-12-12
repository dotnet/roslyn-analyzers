// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1044: Properties should not be write only
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PropertiesShouldNotBeWriteOnlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1044";
        private const string HelpLinkUri = "https://msdn.microsoft.com/en-us/library/ms182165.aspx";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageAddGetter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageAddGetter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeMoreAccessible = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageMakeMoreAccessible), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor AddGetterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageAddGetter,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MakeMoreAccessibleRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeMoreAccessible,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddGetterRule, MakeMoreAccessibleRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
        }

        /// <summary>
        /// Implementation for CA1044: Properties should not be write only
        /// </summary>
        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var property = context.Symbol as IPropertySymbol;
            if (property == null)
            {
                return;
            }

            // not raising a violation for when: 
            //     property is overridden because the issue can only be fixed in the base type 
            //     property is the implementaton of any interface member 
            if (property.IsOverride || property.IsImplementationOfAnyInterfaceMember())
            {
                return;
            }

            // If property is not visible outside the assembly
            if (!property.IsExternallyVisible())
            {
                return;
            }

            // We handled the non-CA1044 cases earlier.  Now, we handle CA1044 cases
            // If there is no getter then it is not accessible
            if (property.IsWriteOnly)
            {
                context.ReportDiagnostic(property.CreateDiagnostic(AddGetterRule, property.Name));
            }
            // Otherwise if there is a setter, check for its relative accessibility
            else if (!(property.IsReadOnly) && (property.GetMethod.DeclaredAccessibility < property.SetMethod.DeclaredAccessibility))
            {
                context.ReportDiagnostic(property.CreateDiagnostic(MakeMoreAccessibleRule, property.Name));
            }
        }
    }
}
