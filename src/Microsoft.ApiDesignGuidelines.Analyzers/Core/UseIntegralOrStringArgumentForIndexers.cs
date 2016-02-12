// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Linq;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1043: Use Integral Or String Argument For Indexers
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseIntegralOrStringArgumentForIndexersAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1043";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.UseIntegralOrStringArgumentForIndexersDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182180.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        private static readonly SpecialType[] s_allowedTypes = new SpecialType[] {
                        SpecialType.System_String,
                        SpecialType.System_Int16,
                        SpecialType.System_Int32,
                        SpecialType.System_Int64,
                        SpecialType.System_Object,
                        SpecialType.System_UInt16,
                        SpecialType.System_UInt32,
                        SpecialType.System_UInt64
                        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = (IPropertySymbol)context.Symbol;
            if (symbol.IsIndexer && !symbol.IsOverride)
            {
                if (symbol.GetParameters().Length == 1)
                {
                    ITypeSymbol paramType = symbol.GetParameters()[0].Type;
                    if (!s_allowedTypes.Contains(paramType.SpecialType))
                    {
                        context.ReportDiagnostic(symbol.CreateDiagnostic(Rule));
                    }
                }
            }
        }
    }
}

