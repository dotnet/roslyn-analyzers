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
    /// CA1018: Custom attributes should have AttributeUsage attribute defined.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class MarkAttributesWithAttributeUsageAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1018";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAttributesWithAttributeUsageTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.MarkAttributesWithAttributeUsageMessageDefault), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                    s_localizableTitle,
                                                                    s_localizableMessage,
                                                                    DiagnosticCategory.Design,
                                                                    DiagnosticSeverity.Warning,
                                                                    isEnabledByDefault: true,
                                                                    helpLinkUri: "http://msdn.microsoft.com/library/ms182158.aspx",
                                                                    customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol attributeType = WellKnownTypes.Attribute(compilationContext.Compilation);
                INamedTypeSymbol attributeUsageAttributeType = WellKnownTypes.AttributeUsageAttribute(compilationContext.Compilation);
                if (attributeType == null || attributeUsageAttributeType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(context =>
                {
                    AnalyzeSymbol((INamedTypeSymbol)context.Symbol, attributeType, attributeUsageAttributeType, context.ReportDiagnostic);
                },
                SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(INamedTypeSymbol symbol, INamedTypeSymbol attributeType, INamedTypeSymbol attributeUsageAttributeType, Action<Diagnostic> addDiagnostic)
        {
            if (symbol.IsAbstract || !symbol.GetBaseTypesAndThis().Contains(attributeType))
            {
                return;
            }

            bool hasAttributeUsageAttribute = symbol.GetAttributes().Any(attribute => attribute.AttributeClass == attributeUsageAttributeType);
            if (!hasAttributeUsageAttribute)
            {
                addDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name));
            }
        }
    }
}
