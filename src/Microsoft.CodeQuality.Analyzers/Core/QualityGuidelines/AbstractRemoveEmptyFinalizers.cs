// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using System.Linq;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    public abstract class AbstractRemoveEmptyFinalizersAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1821";
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizers), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizers), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftQualityGuidelinesAnalyzersResources.RemoveEmptyFinalizersDescription), MicrosoftQualityGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftQualityGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                         s_localizableTitle,
                                                                         s_localizableMessage,
                                                                         DiagnosticCategory.Performance,
                                                                         DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                         isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
                                                                         description: s_localizableDescription,
                                                                         helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1821-remove-empty-finalizers",
                                                                         customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCodeBlockAction(codeBlockContext =>
            {
                if (codeBlockContext.OwningSymbol.Kind != SymbolKind.Method)
                {
                    return;
                }

                var methodSymbol = (IMethodSymbol)codeBlockContext.OwningSymbol;
                if (!methodSymbol.IsDestructor())
                {
                    return;
                }

                var methodBody = methodSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                if (IsEmptyFinalizer(methodBody, codeBlockContext))
                {
                    codeBlockContext.ReportDiagnostic(methodSymbol.CreateDiagnostic(Rule));
                }
            });
        }

        protected static bool InvocationIsConditional(IMethodSymbol methodSymbol, INamedTypeSymbol conditionalAttributeSymbol) =>
            methodSymbol.GetAttributes().Any(n => n.AttributeClass.Equals(conditionalAttributeSymbol));

        protected abstract bool IsEmptyFinalizer(SyntaxNode methodBody, CodeBlockAnalysisContext analysisContext);
    }
}
