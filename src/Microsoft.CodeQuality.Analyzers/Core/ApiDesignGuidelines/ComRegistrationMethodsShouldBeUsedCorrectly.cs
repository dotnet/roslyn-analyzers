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
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ComRegistrationMethodsShouldBeUsedCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string MatchedRuleId = "CA1410";
        internal const string VisibleRuleId = "CA1411";

        private static readonly LocalizableString s_matchedTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComRegistrationMethodsShouldBeMatchedTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_matchedMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComRegistrationMethodsShouldBeMatchedMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_matchedDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComRegistrationMethodsShouldBeMatchedDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_visibleTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComRegistrationMethodsShouldNotBeVisibleTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_visibleMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComRegistrationMethodsShouldNotBeVisibleMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_visibleDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.ComRegistrationMethodsShouldNotBeVisibleDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));


        internal static DiagnosticDescriptor MatchedRule = new DiagnosticDescriptor(MatchedRuleId,
            s_matchedTitle,
            s_matchedMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_matchedDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1410-com-registration-methods-should-be-matched",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        internal static DiagnosticDescriptor VisibleRule = new DiagnosticDescriptor(VisibleRuleId,
            s_visibleTitle,
            s_visibleMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_visibleDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1411-com-registration-methods-should-not-be-visible",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MatchedRule, VisibleRule);

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

            var comRegisterFunc = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.ComRegisterFunctionAttribute");
            var comUnregisterFunc = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.ComUnregisterFunctionAttribute");

            var registerMethod = type.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(a => a.HasAttribute(comRegisterFunc));
            var unregisterMethod = type.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(a => a.HasAttribute(comUnregisterFunc));

            // trueValue: true if left parameter is true, false if right parameter is true, null if neither parameter is true
            static (bool result, bool? trueValue) XorWithResult(bool left, bool right)
            {
                var result = left ^ right;
                if (left)
                    return (result, true);
                else if (right)
                    return (result, false);
                else
                    return (result, null);
            }

            // If the type has either/or function, but not both
            var (result, trueValue) = XorWithResult(!(registerMethod is null), !(unregisterMethod is null));

            if (result)
            {
#pragma warning disable IDE0055 // Fix formatting
                var diagnostic = trueValue switch
                {
                    // The left value was true (register method is present)
                    true => type.CreateDiagnostic(MatchedRule, "ComRegisterFunction", "ComUnregisterFunction"),

                    // The right value was true (unregister method is present)
                    false => type.CreateDiagnostic(MatchedRule, "ComUnregisterFunction", "ComRegisterFunction")
                };
#pragma warning enable IDE0055 // Fix formatting

                if (!(diagnostic is null)) context.ReportDiagnostic(diagnostic);
            }

            // Warn if registration methods are visible

            if (registerMethod?.IsExternallyVisible() == true)
            {
                context.ReportDiagnostic(registerMethod.CreateDiagnostic(VisibleRule));
            }

            if (unregisterMethod?.IsExternallyVisible() == true)
            {
                context.ReportDiagnostic(unregisterMethod.CreateDiagnostic(VisibleRule));
            }

        }
    }
}