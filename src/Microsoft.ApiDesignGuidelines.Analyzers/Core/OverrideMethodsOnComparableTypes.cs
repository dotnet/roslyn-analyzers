// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1036: A public or protected type implements the System.IComparable interface and 
    /// does not override Object.Equals or does not overload the language-specific operator
    /// for equality, inequality, less than, or greater than. The rule does not report a
    /// violation if the type inherits only an implementation of the interface.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class OverrideMethodsOnComparableTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1036";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideMethodsOnComparableTypesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideMethodsOnComparableTypesMessageEquals), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideMethodsOnComparableTypesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                                  s_localizableTitle,
                                                                                  s_localizableMessage,
                                                                                  DiagnosticCategory.Design,
                                                                                  DiagnosticSeverity.Warning,
                                                                                  isEnabledByDefault: true,
                                                                                  description: s_localizableDescription,
                                                                                  helpLinkUri: "http://msdn.microsoft.com/library/ms182163.aspx",
                                                                                  customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol comparableType = WellKnownTypes.IComparable(compilationContext.Compilation);
                INamedTypeSymbol genericComparableType = WellKnownTypes.GenericIComparable(compilationContext.Compilation);

                // Even if one of them is available, we should continue analysis.
                if (comparableType == null && genericComparableType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(context =>
                {
                    AnalyzeSymbol((INamedTypeSymbol)context.Symbol, comparableType, genericComparableType, context.ReportDiagnostic);
                },
                SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol comparableType, INamedTypeSymbol genericComparableType, Action<Diagnostic> addDiagnostic)
        {
            if (namedTypeSymbol.DeclaredAccessibility == Accessibility.Private || namedTypeSymbol.TypeKind == TypeKind.Interface || namedTypeSymbol.TypeKind == TypeKind.Enum)
            {
                return;
            }

            if (namedTypeSymbol.AllInterfaces.Any(t => t.Equals(comparableType) ||
                                                      (t.ConstructedFrom?.Equals(genericComparableType) ?? false)))
            {
                if (!(namedTypeSymbol.OverridesEquals() && namedTypeSymbol.ImplementsComparisonOperators()))
                {
                    addDiagnostic(namedTypeSymbol.CreateDiagnostic(Rule));
                }
            }
        }
    }
}
