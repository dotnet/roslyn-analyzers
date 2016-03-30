// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1815: Override equals and operator equals on value types
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1815";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageEquals = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesMessageEquals), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageOpEquality = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesMessageOpEquality), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.OverrideEqualsAndOperatorEqualsOnValueTypesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly string s_category = DiagnosticCategory.Performance;
        private static readonly DiagnosticSeverity s_severity = DiagnosticSeverity.Warning;
        private static readonly bool s_isEnabledByDefault = true;
        private static readonly string s_helpLinkUri = "https://msdn.microsoft.com/en-us/library/ms182276.aspx";
        private static readonly string s_customTags = WellKnownDiagnosticTags.Telemetry;

        internal static DiagnosticDescriptor EqualsRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageEquals,
                                                                             s_category,
                                                                             s_severity,
                                                                             s_isEnabledByDefault,
                                                                             s_localizableDescription,
                                                                             s_helpLinkUri,
                                                                             s_customTags);

        internal static DiagnosticDescriptor OpEqualityRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageOpEquality,
                                                                             s_category,
                                                                             s_severity,
                                                                             s_isEnabledByDefault,
                                                                             s_localizableDescription,
                                                                             s_helpLinkUri,
                                                                             s_customTags);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EqualsRule, OpEqualityRule);

        protected virtual bool IsAssignableTo(INamedTypeSymbol type, INamedTypeSymbol assignableToType)
        {
            // TODO: Use the language specific helper for IsAssignableTo.
            return false;
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationStartContext =>
            {
                var iEnumerator = WellKnownTypes.IEnumerator(compilationStartContext.Compilation);
                var genericIEnumerator = WellKnownTypes.GenericIEnumerator(compilationStartContext.Compilation);

                compilationStartContext.RegisterSymbolAction(context =>
                {
                    var namedType = (INamedTypeSymbol)context.Symbol;

                    // FxCop compat:
                    //  1. Do not fire for enums.
                    //  2. Do not fire for enumerators.
                    //  3. Do not fire for value types without members.
                    if (!namedType.IsValueType ||
                        namedType.TypeKind == TypeKind.Enum ||
                        namedType.GetResultantVisibility() != SymbolVisibility.Public ||
                        !namedType.GetMembers().Any(m => !m.IsConstructor()))
                    {
                        return;
                    }

                    // Enumerators are often ValueTypes to prevent heap allocation when enumerating
                    if (iEnumerator != null && IsAssignableTo(namedType, iEnumerator) ||
                        genericIEnumerator != null && IsAssignableTo(namedType, genericIEnumerator))
                    {
                        return;
                    }

                    if (!namedType.OverridesEquals())
                    {
                        context.ReportDiagnostic(namedType.CreateDiagnostic(EqualsRule, namedType.Name));
                    }

                    if (!namedType.ImplementsEqualityOperators())
                    {
                        context.ReportDiagnostic(namedType.CreateDiagnostic(OpEqualityRule, namedType.Name));
                    }
                }, SymbolKind.NamedType);
            });
        }
    }
}