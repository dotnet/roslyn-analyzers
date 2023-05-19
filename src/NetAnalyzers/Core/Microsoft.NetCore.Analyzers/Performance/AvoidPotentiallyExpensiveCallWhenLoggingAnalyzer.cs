// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal sealed class AvoidPotentiallyExpensiveCallWhenLoggingAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor s_rule = DiagnosticDescriptorHelper.Create(
            "CA1862",
            AvoidPotentiallyExpensiveCallWhenLoggingTitle,
            AvoidPotentiallyExpensiveCallWhenLoggingMessage,
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            AvoidPotentiallyExpensiveCallWhenLoggingDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterCompilationStartAction(context =>
            {
                var loggerMessageAttribute = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerMessageAttribute);
                var loggerExtensionsSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerExtensions);
                var iloggerLogMethodSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingILogger)?.GetMembers("Log").FirstOrDefault() as IMethodSymbol;
                context.RegisterOperationAction(context => AnalyzeInvocationOperation(context, loggerMessageAttribute, loggerExtensionsSymbol, iloggerLogMethodSymbol), OperationKind.Invocation);
            });

        }

        private static void AnalyzeInvocationOperation(OperationAnalysisContext context, INamedTypeSymbol? loggerMessageAttribute, INamedTypeSymbol? loggerExtensionsSymbol, IMethodSymbol? iloggerLogMethodSymbol)
        {
            var invocation = (IInvocationOperation)context.Operation;
            if (!IsValidLoggingInvocation(invocation, loggerMessageAttribute, loggerExtensionsSymbol, iloggerLogMethodSymbol))
            {
                return;
            }

            foreach (var argument in invocation.Arguments)
            {
                if (!IsGoodArgument(argument))
                {
                    context.ReportDiagnostic(argument.CreateDiagnostic(s_rule));
                }
            }
        }

        private static bool IsGoodArgument(IArgumentOperation argumentOperation)
        {
            if (argumentOperation.Value is ILiteralOperation or ILocalReferenceOperation)
            {
                return true;
            }

            return IsGoodArgumentRecursive(argumentOperation.Value);
        }

        private static bool IsGoodArgumentRecursive(IOperation operationValue)
        {
            if (operationValue is null)
            {
                return true;
            }

            if (operationValue is IMemberReferenceOperation memberReference)
            {
                return IsGoodArgumentRecursive(memberReference.Instance);
            }

            if (operationValue is IArrayElementReferenceOperation arrayElementReference)
            {
                return IsGoodArgumentRecursive(arrayElementReference.ArrayReference);
            }

            return false;
        }

        private static bool IsValidLoggingInvocation(IInvocationOperation invocation, INamedTypeSymbol? loggerMessageAttribute, INamedTypeSymbol? loggerExtensionsSymbol, IMethodSymbol? iloggerLogMethodSymbol)
        {
            var method = invocation.TargetMethod;
            if (method.Equals(iloggerLogMethodSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }

            if (method.ContainingType.Equals(loggerExtensionsSymbol, SymbolEqualityComparer.Default))
            {
                return true;
            }

            if (loggerMessageAttribute is not null &&
                method.GetAttributes().Any((att, arg) => att.AttributeClass.Equals(arg, SymbolEqualityComparer.Default), loggerMessageAttribute))
            {
                return true;
            }

            return false;
        }
    }
}