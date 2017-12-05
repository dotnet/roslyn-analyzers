// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1028: Enum Storage should be Int32
    /// Implementation slightly modified from original FxCop after discussing with Nick Guerrera
    /// FxCop implementation used 2 distinct diagnostic messages depending on the underlying type of the enum
    /// In this implementation, we have only 1 diagnostic message - "If possible, make the underlying type of '{0}'  System.Int32 instead of {1}."
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class EnumStorageShouldBeInt32Analyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1028";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumStorageShouldBeInt32Title), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNotInt32 = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumStorageShouldBeInt32Message), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.EnumStorageShouldBeInt32Description), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNotInt32,
                                                                             DiagnosticCategory.Design,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/ms182147.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationContext =>
            {
                INamedTypeSymbol flagsAttribute = WellKnownTypes.FlagsAttribute(compilationContext.Compilation);
                if (flagsAttribute == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, flagsAttribute), SymbolKind.NamedType);
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol flagsAttribute)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;

            if (symbol.TypeKind != TypeKind.Enum)
            {
                return;
            }

            SpecialType underlyingType = symbol.EnumUnderlyingType.SpecialType;
            if (underlyingType == SpecialType.System_Int32)
            {
                return;
            }

            // If accessibility of enum is not public exit
            if (!symbol.IsExternallyVisible())
            {
                return;
            }

            // If enum is Int64 and has Flags attributes then exit
            bool hasFlagsAttribute = symbol.GetAttributes().Any(a => a.AttributeClass.Equals(flagsAttribute));
            if (underlyingType == SpecialType.System_Int64 && hasFlagsAttribute)
            {
                return;
            }

            context.ReportDiagnostic(symbol.CreateDiagnostic(Rule, symbol.Name, symbol.EnumUnderlyingType));
        }
    }
}