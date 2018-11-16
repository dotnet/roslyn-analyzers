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

        private static readonly LocalizableString s_localizableTitle2 = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterTitle),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, 
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageDefault2 = new LocalizableResourceString(
            nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.StringBuilderShouldUseSubstringOverloadWithTwoParameterDefault),
            MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager,
            typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor RuleReplaceOneParameter = new DiagnosticDescriptor(
            RuleIdOneParameterId,
            s_localizableTitle,
            s_localizableMessageDefault,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableMessageDefault,
            helpLinkUri: "", // TODO (might require an addition to docs repository?
            customTags: WellKnownDiagnosticTags.AnalyzerException);

        internal static DiagnosticDescriptor RuleReplaceTwoParameter = new DiagnosticDescriptor(
            RuleIdTwoParameterId,
            s_localizableTitle2,
            s_localizableMessageDefault2,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableMessageDefault2,
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
            var stringSymbol = WellKnownTypes.String(startContext.Compilation);

            if (stringSymbol is null)
            {
                return;
            }

            var substringMethods = stringSymbol.GetMembers(nameof(string.Substring)).OfType<IMethodSymbol>();
            var substring1ParameterMethod = substringMethods
                .FirstOrDefault(substring => substring.Parameters.Length == 1
                                    && substring.Parameters[0].Type.SpecialType == SpecialType.System_Int32);

            if (substring1ParameterMethod is null)
            {
                return;
            }

            var substring2ParameterMethod = substringMethods
                .FirstOrDefault(substring => substring.Parameters.Length == 2
                                              && substring.Parameters[0].Type.SpecialType == SpecialType.System_Int32
                                              && substring.Parameters[1].Type.SpecialType == SpecialType.System_Int32);

            if (substring2ParameterMethod is null)
            {
                return;
            }

            var stringBuilderSymbol = WellKnownTypes.StringBuilder(startContext.Compilation);

            if (stringBuilderSymbol is null)
            {
                return;
            }

            var sourceAppendMethod = stringBuilderSymbol
                .GetMembers(nameof(StringBuilder.Append))
                .OfType<IMethodSymbol>()
                .Where(append => append.Parameters.Length == 1)
                .FirstOrDefault(append => append.Parameters[0].Type.SpecialType == SpecialType.System_String);

            if (sourceAppendMethod is null)
            {
                return;
            }
            
            startContext.RegisterOperationAction(
                context =>
                {
                    var invocation = (IInvocationOperation) context.Operation;
                    var invokedMethod = invocation.TargetMethod;

                    if (invokedMethod == sourceAppendMethod)
                    {
                        var argument = invocation.Arguments.FirstOrDefault();
                        if (argument.Value is IInvocationOperation invocationExpression)
                        {
                            if (invocationExpression.TargetMethod == substring1ParameterMethod)
                            {
                                if (IsSideEffectFree(invocationExpression.Instance)
                                    && IsSideEffectFree(invocationExpression.Arguments[0].Value))
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
                },
                OperationKind.Invocation);
        }

        private bool IsSideEffectFree(IOperation instance)
        {
            return instance.ConstantValue.HasValue
                   || instance.Kind == OperationKind.ConstantPattern
                   || instance.Kind == OperationKind.ArrayElementReference
                   || instance.Kind == OperationKind.FieldReference
                   || instance.Kind == OperationKind.InstanceReference
                   || instance.Kind == OperationKind.Literal
                   || instance.Kind == OperationKind.LocalReference
                   || instance.Kind == OperationKind.NameOf
                   || instance.Kind == OperationKind.ParameterReference;
        }
    }
}