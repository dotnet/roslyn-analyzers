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
    public sealed class TypesWithCertainAttributesShouldNotBeComVisibleAnalyzer : DiagnosticAnalyzer
    {
        internal const string AutoLayoutRuleId = "CA1403";
        internal const string AutoDualRuleId = "CA1408";

        private static readonly LocalizableString s_autoLayoutTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AutoLayoutTypesShouldNotBeComVisibleTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_autoLayoutMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AutoLayoutTypesShouldNotBeComVisibleMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_autoLayoutDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AutoLayoutTypesShouldNotBeComVisibleDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));


        private static readonly LocalizableString s_autoDualTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotUseAutoDualClassInterfaceTypeTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_autoDualMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotUseAutoDualClassInterfaceTypeMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_autoDualDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotUseAutoDualClassInterfaceTypeDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));


        internal static DiagnosticDescriptor NoAutoLayoutRule = new DiagnosticDescriptor(AutoLayoutRuleId,
            s_autoLayoutTitle,
            s_autoLayoutMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_autoLayoutDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1403-auto-layout-types-should-not-be-com-visible",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        internal static DiagnosticDescriptor NoAutoDualRule = new DiagnosticDescriptor(AutoDualRuleId,
            s_autoDualTitle,
            s_autoDualMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_autoDualDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1408-do-not-use-autodual-classinterfacetype",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoAutoLayoutRule, NoAutoDualRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol.DeclaredAccessibility != Accessibility.Public) return;

            var type = (INamedTypeSymbol)context.Symbol;
            if (type.Arity > 0) return;

            var structLayout = WellKnownTypes.StructLayoutAttribute(context.Compilation);
            var classInterface = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.ClassInterfaceAttribute");

            if (type.ComVisibleIsApplied(context.Compilation))
            {
                if (type.IsValueType && type.GetAttributes().AsParallel()
                    .Any(a => a.AttributeClass.Equals(structLayout) && a.ConstructorArguments[0].Value is 3)) // 3 == LayoutKind.Auto
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(NoAutoLayoutRule));
                }

                if (type.TypeKind == TypeKind.Class && type.GetAttributes().AsParallel()
                    .Any(a => a.AttributeClass.Equals(classInterface) && a.ConstructorArguments[0].Value is 2)) // 2 == ClassInterfaceType.AutoDual
                {
                    context.ReportDiagnostic(type.CreateDiagnostic(NoAutoDualRule));
                }
            }
        }
    }
}