// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    /// <summary>
    /// CA1873: <inheritdoc cref="AvoidPotentiallyExpensiveCallWhenLoggingTitle"/>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidPotentiallyExpensiveCallWhenLoggingAnalyzer : DiagnosticAnalyzer
    {
        private const string RuleId = "CA1873";

        private const string Level = nameof(Level);
        private const string LogLevel = nameof(LogLevel);

        private const string Log = nameof(Log);
        private const string IsEnabled = nameof(IsEnabled);
        private const string LogTrace = nameof(LogTrace);
        private const string LogDebug = nameof(LogDebug);
        private const string LogInformation = nameof(LogInformation);
        private const string LogWarning = nameof(LogWarning);
        private const string LogError = nameof(LogError);
        private const string LogCritical = nameof(LogCritical);

        private const int LogLevelTrace = 0;
        private const int LogLevelDebug = 1;
        private const int LogLevelInformation = 2;
        private const int LogLevelWarning = 3;
        private const int LogLevelError = 4;
        private const int LogLevelCritical = 5;
        private const int LogLevelPassedAsParameter = int.MinValue;

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(AvoidPotentiallyExpensiveCallWhenLoggingTitle)),
            CreateLocalizableResourceString(nameof(AvoidPotentiallyExpensiveCallWhenLoggingMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(AvoidPotentiallyExpensiveCallWhenLoggingDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!RequiredSymbols.TryGetSymbols(context.Compilation, out var symbols))
            {
                return;
            }

            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);

            void AnalyzeInvocation(OperationAnalysisContext context)
            {
                var invocation = (IInvocationOperation)context.Operation;

                // Check if invocation is a logging invocation and capture the log level (either as IOperation or as int, depending if it is dynamic or not).
                // Use these to check if the logging invocation is guarded by 'ILogger.IsEnabled' and bail out if it is.
                if (!symbols.TryGetLogLevel(invocation, out var logLevelArgumentOperation, out var logLevel) ||
                    symbols.IsGuardedByIsEnabled(invocation, logLevel, logLevelArgumentOperation))
                {
                    return;
                }

                var arguments = invocation.Arguments.Skip(invocation.IsExtensionMethodAndHasNoInstance() ? 1 : 0);

                // At this stage we have a logging invocation that is not guarded.
                // Check each argument if it is potentially expensive.
                foreach (var argument in arguments)
                {
                    // Check the argument value after conversions to prevent noise (e.g. implicit conversions from null or from int to EventId).
                    if (IsPotentiallyExpensive(argument.Value.WalkDownConversion()))
                    {
                        // Filter out implicit operations in the case of params arguments.
                        // If we would report the diagnostic on the implicit argument operation, it would flag the whole invocation.
                        var explicitDescendants = argument.Value.GetTopmostExplicitDescendants();

                        if (!explicitDescendants.IsEmpty)
                        {
                            context.ReportDiagnostic(explicitDescendants[0].CreateDiagnostic(Rule));
                        }
                    }
                }
            }
        }

        private static bool IsPotentiallyExpensive(IOperation? operation)
        {
            if (operation is null
                // Implicit params array creation is treated as not expensive. This would otherwise cause a lot of noise.
                or IArrayCreationOperation { IsImplicit: true, Initializer.ElementValues.IsEmpty: true }
                or IInstanceReferenceOperation
                or IConditionalAccessInstanceOperation
                or ILiteralOperation
                or ILocalReferenceOperation
                or IParameterReferenceOperation)
            {
                return false;
            }

            if (operation is IPropertyReferenceOperation { Arguments.IsEmpty: false } indexerReference)
            {
                return IsPotentiallyExpensive(indexerReference.Instance) ||
                    indexerReference.Arguments.Any(a => IsPotentiallyExpensive(a.Value));
            }

            if (operation is IArrayElementReferenceOperation arrayElementReference)
            {
                return IsPotentiallyExpensive(arrayElementReference.ArrayReference) ||
                    arrayElementReference.Indices.Any(IsPotentiallyExpensive);
            }

            if (operation is IConditionalAccessOperation conditionalAccess)
            {
                return IsPotentiallyExpensive(conditionalAccess.WhenNotNull);
            }

            if (operation is IMemberReferenceOperation memberReference)
            {
                return IsPotentiallyExpensive(memberReference.Instance);
            }

            return true;
        }

        internal sealed class RequiredSymbols
        {
            private RequiredSymbols(
                IMethodSymbol logMethod,
                IMethodSymbol isEnabledMethod,
                ImmutableDictionary<IMethodSymbol, int> logExtensionsMethodsAndLevel,
                INamedTypeSymbol? loggerMessageAttributeType)
            {
                _logMethod = logMethod;
                _isEnabledMethod = isEnabledMethod;
                _logExtensionsMethodsAndLevel = logExtensionsMethodsAndLevel;
                _loggerMessageAttributeType = loggerMessageAttributeType;
            }

            public static bool TryGetSymbols(Compilation compilation, [NotNullWhen(true)] out RequiredSymbols? symbols)
            {
                symbols = default;

                var iLoggerType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingILogger);

                if (iLoggerType is null)
                {
                    return false;
                }

                var logMethod = iLoggerType.GetMembers(Log)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault();

                var isEnabledMethod = iLoggerType.GetMembers(IsEnabled)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault();

                if (logMethod is null || isEnabledMethod is null)
                {
                    return false;
                }

                var loggerExtensionsType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerExtensions);
                var logExtensionsMethodsBuilder = ImmutableDictionary.CreateBuilder<IMethodSymbol, int>(SymbolEqualityComparer.Default);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(LogTrace).OfType<IMethodSymbol>(), LogLevelTrace);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(LogDebug).OfType<IMethodSymbol>(), LogLevelDebug);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(LogInformation).OfType<IMethodSymbol>(), LogLevelInformation);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(LogWarning).OfType<IMethodSymbol>(), LogLevelWarning);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(LogError).OfType<IMethodSymbol>(), LogLevelError);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(LogCritical).OfType<IMethodSymbol>(), LogLevelCritical);
                AddRangeIfNotNull(logExtensionsMethodsBuilder, loggerExtensionsType?.GetMembers(Log).OfType<IMethodSymbol>(), LogLevelPassedAsParameter);

                var loggerMessageAttributeType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerMessageAttribute);

                symbols = new RequiredSymbols(logMethod, isEnabledMethod, logExtensionsMethodsBuilder.ToImmutable(), loggerMessageAttributeType);

                return true;

                void AddRangeIfNotNull(ImmutableDictionary<IMethodSymbol, int>.Builder builder, IEnumerable<IMethodSymbol>? range, int value)
                {
                    if (range is not null)
                    {
                        builder.AddRange(range.Select(s => new KeyValuePair<IMethodSymbol, int>(s, value)));
                    }
                }
            }

            public bool TryGetLogLevel(IInvocationOperation invocation, out IArgumentOperation? logLevelArgumentOperation, out int logLevel)
            {
                logLevelArgumentOperation = default;
                logLevel = LogLevelPassedAsParameter;

                var method = invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod;

                // ILogger.Log
                if (SymbolEqualityComparer.Default.Equals(method.ConstructedFrom, _logMethod) ||
                    method.ConstructedFrom.IsOverrideOrImplementationOfInterfaceMember(_logMethod))
                {
                    logLevelArgumentOperation = invocation.Arguments.GetArgumentForParameterAtIndex(0);

                    return true;
                }

                // LoggerExtensions.Log and named variants (e.g. LoggerExtensions.LogInformation)
                if (_logExtensionsMethodsAndLevel.TryGetValue(method, out logLevel))
                {
                    // LoggerExtensions.Log
                    if (logLevel == LogLevelPassedAsParameter)
                    {
                        logLevelArgumentOperation = invocation.Arguments.GetArgumentForParameterAtIndex(invocation.IsExtensionMethodAndHasNoInstance() ? 1 : 0);
                    }

                    return true;
                }

                var loggerMessageAttribute = method.GetAttribute(_loggerMessageAttributeType);

                if (loggerMessageAttribute is null)
                {
                    return false;
                }

                // Try to get the log level from the attribute arguments.
                logLevel = loggerMessageAttribute.NamedArguments
                    .FirstOrDefault(p => p.Key.Equals(Level, StringComparison.Ordinal))
                    .Value.Value as int?
                    ?? LogLevelPassedAsParameter;

                if (logLevel == LogLevelPassedAsParameter)
                {
                    logLevelArgumentOperation = invocation.Arguments
                        .FirstOrDefault(a => a.Value.Type?.Name.Equals(LogLevel, StringComparison.Ordinal) ?? false);

                    if (logLevelArgumentOperation is null)
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool IsGuardedByIsEnabled(IInvocationOperation logInvocation, int logLevel, IArgumentOperation? logLevelArgumentOperation)
            {
                var currentAncestorConditional = logInvocation.GetAncestor<IConditionalOperation>(OperationKind.Conditional);
                while (currentAncestorConditional is not null)
                {
                    var conditionInvocations = currentAncestorConditional.Condition
                        .DescendantsAndSelf()
                        .OfType<IInvocationOperation>();

                    // Check each invocation in the condition to see if it is a valid guard invocation, i.e. same instance and same log level.
                    // This is not perfect (e.g. 'if (logger.IsEnabled(LogLevel.Debug) || true)' is treated as guarded), but should be good enough to prevent false positives.
                    if (conditionInvocations.Any(IsValidIsEnabledGuardInvocation))
                    {
                        return true;
                    }

                    currentAncestorConditional = currentAncestorConditional.GetAncestor<IConditionalOperation>(OperationKind.Conditional);
                }

                return false;

                bool IsValidIsEnabledGuardInvocation(IInvocationOperation invocation)
                {
                    if (!SymbolEqualityComparer.Default.Equals(_isEnabledMethod, invocation.TargetMethod) &&
                        !invocation.TargetMethod.IsOverrideOrImplementationOfInterfaceMember(_isEnabledMethod))
                    {
                        return false;
                    }

                    return AreInvocationsOnSameInstance(logInvocation, invocation) && IsSameLogLevel(invocation.Arguments[0]);
                }

                static bool AreInvocationsOnSameInstance(IInvocationOperation invocation1, IInvocationOperation invocation2)
                {
                    return (invocation1.GetInstance()?.WalkDownConversion(), invocation2.GetInstance()?.WalkDownConversion()) switch
                    {
                        (IFieldReferenceOperation fieldRef1, IFieldReferenceOperation fieldRef2) => fieldRef1.Member == fieldRef2.Member,
                        (IPropertyReferenceOperation propRef1, IPropertyReferenceOperation propRef2) => propRef1.Member == propRef2.Member,
                        (IParameterReferenceOperation paramRef1, IParameterReferenceOperation paramRef2) => paramRef1.Parameter == paramRef2.Parameter,
                        (ILocalReferenceOperation localRef1, ILocalReferenceOperation localRef2) => localRef1.Local == localRef2.Local,
                        _ => false,
                    };
                }

                bool IsSameLogLevel(IArgumentOperation isEnabledArgument)
                {
                    if (isEnabledArgument.Value.ConstantValue.HasValue)
                    {
                        int isEnabledLogLevel = (int)isEnabledArgument.Value.ConstantValue.Value!;

                        return logLevel == LogLevelPassedAsParameter
                            ? logLevelArgumentOperation?.Value.HasConstantValue(isEnabledLogLevel) ?? false
                            : isEnabledLogLevel == logLevel;
                    }

                    return isEnabledArgument.Value.GetReferencedMemberOrLocalOrParameter() == logLevelArgumentOperation?.Value.GetReferencedMemberOrLocalOrParameter();
                }
            }

            private readonly IMethodSymbol _logMethod;
            private readonly IMethodSymbol _isEnabledMethod;
            private readonly ImmutableDictionary<IMethodSymbol, int> _logExtensionsMethodsAndLevel;
            private readonly INamedTypeSymbol? _loggerMessageAttributeType;
        }
    }
}
