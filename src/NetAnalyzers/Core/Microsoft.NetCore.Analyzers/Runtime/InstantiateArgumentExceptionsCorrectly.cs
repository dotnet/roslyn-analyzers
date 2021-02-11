// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2208: Instantiate argument exceptions correctly
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class InstantiateArgumentExceptionsCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2208";
        internal const string MessagePosition = nameof(MessagePosition);

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNoArguments = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageNoArguments), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIncorrectMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIncorrectParameterName = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.InstantiateArgumentExceptionsCorrectlyDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor RuleNoArguments = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNoArguments,
                                                                             DiagnosticCategory.Usage,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);

        internal static DiagnosticDescriptor RuleIncorrectMessage = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIncorrectMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);

        internal static DiagnosticDescriptor RuleIncorrectParameterName = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageIncorrectParameterName,
                                                                             DiagnosticCategory.Usage,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: true,
                                                                             isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleNoArguments, RuleIncorrectMessage, RuleIncorrectParameterName);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(
                compilationContext =>
                {
                    Compilation compilation = compilationContext.Compilation;
                    ITypeSymbol? argumentExceptionType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemArgumentException);

                    if (argumentExceptionType == null)
                    {
                        return;
                    }

                    compilationContext.RegisterOperationAction(
                        operationContext => AnalyzeObjectCreation(
                            operationContext,
                            operationContext.ContainingSymbol,
                            argumentExceptionType),
                        OperationKind.ObjectCreation);
                });
        }

        private static void AnalyzeObjectCreation(
            OperationAnalysisContext context,
            ISymbol owningSymbol,
            ITypeSymbol argumentExceptionType)
        {
            var creation = (IObjectCreationOperation)context.Operation;
            if (!creation.Type.Inherits(argumentExceptionType) || !MatchesConfiguredVisibility(owningSymbol, context) || !HasParameterNameConstructor(creation.Type))
            {
                return;
            }

            if (creation.Arguments.IsEmpty)
            {
                if (HasParameters(owningSymbol))
                {
                    // Call the {0} constructor that contains a message and/ or paramName parameter
                    context.ReportDiagnostic(context.Operation.Syntax.CreateDiagnostic(RuleNoArguments, creation.Type.Name));
                }
            }
            else
            {
                Diagnostic? diagnostic = null;
                foreach (IArgumentOperation argument in creation.Arguments)
                {
                    if (argument.Parameter.Type.SpecialType != SpecialType.System_String)
                    {
                        continue;
                    }

                    string? value = argument.Value.ConstantValue.HasValue ? argument.Value.ConstantValue.Value as string : null;
                    if (value == null)
                    {
                        continue;
                    }

                    diagnostic = CheckArgument(owningSymbol, creation, argument.Parameter, value, context);

                    // RuleIncorrectMessage is the highest priority rule, no need to check other rules
                    if (diagnostic != null && diagnostic.Descriptor.Equals(RuleIncorrectMessage))
                    {
                        break;
                    }
                }

                if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool MatchesConfiguredVisibility(ISymbol owningSymbol, OperationAnalysisContext context) =>
             context.Options.MatchesConfiguredVisibility(RuleIncorrectParameterName, owningSymbol, context.Compilation,
                 context.CancellationToken, defaultRequiredVisibility: SymbolVisibilityGroup.All);

        private static bool HasParameters(ISymbol owningSymbol) => !owningSymbol.GetParameters().IsEmpty;

        private static Diagnostic? CheckArgument(
            ISymbol targetSymbol,
            IObjectCreationOperation creation,
            IParameterSymbol parameter,
            string stringArgument,
            OperationAnalysisContext context)
        {
            bool matchesParameter = MatchesParameter(targetSymbol, creation, stringArgument);

            if (IsMessage(parameter) && matchesParameter)
            {
                var dictBuilder = ImmutableDictionary.CreateBuilder<string, string?>();
                dictBuilder.Add(MessagePosition, parameter.Ordinal.ToString(CultureInfo.InvariantCulture));
                return context.Operation.CreateDiagnostic(RuleIncorrectMessage, dictBuilder.ToImmutable(), targetSymbol.Name, stringArgument, parameter.Name, creation.Type.Name);
            }
            else if (HasParameters(targetSymbol) && IsParameterName(parameter) && !matchesParameter)
            {
                // Allow argument exceptions in accessors to use the associated property symbol name.
                if (!MatchesAssociatedSymbol(targetSymbol, stringArgument))
                {
                    return context.Operation.CreateDiagnostic(RuleIncorrectParameterName, targetSymbol.Name, stringArgument, parameter.Name, creation.Type.Name);
                }
            }

            return null;
        }

        private static bool IsMessage(IParameterSymbol parameter)
        {
            return parameter.Name == "message";
        }

        private static bool IsParameterName(IParameterSymbol parameter)
        {
            return parameter.Name is "paramName" or "parameterName";
        }

        private static bool HasParameterNameConstructor(ITypeSymbol type)
        {
            foreach (ISymbol member in type.GetMembers())
            {
                if (!member.IsConstructor())
                {
                    continue;
                }

                foreach (IParameterSymbol parameter in member.GetParameters())
                {
                    if (parameter.Type.SpecialType == SpecialType.System_String
                        && IsParameterName(parameter))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool MatchesParameter(ISymbol? symbol, IObjectCreationOperation creation, string stringArgumentValue)
        {
            if (MatchesParameterCore(symbol, stringArgumentValue))
            {
                return true;
            }

            var operation = creation.Parent;
            while (operation != null)
            {
                symbol = null;
                switch (operation.Kind)
                {
                    case OperationKind.LocalFunction:
                        symbol = ((ILocalFunctionOperation)operation).Symbol;
                        break;

                    case OperationKind.AnonymousFunction:
                        symbol = ((IAnonymousFunctionOperation)operation).Symbol;
                        break;
                }

                if (symbol != null && MatchesParameterCore(symbol, stringArgumentValue))
                {
                    return true;
                }

                operation = operation.Parent;
            }

            return false;
        }

        private static bool MatchesParameterCore(ISymbol? symbol, string stringArgumentValue)
        {
            foreach (IParameterSymbol parameter in symbol.GetParameters())
            {
                if (parameter.Name == stringArgumentValue)
                {
                    return true;
                }
            }

            if (symbol is IMethodSymbol method)
            {
                if (method.IsGenericMethod)
                {
                    foreach (ITypeParameterSymbol parameter in method.TypeParameters)
                    {
                        if (parameter.Name == stringArgumentValue)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool MatchesAssociatedSymbol(ISymbol targetSymbol, string stringArgument)
            => targetSymbol.IsAccessorMethod() &&
            ((IMethodSymbol)targetSymbol).AssociatedSymbol?.Name == stringArgument;
    }
}