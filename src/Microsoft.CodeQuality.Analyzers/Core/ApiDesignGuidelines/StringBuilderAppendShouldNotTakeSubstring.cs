using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class StringBuilderAppendShouldNotTakeSubstring : DiagnosticAnalyzer
    {
        internal const string RuleIdOneParameterId = "_1638_Substring1";
        internal const string RuleIdTwoParameterId = "_1638_Substring2";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterTitle), 
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, 
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterDefault),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithOneParameterDescription),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableTitle2 = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterTitle),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, 
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDefault2 = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterDefault),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription2 = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterDescription),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, 
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor RuleReplaceOneParameter = new DiagnosticDescriptor(
            RuleIdOneParameterId,
            s_localizableTitle,
            s_localizableMessageDefault,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "", // TODO (might require an addition to docs repository?
            customTags: WellKnownDiagnosticTags.AnalyzerException);

        internal static DiagnosticDescriptor RuleReplaceTwoParameter = new DiagnosticDescriptor(
            RuleIdTwoParameterId,
            s_localizableTitle2,
            s_localizableMessageDefault2,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription2,
            helpLinkUri: "", // TODO (might require an addition to docs repository?
            customTags: WellKnownDiagnosticTags.AnalyzerException);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleReplaceOneParameter, RuleReplaceTwoParameter);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(this.Action, OperationKind.Invocation);
        }

        private void Action(OperationAnalysisContext context)
        {
            if (context.Operation is IInvocationOperation invocation)
            {
                var invokedMethod = invocation.TargetMethod;

                if (invokedMethod.ContainingSymbol is INamedTypeSymbol namedType
                    && namedType.Name == nameof(StringBuilder) 
                    && namedType.ContainingNamespace.Name == nameof(System.Text)
                    && namedType.ContainingNamespace.ContainingNamespace.Name == nameof(System)
                    && namedType.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
                    && invokedMethod.Name == nameof(StringBuilder.Append))
                {
                    var parameters = invokedMethod.Parameters;
                    if (parameters.Length == 1 && parameters[0].Type.Name == nameof(String))
                    {
                        IArgumentOperation argument = invocation.Arguments.FirstOrDefault();
                        if (argument.Value is IInvocationOperation invocationExpression
                            && invocationExpression.TargetMethod is IMethodSymbol parameterMethod
                            && parameterMethod.Name == nameof(String.Substring)
                            && parameterMethod.ContainingSymbol.Name == nameof(String)
                            && parameterMethod.ContainingSymbol.ContainingNamespace.Name == nameof(System)
                            && parameterMethod.ContainingSymbol.ContainingNamespace.ContainingNamespace.IsGlobalNamespace)
                        {
                            switch (parameterMethod.Parameters.Length)
                            {
                                case 1:
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            RuleReplaceOneParameter, 
                                            invocation.Syntax.GetLocation()));
                                    break;
                                case 2:
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            RuleReplaceTwoParameter,
                                            invocation.Syntax.GetLocation()));
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}