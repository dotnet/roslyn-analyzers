// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        internal enum ProblemKind
        {
            IncorrectMessage,
            IncorrectParameterName,
            NoArguments,
            SwappedMessageAndParameterName
        }

        private struct BadArguments
        {
            public string BadMessage { get; set; } // Represents a parameter name
            public string BadParameterName { get; set; } // Represents a message

            public IParameterSymbol MessageParameter { get; set; }
            public IParameterSymbol ParameterNameParameter { get; set; }
        }

        internal const string RuleId = "CA2208";
        private const string HelpUri = "https://msdn.microsoft.com/en-us/library/ms182347.aspx";

        internal const string ProblemKindProperty = "ProblemKind";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageNoArguments = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageNoArguments), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIncorrectMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageIncorrectParameterName = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageIncorrectParameterName), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageSwappedMessageAndParameterName = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyMessageSwappedMessageAndParameterName), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.InstantiateArgumentExceptionsCorrectlyDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             "{0}",
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                compilationContext =>
                {
                    Compilation compilation = compilationContext.Compilation;
                    ITypeSymbol argumentExceptionType = WellKnownTypes.ArgumentException(compilation);

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

        internal static bool IsMessage(IParameterSymbol parameter)
        {
            return parameter?.Type.SpecialType == SpecialType.System_String
                && parameter?.Name == "message";
        }

        internal static bool IsParameterName(IParameterSymbol parameter)
        {
            if (parameter?.Type.SpecialType != SpecialType.System_String)
            {
                return false;
            }

            switch (parameter?.Name)
            {
                case "paramName":
                case "parameterName":
                    return true;
                default:
                    return false;
            }
        }

        private static void AnalyzeObjectCreation(
            OperationAnalysisContext context,
            ISymbol owningSymbol,
            ITypeSymbol argumentExceptionType)
        {
            var creation = (IObjectCreationOperation)context.Operation;
            if (!creation.Type.Inherits(argumentExceptionType))
            {
                return;
            }

            if (creation.Arguments.Length == 0)
            {
                if (HasMessageOrParameterNameConstructor(creation.Type))
                {
                    // Call the {0} constructor that contains a message and/ or paramName parameter
                    var builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder.Add(ProblemKindProperty, nameof(ProblemKind.NoArguments));
                    ReportDiagnostic(context, s_localizableMessageNoArguments, builder.ToImmutable(), creation.Type.Name);
                }
            }
            else
            {
                var badArguments = GetBadArguments(creation, owningSymbol);
                var builder = ImmutableDictionary.CreateBuilder<string, string>();

                string methodName = owningSymbol.Name;
                string messageName = badArguments.MessageParameter?.Name;
                string parameterNameName = badArguments.ParameterNameParameter?.Name;
                string exceptionName = creation.Type.Name;

                if (badArguments.BadMessage != null && badArguments.BadParameterName != null)
                {
                    builder.Add(ProblemKindProperty, nameof(ProblemKind.SwappedMessageAndParameterName));
                    ReportDiagnostic(
                        context,
                        s_localizableMessageSwappedMessageAndParameterName,
                        builder.ToImmutable(),
                        methodName,
                        messageName,
                        parameterNameName,
                        exceptionName);
                }
                else if (badArguments.BadMessage != null)
                {
                    builder.Add(ProblemKindProperty, nameof(ProblemKind.IncorrectMessage));
                    ReportDiagnostic(
                        context,
                        s_localizableMessageIncorrectMessage,
                        builder.ToImmutable(),
                        owningSymbol.Name,
                        badArguments.BadMessage,
                        messageName,
                        exceptionName);
                }
                else if (badArguments.BadParameterName != null)
                {
                    builder.Add(ProblemKindProperty, nameof(ProblemKind.IncorrectParameterName));
                    ReportDiagnostic(
                        context,
                        s_localizableMessageIncorrectParameterName,
                        builder.ToImmutable(),
                        owningSymbol.Name,
                        badArguments.BadParameterName,
                        parameterNameName,
                        exceptionName);
                }
            }
        }

        private static BadArguments GetBadArguments(IObjectCreationOperation creation, ISymbol owningSymbol)
        {
            var badArguments = new BadArguments();

            foreach (IArgumentOperation argument in creation.Arguments)
            {
                if (argument.Parameter.Type.SpecialType != SpecialType.System_String)
                {
                    continue;
                }

                string value = argument.Value.ConstantValue.HasValue ? argument.Value.ConstantValue.Value as string : null;
                if (value == null)
                {
                    continue;
                }

                CheckArgument(owningSymbol, argument.Parameter, value, ref badArguments);
            }

            return badArguments;
        }

        private static void CheckArgument(
            ISymbol targetSymbol,
            IParameterSymbol parameter,
            string stringArgument,
            ref BadArguments badArguments)
        {
            bool matchesParameterName = MatchesNameOfAnyParameter(targetSymbol, stringArgument);

            if (IsMessage(parameter) && matchesParameterName)
            {
                badArguments.BadMessage = stringArgument;
                badArguments.MessageParameter = parameter;
            }
            else if (IsParameterName(parameter) && !matchesParameterName)
            {
                badArguments.BadParameterName = stringArgument;
                badArguments.ParameterNameParameter = parameter;
            }
        }

        private static void ReportDiagnostic(
            OperationAnalysisContext context,
            LocalizableString format,
            ImmutableDictionary<string, string> properties,
            params object[] args)
        {
            Debug.Assert(properties.ContainsKey(ProblemKindProperty));

            context.ReportDiagnostic(
                context.Operation.Syntax.CreateDiagnostic(
                    Descriptor,
                    properties,
                    string.Format(format.ToString(), args)));
        }

        private static bool HasMessageOrParameterNameConstructor(ITypeSymbol type)
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
                        && (IsMessage(parameter) || IsParameterName(parameter)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool MatchesNameOfAnyParameter(ISymbol symbol, string stringArgumentValue)
        {
            foreach (IParameterSymbol parameter in symbol.GetParameters())
            {
                if (parameter.Name == stringArgumentValue)
                {
                    return true;
                }
            }

            return false;
        }
    }
}