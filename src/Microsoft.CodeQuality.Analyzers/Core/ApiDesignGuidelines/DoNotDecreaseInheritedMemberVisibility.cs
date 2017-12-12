// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA2222: Do not decrease inherited member visibility
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotDecreaseInheritedMemberVisibilityAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2222";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDecreaseInheritedMemberVisibilityTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDecreaseInheritedMemberVisibilityMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDecreaseInheritedMemberVisibilityDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182332.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // It may be interesting to flag field, property, and event symbols for this analyzer, but don't for now in order
            // to maintain compatibility with the old FxCop CA2222 rule which only analyzed methods.
            analysisContext.RegisterSymbolAction(CheckForDecreasedVisibility, SymbolKind.Method /*, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event*/);
        }

        private static void CheckForDecreasedVisibility(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            // Only look for methods hiding others (not overriding). Overriding with a different visibility is already a compiler error
            if (method.IsOverride)
            {
                return;
            }

            // Bail out if the method is publicly accessible, sealed, or a constructor.
            if (method.IsExternallyVisible() || method.IsSealed || method.MethodKind == MethodKind.Constructor)
            {
                return;
            }

            // Bail out if the method's containing type is not publicly accessible or is sealed.
            var type = method.ContainingType;
            if (!type.IsExternallyVisible() || type.IsSealed)
            {
                return;
            }

            // Event accessors cannot have visibility modifiers, so don't analyze them
            if (method.AssociatedSymbol as IEventSymbol != null)
            {
                return;
            }

            // Find members on base types that share the member's name
            var ancestorTypes = method.ContainingType.GetBaseTypes() ?? Enumerable.Empty<INamedTypeSymbol>();
            var hiddenOrOverriddenMembers = ancestorTypes.SelectMany(t => t.GetMembers(method.Name));

            if (hiddenOrOverriddenMembers.Any(m => m.IsExternallyVisible()))
            {
                context.ReportDiagnostic(method.CreateDiagnostic(Rule));
            }
        }
    }
}