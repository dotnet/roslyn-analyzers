using System;
using System.Collections.Immutable;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

            context.RegisterSyntaxNodeAction(this.Action, SyntaxKind.InvocationExpression);
        }

        private void Action(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocation)
            {
                var invokedSymbol = context.SemanticModel.GetSymbolInfo(invocation);
                var invokedClass = invokedSymbol.Symbol.ContainingSymbol;
                var invokedMethod = invokedSymbol.Symbol.Name;

                if (invokedClass is INamedTypeSymbol namedType
                    && namedType.ContainingNamespace.Name == nameof(System.Text) 
                    && namedType.ContainingNamespace.ContainingNamespace.Name == nameof(System)
                    && namedType.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
                    && invokedClass.Name == nameof(StringBuilder)
                    && invokedMethod == nameof(StringBuilder.Append)
                    && invokedSymbol.Symbol is IMethodSymbol methodSymbol)
                {
                    var parameters = methodSymbol.Parameters;

                    if (parameters.Length == 1 && parameters[0].Type.Name == nameof(String))
                    {
                        ArgumentSyntax argument = invocation.ArgumentList.Arguments.FirstOrDefault();

                        // todo: what if there's ref? argument.RefOrOutKeyword
                        if (argument?.Expression is InvocationExpressionSyntax invocationExpression)
                        {
                            var innerInvokedSymbol = context.SemanticModel.GetSymbolInfo(invocationExpression);
                            if (innerInvokedSymbol.Symbol is IMethodSymbol parameterMethod
                                && parameterMethod.ContainingSymbol.Name == nameof(String)
                                && parameterMethod.ContainingSymbol.ContainingNamespace.Name == nameof(System)
                                && parameterMethod.ContainingSymbol.ContainingNamespace.ContainingNamespace.IsGlobalNamespace
                                && parameterMethod.Name == nameof(String.Substring))
                            {
                                // we know: sb.Append(text.substring()
                                if (parameterMethod.Parameters.Length == 2)
                                {
                                    //"foo".Substring(StartIndex, Length);
                                    //sb.Append(/* char[] */ "value", /*startIndex*/ 1, /*count*/ 2);
                                    context.ReportDiagnostic(Diagnostic.Create(
                                        RuleReplaceTwoParameter,
                                        invocation.GetLocation()
                                        /*, message.args */));

                                }
                                else if (parameterMethod.Parameters.Length == 1)
                                {
                                    //"foo".Substring(StartIndex)
                                    context.ReportDiagnostic(Diagnostic.Create(
                                        RuleReplaceOneParameter,
                                        invocation.GetLocation()
                                        /*, message args */));
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}