// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

using Analyzer.Utilities;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class InstantiateArgumentExceptionsCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2208";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNoArguments = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageNoArguments), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIncorrectMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIncorrectParameterName = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private const string s_helpUri = "https://msdn.microsoft.com/en-us/library/ms182347.aspx";

        internal static DiagnosticDescriptor NoArgumentsRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNoArguments,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: s_helpUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor IncorrectMessageRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIncorrectMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: s_helpUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor IncorrectParameterNameRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIncorrectParameterName,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: s_helpUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoArgumentsRule, IncorrectMessageRule, IncorrectParameterNameRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                compilationContext =>
                {
                    Compilation compilation = compilationContext.Compilation;
                    ITypeSymbol argumentExceptionType = compilation.GetTypeByMetadataName("System.ArgumentException");

                    if (argumentExceptionType == null)
                        return;

                    compilationContext.RegisterOperationBlockStartAction(
                        operationBlockStartContext =>
                        {
                            operationBlockStartContext.RegisterOperationAction(
                                operationContext => AnalyzeObjectCreation(
                                    operationContext, 
                                    operationBlockStartContext.OwningSymbol, 
                                    argumentExceptionType),
                                OperationKind.ObjectCreationExpression);
                        });
                });
        }

        private void AnalyzeObjectCreation(
            OperationAnalysisContext context, 
            ISymbol owningSymbol,
            ITypeSymbol argumentExceptionType)
        {
            var creation = (IObjectCreationExpression)context.Operation;
            if (!creation.ResultType.Inherits(argumentExceptionType))
                return;
          
            if (creation.ConstructorArguments.Length == 0)
            {
                if (HasMessageOrParameterNameConstructor(creation.ResultType))
                {
                    // Call the {0} constructor that contains a message and/ or paramName parameter
                    context.ReportDiagnostic(
                        creation.Syntax.CreateDiagnostic(NoArgumentsRule, creation.ResultType));
                }
            }
            else
            {
                foreach (var argument in creation.ConstructorArguments)
                {
                    if (argument.Parameter.Type.SpecialType != SpecialType.System_String)
                        continue;
                 
                    string value = argument.Value.ConstantValue as string;
                    if (value == null)
                        continue;

                    CheckArgument(owningSymbol, creation.ResultType, argument.Parameter, value, context);
                }
            }
        }

        private void CheckArgument(
            ISymbol targetSymbol,
            ITypeSymbol constructorType, 
            IParameterSymbol parameter, 
            string stringArgument, 
            OperationAnalysisContext context)
        {
            bool matchesParameter = MatchesParameter(targetSymbol, stringArgument);
            DiagnosticDescriptor descriptor = null;

            if (IsMessage(parameter) && matchesParameter)
            {
                descriptor = IncorrectMessageRule;
            }
            else if (IsParameterName(parameter) && !matchesParameter)
            {
                descriptor = IncorrectParameterNameRule;
            }

            if (descriptor != null)
            {
                // Method {0} passes parameter name '{1}' as the '{2}' argument to a {3} constructor. [...]
                context.ReportDiagnostic(
                    context.Operation.Syntax.CreateDiagnostic(
                        descriptor,
                        targetSymbol,
                        stringArgument,
                        parameter.Name,
                        constructorType));
            }
        }

        private static bool IsMessage(IParameterSymbol parameter)
        {
            return parameter.Name == "message";
        }

        private static bool IsParameterName(IParameterSymbol parameter)
        {
            return parameter.Name == "paramName"
                || parameter.Name == "parameterName"
                || parameter.Name == "argumentName";
        }

        private static bool HasMessageOrParameterNameConstructor(ITypeSymbol type)
        {
            foreach (var member in type.GetMembers())
            {
                if (!member.IsConstructor())
                    continue;

                foreach (var parameter in member.GetParameters())
                {
                    if (parameter.Type.SpecialType == SpecialType.System_String
                        && (IsMessage(parameter) || IsParameterName(parameter)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool MatchesParameter(ISymbol symbol, string stringArgumentValue)
        {
            foreach (var parameter in symbol.GetParameters())
                if (parameter.Name == stringArgumentValue)
                    return true;

            return false;
        }
    }
}