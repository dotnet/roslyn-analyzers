// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Microsoft.ApiDesignGuidelines.Analyzers
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
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
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
            ISymbol symbol = context.Symbol;

            // Only look for methods hiding others (not overriding). Overriding with a different visibility is already a compiler error
            if (symbol.IsOverride)
            {
                return;
            }

            // Bail out if the member is publicly accessible, or sealed, or on a sealed type
            if (IsVisibleOutsideAssembly(symbol) || symbol.IsSealed || (symbol.ContainingType?.IsSealed ?? true))
            {
                return;
            }

            // Event accessors cannot have visibility modifiers, so don't analyze them
            if ((symbol as IMethodSymbol)?.AssociatedSymbol as IEventSymbol != null)
            {
                return;
            }

            // Find members on base types that share the member's name
            var ancestorTypes = symbol?.ContainingType?.GetBaseTypes() ?? Enumerable.Empty<INamedTypeSymbol>();
            var hiddenOrOverriddenMembers = ancestorTypes.SelectMany(t => t.GetMembers(symbol.Name));

            if (hiddenOrOverriddenMembers.Any(IsVisibleOutsideAssembly))
            {
                context.ReportDiagnostic(symbol.CreateDiagnostic(Rule));
            }
        }

        private static bool IsVisibleOutsideAssembly(ISymbol symbol)
        {
            // If the containing type is not visible, then neither is the contained symbol
            if (symbol.ContainingType != null && !IsVisibleOutsideAssembly(symbol.ContainingType))
            {
                return false;
            }

            // Public symbols are visible outside the assembly
            if (symbol.DeclaredAccessibility == Accessibility.Public)
            {
                return true;
            }

            // Protected members are visible if on unsealed types
            if ((symbol.DeclaredAccessibility == Accessibility.Protected || symbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
                && (symbol.ContainingType?.IsSealed == false))
            {
                return true;
            }

            // Check for explicit interface implementations if the symbol is a method, property, or event
            if ((symbol as IMethodSymbol)?.ExplicitInterfaceImplementations.Any(IsVisibleOutsideAssembly) ?? false)
            {
                return true;
            }
            if ((symbol as IPropertySymbol)?.ExplicitInterfaceImplementations.Any(IsVisibleOutsideAssembly) ?? false)
            {
                return true;
            }
            if ((symbol as IEventSymbol)?.ExplicitInterfaceImplementations.Any(IsVisibleOutsideAssembly) ?? false)
            {
                return true;
            }

            return false;
        }
    }
}