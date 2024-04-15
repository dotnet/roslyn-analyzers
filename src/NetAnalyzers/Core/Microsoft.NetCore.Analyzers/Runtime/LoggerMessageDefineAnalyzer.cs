// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeQuality.Analyzers;
using Microsoft.Extensions.Logging;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftCodeQualityAnalyzersResources;

    /// <summary>
    /// CA1727: <inheritdoc cref="LoggerMessageDiagnosticUsePascalCasedLogMessageTokensTitle"/>
    /// CA1848: <inheritdoc cref="LoggerMessageDiagnosticUseCompiledLogMessagesTitle"/>
    /// CA2253: <inheritdoc cref="LoggerMessageDiagnosticNumericsInFormatStringTitle"/>
    /// CA2254: <inheritdoc cref="LoggerMessageDiagnosticConcatenationInFormatStringTitle"/>
    /// CA2017: <inheritdoc cref="LoggerMessageDiagnosticFormatParameterCountMismatchTitle"/>
    /// CA2023: <inheritdoc cref="LoggerMessageDiagnosticMessageTemplateBracesMismatchTitle"/>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class LoggerMessageDefineAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA1727RuleId = "CA1727";
        internal const string CA1848RuleId = "CA1848";
        internal const string CA2253RuleId = "CA2253";
        internal const string CA2254RuleId = "CA2254";
        internal const string CA2017RuleId = "CA2017";
        internal const string CA2023RuleId = "CA2023";

        internal static readonly DiagnosticDescriptor CA1727Rule = DiagnosticDescriptorHelper.Create(CA1727RuleId,
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticUsePascalCasedLogMessageTokensTitle)),
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticUsePascalCasedLogMessageTokensMessage)),
                                                                         DiagnosticCategory.Naming,
                                                                         RuleLevel.IdeHidden_BulkConfigurable,
                                                                         description: CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticUsePascalCasedLogMessageTokensDescription)),
                                                                         isPortedFxCopRule: false,
                                                                         isDataflowRule: false,
                                                                         isReportedAtCompilationEnd: false);

        internal static readonly DiagnosticDescriptor CA1848Rule = DiagnosticDescriptorHelper.Create(CA1848RuleId,
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticUseCompiledLogMessagesTitle)),
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticUseCompiledLogMessagesMessage)),
                                                                         DiagnosticCategory.Performance,
                                                                         RuleLevel.IdeHidden_BulkConfigurable,
                                                                         description: CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticUseCompiledLogMessagesDescription)),
                                                                         isPortedFxCopRule: false,
                                                                         isDataflowRule: false,
                                                                         isReportedAtCompilationEnd: false);

        internal static readonly DiagnosticDescriptor CA2253Rule = DiagnosticDescriptorHelper.Create(CA2253RuleId,
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticNumericsInFormatStringTitle)),
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticNumericsInFormatStringMessage)),
                                                                         DiagnosticCategory.Usage,
                                                                         RuleLevel.IdeSuggestion,
                                                                         description: CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticNumericsInFormatStringDescription)),
                                                                         isPortedFxCopRule: false,
                                                                         isDataflowRule: false,
                                                                         isReportedAtCompilationEnd: false);

        internal static readonly DiagnosticDescriptor CA2254Rule = DiagnosticDescriptorHelper.Create(CA2254RuleId,
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticConcatenationInFormatStringTitle)),
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticConcatenationInFormatStringMessage)),
                                                                         DiagnosticCategory.Usage,
                                                                         RuleLevel.IdeSuggestion,
                                                                         description: CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticConcatenationInFormatStringDescription)),
                                                                         isPortedFxCopRule: false,
                                                                         isDataflowRule: false,
                                                                         isReportedAtCompilationEnd: false);

        internal static readonly DiagnosticDescriptor CA2017Rule = DiagnosticDescriptorHelper.Create(CA2017RuleId,
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticFormatParameterCountMismatchTitle)),
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticFormatParameterCountMismatchMessage)),
                                                                         DiagnosticCategory.Reliability,
                                                                         RuleLevel.BuildWarning,
                                                                         description: CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticFormatParameterCountMismatchDescription)),
                                                                         isPortedFxCopRule: false,
                                                                         isDataflowRule: false,
                                                                         isReportedAtCompilationEnd: false);

        internal static readonly DiagnosticDescriptor CA2023Rule = DiagnosticDescriptorHelper.Create(CA2023RuleId,
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticMessageTemplateBracesMismatchTitle)),
                                                                         CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticMessageTemplateBracesMismatchMessage)),
                                                                         DiagnosticCategory.Reliability,
                                                                         RuleLevel.BuildError,
                                                                         description: CreateLocalizableResourceString(nameof(LoggerMessageDiagnosticMessageTemplateBracesMismatchDescription)),
                                                                         isPortedFxCopRule: false,
                                                                         isDataflowRule: false,
                                                                         isReportedAtCompilationEnd: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(CA1727Rule, CA1848Rule, CA2253Rule, CA2254Rule, CA2017Rule, CA2023Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(context =>
            {
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);

                if (!wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerExtensions, out var loggerExtensionsType) ||
                    !wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingILogger, out var loggerType) ||
                    !wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerMessage, out var loggerMessageType))
                {
                    return;
                }

                context.RegisterOperationAction(context => AnalyzeInvocation(context, loggerType, loggerExtensionsType, loggerMessageType), OperationKind.Invocation);
            });
        }

        private void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol loggerType, INamedTypeSymbol loggerExtensionsType, INamedTypeSymbol loggerMessageType)
        {
            var invocation = (IInvocationOperation)context.Operation;

            var methodSymbol = invocation.TargetMethod;
            var containingType = methodSymbol.ContainingType;
            bool usingLoggerExtensionsTypes = false;

            if (containingType.Equals(loggerExtensionsType, SymbolEqualityComparer.Default))
            {
                usingLoggerExtensionsTypes = true;
                context.ReportDiagnostic(invocation.CreateDiagnostic(CA1848Rule, methodSymbol.ToDisplayString(GetLanguageSpecificFormat(invocation))));
            }
            else if (
                !containingType.Equals(loggerType, SymbolEqualityComparer.Default) &&
                !containingType.Equals(loggerMessageType, SymbolEqualityComparer.Default))
            {
                return;
            }

            if (FindLogParameters(methodSymbol, out var messageArgument, out var paramsArgument))
            {
                var paramsCount = 0;
                IOperation? formatExpression = null;
                var argsIsArray = false;

                if (containingType.Equals(loggerMessageType, SymbolEqualityComparer.Default))
                {
                    // For LoggerMessage.Define, count type parameters on the invocation instead of arguments
                    paramsCount = methodSymbol.TypeParameters.Length;
                    var arg = invocation.Arguments.FirstOrDefault(argument =>
                    {
                        var parameter = argument.Parameter;
                        return SymbolEqualityComparer.Default.Equals(parameter, messageArgument);
                    });
                    formatExpression = arg?.Value;
                }
                else
                {
                    foreach (var argument in invocation.Arguments)
                    {
                        var parameter = argument.Parameter;
                        if (SymbolEqualityComparer.Default.Equals(parameter, messageArgument))
                        {
                            formatExpression = argument.Value;
                        }
                        else if (SymbolEqualityComparer.Default.Equals(parameter, paramsArgument))
                        {
                            var parameterType = parameter!.Type;
                            if (parameterType == null)
                            {
                                return;
                            }

                            if (argument.Value is IArrayCreationOperation arrayCreation)
                            {
                                paramsCount += arrayCreation.Initializer!.ElementValues.Length;
                            }
                            else
                            {
                                argsIsArray = true;
                                paramsCount++;
                            }
                        }
                    }
                }

                if (formatExpression is not null)
                {
                    AnalyzeFormatArgument(context, formatExpression, paramsCount, argsIsArray, usingLoggerExtensionsTypes, methodSymbol);
                }
            }
        }

        private void AnalyzeFormatArgument(OperationAnalysisContext context, IOperation formatExpression, int paramsCount, bool argsIsArray, bool usingLoggerExtensionsTypes, IMethodSymbol methodSymbol)
        {
            var text = TryGetFormatText(formatExpression);
            if (text == null)
            {
                if (usingLoggerExtensionsTypes)
                {
                    context.ReportDiagnostic(formatExpression.CreateDiagnostic(CA2254Rule, methodSymbol.ToDisplayString(GetLanguageSpecificFormat(formatExpression))));
                }

                return;
            }

            LogValuesFormatter formatter;
            try
            {
                formatter = new LogValuesFormatter(text);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return;
            }

            if (!IsValidBraces(formatter))
            {
                context.ReportDiagnostic(formatExpression.CreateDiagnostic(CA2023Rule));
                return;
            }

            foreach (var valueName in formatter.ValueNames)
            {
                if (int.TryParse(valueName, out _))
                {
                    context.ReportDiagnostic(formatExpression.CreateDiagnostic(CA2253Rule));
                }
                else if (!string.IsNullOrEmpty(valueName) && char.IsLower(valueName[0]))
                {
                    context.ReportDiagnostic(formatExpression.CreateDiagnostic(CA1727Rule));
                }
            }

            var argsPassedDirectly = argsIsArray && paramsCount == 1;
            if (!argsPassedDirectly && paramsCount != formatter.ValueNames.Count)
            {
                context.ReportDiagnostic(formatExpression.CreateDiagnostic(CA2017Rule));
            }
        }

        private static SymbolDisplayFormat GetLanguageSpecificFormat(IOperation operation) =>
            operation.Language == LanguageNames.CSharp ?
                SymbolDisplayFormat.CSharpShortErrorMessageFormat : SymbolDisplayFormat.VisualBasicShortErrorMessageFormat;

        private string? TryGetFormatText(IOperation? argumentExpression)
        {
            if (argumentExpression is null)
                return null;

            switch (argumentExpression)
            {
                case IOperation { ConstantValue: { HasValue: true, Value: string constantValue } }:
                    return constantValue;
                case IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binary:
                    var leftText = TryGetFormatText(binary.LeftOperand);
                    var rightText = TryGetFormatText(binary.RightOperand);

                    if (leftText != null && rightText != null)
                    {
                        return leftText + rightText;
                    }

                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Does the text have valid braces? (no unclosed braces, no braces without an opening, and no unescaped braces)
        /// </summary>
        /// <param name="formatter">The text to check.</param>
        /// <returns>When true braces are valid, false otherwise.</returns>
        private static bool IsValidBraces(LogValuesFormatter formatter)
        {
            var textWithNumericPlaceholders = formatter.Format(); // an easily parsable representation of the template string like "{0}" instead of "{MyTemplateVar}"
            var textWithValuePlaceHoldersRemoved = GetTextWithValuePlaceholdersRemoved(textWithNumericPlaceholders, formatter.ValueNames.Count);
            var textWithEscapedBracesRemoved = GetTextWithEscapedBracesRemoved(textWithValuePlaceHoldersRemoved);

            var stack = new Stack<char>();

            for (var i = 0; i < textWithEscapedBracesRemoved.Length; i++)
            {
                // If we're on a closing bracket...
                if (textWithEscapedBracesRemoved[i].Equals('}'))
                {
                    // and nothing in the stack, invalid
                    if (stack.Count == 0)
                        return false;

                    // pop from the stack as this should be the opening bracket to this closing one
                    stack.Pop();
                }

                // If we're on an opening bracket, push onto stack for tracking
                if (textWithEscapedBracesRemoved[i].Equals('{'))
                {
                    stack.Push(textWithEscapedBracesRemoved[i]);
                }
            }

            // If anything exists in the stack, that means we have an opening without a close
            if (stack.Count != 0)
            {
                return false;
            }

            // Entire text has been evaluated and no issues found
            return true;
        }

        /// <summary>
        /// Get the template message with the numeral variables substituted for placeholders
        /// </summary>
        /// <example>
        /// "My log {0}" -> "My log ___"
        /// </example>
        private static string GetTextWithValuePlaceholdersRemoved(string text, int count)
        {
            // no items to swap to placeholders
            if (count == 0)
            {
                return text;
            }

            const char placeholder = '_';
            var queueOfPlaceholders = new Queue<string>();
            for (var i = 0; i < count; i++)
            {
                queueOfPlaceholders.Enqueue($"{{{i}}}");
            }

            var textWithPlaceholdersRemoved = new StringBuilder();
            var currentSearchString = queueOfPlaceholders.Dequeue();
            for (var i = 0; i < text.Length; i++)
            {
                // look forward to see if this is one of the variable placeholders
                if (text.Substring(i, currentSearchString.Length) == currentSearchString)
                {
                    textWithPlaceholdersRemoved.Append(placeholder, currentSearchString.Length);

                    i += currentSearchString.Length - 1;

                    if (queueOfPlaceholders.Count > 0)
                    {
                        currentSearchString = queueOfPlaceholders.Dequeue();
                    }
                    else
                    {
                        currentSearchString = placeholder.ToString();
                    }

                    continue;
                }

                textWithPlaceholdersRemoved.Append(text[i]);
            }

            return textWithPlaceholdersRemoved.ToString();
        }

        /// <summary>
        /// Get the template message with escaped braces (either opening or closing) substituted with placeholders.
        /// </summary>
        /// <example>
        /// "My log ___}} }} {{" -> "My log _____ __ __"
        /// </example>
        private static string GetTextWithEscapedBracesRemoved(string text)
        {
            var escapableBraces = new HashSet<char>() { '{', '}' };
            const char placeholder = '_';
            var textWithEscapedBracesRemoved = new StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                var evalChar = text[i];
                // the current character is a brace, and another character exists in the loop that is the same character
                if (IsCurrentAndNextCharSameEscapableBrace(text, escapableBraces, i, evalChar))
                {
                    // replace the current character and next with the placeholder text
                    textWithEscapedBracesRemoved.Append(placeholder, 2);

                    // skip the next char, since we know it's the end to this pair
                    i++;
                    continue;
                }

                textWithEscapedBracesRemoved.Append(evalChar);
            }

            return textWithEscapedBracesRemoved.ToString();
        }

        private static bool IsCurrentAndNextCharSameEscapableBrace(string text, HashSet<char> escapableBraces, int i, char evalChar)
        {
            return escapableBraces.Contains(evalChar) && i + 1 < text.Length && text[i + 1] == evalChar;
        }

        private static bool FindLogParameters(IMethodSymbol methodSymbol, [NotNullWhen(true)] out IParameterSymbol? message, out IParameterSymbol? arguments)
        {
            message = null;
            arguments = null;
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (parameter.Type.SpecialType == SpecialType.System_String &&
                    (string.Equals(parameter.Name, "message", StringComparison.Ordinal) ||
                    string.Equals(parameter.Name, "messageFormat", StringComparison.Ordinal) ||
                    string.Equals(parameter.Name, "formatString", StringComparison.Ordinal)))
                {
                    message = parameter;
                }
                // When calling logger.BeginScope("{Param}") generic overload would be selected
                else if (parameter.Type.SpecialType == SpecialType.System_String &&
                    methodSymbol.Name.Equals("BeginScope") &&
                    string.Equals(parameter.Name, "state", StringComparison.Ordinal))
                {
                    message = parameter;
                }
                else if (parameter.IsParams &&
                    string.Equals(parameter.Name, "args", StringComparison.Ordinal))
                {
                    arguments = parameter;
                }
            }

            return message != null;
        }
    }
}