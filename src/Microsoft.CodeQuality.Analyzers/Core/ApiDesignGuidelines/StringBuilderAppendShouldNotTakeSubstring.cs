// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
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

            context.RegisterCompilationStartAction(this.OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext startContext)
        {
            var compilation = startContext.Compilation;

            var stringSymbol = WellKnownTypes.String(compilation);

            if (stringSymbol is null)
            {
                return;
            }

            var substring1ParameterMethod = stringSymbol.GetMembers(nameof(string.Substring)).OfType<IMethodSymbol>()
                .SingleOrDefault(substring => substring.Parameters.Length == 1
                                    && substring.Parameters[0].Type.SpecialType == SpecialType.System_Int32);


            if (substring1ParameterMethod is null)
            {
                return;
            }

            var substring2ParameterMethod = stringSymbol.GetMembers(nameof(string.Substring)).OfType<IMethodSymbol>()
                .SingleOrDefault(substring => substring.Parameters.Length == 2
                                              && substring.Parameters[0].Type.SpecialType == SpecialType.System_Int32
                                              && substring.Parameters[1].Type.SpecialType == SpecialType.System_Int32);

            if (substring2ParameterMethod is null)
            {
                return;
            }

            var stringBuilderSymbol = WellKnownTypes.StringBuilder(compilation);

            if (stringBuilderSymbol is null)
            {
                return;
            }

            var sourceAppendMethod = stringBuilderSymbol
                .GetMembers(nameof(StringBuilder.Append))
                .OfType<IMethodSymbol>()
                .Where(append => append.Parameters.Length == 1)
                .SingleOrDefault(append => append.Parameters[0].Type == stringSymbol);

            if (sourceAppendMethod is null)
            {
                return;
            }
            
            startContext.RegisterOperationAction(
                context =>
                {
                    if (context.Operation is IInvocationOperation invocation)
                    {
                        var invokedMethod = invocation.TargetMethod;

                        if (invokedMethod == sourceAppendMethod)
                        {
                            var parameters = invokedMethod.Parameters;
                            if (parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String)
                            {
                                var argument = invocation.Arguments.FirstOrDefault();
                                if (argument.Value is IInvocationOperation invocationExpression)
                                {
                                    if (invocationExpression.TargetMethod == substring1ParameterMethod)
                                    {
                                        bool stringParameterIsSafeToReuse =
                                            invocationExpression.Instance.ConstantValue.HasValue
                                            || invocationExpression.Instance.Kind == OperationKind.ConstantPattern
                                            || invocationExpression.Instance.Kind == OperationKind.ArrayElementReference
                                            || invocationExpression.Instance.Kind == OperationKind.FieldReference
                                            || invocationExpression.Instance.Kind == OperationKind.InstanceReference
                                            || invocationExpression.Instance.Kind == OperationKind.Literal
                                            || invocationExpression.Instance.Kind == OperationKind.LocalReference
                                            || invocationExpression.Instance.Kind == OperationKind.NameOf
                                            || invocationExpression.Instance.Kind == OperationKind.ParameterReference;

                                        if (stringParameterIsSafeToReuse)
                                        {
                                            context.ReportDiagnostic(
                                                Diagnostic.Create(
                                                    RuleReplaceOneParameter,
                                                    invocation.Syntax.GetLocation()));
                                        }                                        
                                    }
                                    else if (invocationExpression.TargetMethod == substring2ParameterMethod)
                                    {
                                        context.ReportDiagnostic(
                                            Diagnostic.Create(
                                                RuleReplaceTwoParameter,
                                                invocation.Syntax.GetLocation()));
                                    }
                                }
                            }
                        }
                    }
                },
                OperationKind.Invocation);
        }
    }
}