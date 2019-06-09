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
    /// <summary>
    /// CA1407: Avoid static members in COM visible types
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidStaticMembersInComVisibleTypesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1407";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidStaticMembersInComVisibleTypesTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidStaticMembersInComVisibleTypesMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.AvoidStaticMembersInComVisibleTypesDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Interoperability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1407-avoid-static-members-in-com-visible-types",
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol.DeclaredAccessibility != Accessibility.Public) return;

            var method = (IMethodSymbol)context.Symbol;
            if (method.ContainingType.DeclaredAccessibility != Accessibility.Public) return;

            // Skip non-static methods or operators and static accessors.
            if (!method.IsStatic) return;
            if (method.IsAccessorMethod()) return;
            if (method.IsOperator()) return;

            var comRegisterFunc = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.ComRegisterFunctionAttribute");
            var comUnregisterFunc = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.ComUnregisterFunctionAttribute");

            // Skip static COM registration methods (as those have a utility in COM)
            if (method.GetAttributes().AsParallel()
                .Any(a => a.AttributeClass.Equals(comRegisterFunc) || a.AttributeClass.Equals(comUnregisterFunc)))
            {
                return;
            }

            // Now we know the method is a 'plain' static method. 
            // Now we make sure it would be exposed to COM
            if (method.ComVisibleIsApplied(context.Compilation))
            {
                context.ReportDiagnostic(method.CreateDiagnostic(Rule));
            }
        }
    }
}