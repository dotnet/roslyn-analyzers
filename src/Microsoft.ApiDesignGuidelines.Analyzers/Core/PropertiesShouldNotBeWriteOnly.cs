// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1044: Properties should not be write only
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PropertiesShouldNotBeWriteOnlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1044";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageAddGetter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageAddGetter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMakeMoreAccessible = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyMessageMakeMoreAccessible), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropertiesShouldNotBeWriteOnlyDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));


        internal static DiagnosticDescriptor AddGetterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageAddGetter,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182165.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MakeMoreAccessibleRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMakeMoreAccessible,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182165.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AddGetterRule, MakeMoreAccessibleRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext), SymbolKind.Property);
        }

        /// <summary>
        /// Implementation for CA1044: Properties should not be write only
        /// </summary>
        /// <param name="context"></param>
        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {

            var property = context.Symbol as IPropertySymbol;
            // Nothing to do for overwritten issues
            if (property == null)
            {
                return;
            }
            // check for when rule is turned off
            if (property.IsOverride)
            {
                return;
            }

            // If there is no getter then it is not accessible
            if (property.IsWriteOnly)
            {
                context.ReportDiagnostic(property.CreateDiagnostic(AddGetterRule, property.Name));
            }
            // Otherwise there is a setter, so check for relative accessibility
            else if (!(property.IsReadOnly) && (property.GetMethod.DeclaredAccessibility < property.SetMethod.DeclaredAccessibility))
            {
                context.ReportDiagnostic(property.CreateDiagnostic(MakeMoreAccessibleRule, property.Name));
            }
        }
    }
}
