// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA1820: Test for empty strings using string length.
    /// <para>
    /// Comparing strings using the <see cref="string.Length"/> property or the <see cref="string.IsNullOrEmpty"/> method is significantly faster than using <see cref="string.Equals(string)"/>.
    /// This is because Equals executes significantly more MSIL instructions than either IsNullOrEmpty or the number of instructions executed to retrieve the Length property value and compare it to zero.
    /// </para>
    /// <remarks>NOTE: This rule is not supported for VisualBasic. See https://github.com/dotnet/roslyn-analyzers/issues/2684 for details.</remarks>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestForEmptyStringsUsingStringLengthAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1820";
        private const string StringEmptyFieldName = "Empty";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.TestForEmptyStringsUsingStringLengthTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.TestForEmptyStringsUsingStringLengthMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.TestForEmptyStringsUsingStringLengthDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultForVsixAndNuget,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1820-test-for-empty-strings-using-string-length",
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(
                operationAnalysisContext => AnalyzeInvocationExpression((IInvocationOperation)operationAnalysisContext.Operation, operationAnalysisContext.ReportDiagnostic),
                OperationKind.Invocation);

            context.RegisterOperationAction(
                operationAnalysisContext => AnalyzeBinaryExpression((IBinaryOperation)operationAnalysisContext.Operation, operationAnalysisContext.ReportDiagnostic),
                OperationKind.BinaryOperator);
        }

        /// <summary>
        /// Check to see if we have an invocation to string.Equals that has an empty string as an argument.
        /// </summary>
        private static void AnalyzeInvocationExpression(IInvocationOperation invocationOperation, Action<Diagnostic> reportDiagnostic)
        {
            if (invocationOperation.Arguments.Length > 0)
            {
                IMethodSymbol methodSymbol = invocationOperation.TargetMethod;
                if (methodSymbol != null &&
                    IsStringEqualsMethod(methodSymbol) &&
                    HasAnEmptyStringArgument(invocationOperation))
                {
                    reportDiagnostic(invocationOperation.Syntax.CreateDiagnostic(s_rule));
                }
            }
        }

        /// <summary>
        /// Check to see if we have a equals or not equals expression where an empty string is being
        /// compared.
        /// </summary>
        private static void AnalyzeBinaryExpression(IBinaryOperation binaryOperation, Action<Diagnostic> reportDiagnostic)
        {
            if (binaryOperation.OperatorKind != BinaryOperatorKind.Equals &&
                binaryOperation.OperatorKind != BinaryOperatorKind.NotEquals)
            {
                return;
            }

            if (binaryOperation.LeftOperand.Type?.SpecialType != SpecialType.System_String ||
                binaryOperation.RightOperand.Type?.SpecialType != SpecialType.System_String)
            {
                return;
            }

            if (IsEmptyString(binaryOperation.LeftOperand) || IsEmptyString(binaryOperation.RightOperand))
            {
                reportDiagnostic(binaryOperation.Syntax.CreateDiagnostic(s_rule));
            }
        }


        /// <summary>
        /// Checks if the given method is the string.Equals method.
        /// </summary>
        private static bool IsStringEqualsMethod(IMethodSymbol methodSymbol)
        {
            return string.Equals(methodSymbol.Name, WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal) &&
                   methodSymbol.ContainingType.SpecialType == SpecialType.System_String;
        }

        /// <summary>
        /// Checks if the given expression something that evaluates to a constant string
        /// or the string.Empty field
        /// </summary>
        private static bool IsEmptyString(IOperation expression)
        {
            if (expression == null)
            {
                return false;
            }

            Optional<object> constantValueOpt = expression.ConstantValue;
            if (constantValueOpt.HasValue)
            {
                return (constantValueOpt.Value as string)?.Length == 0;
            }

            if (expression.Kind == OperationKind.FieldReference)
            {
                IFieldSymbol field = ((IFieldReferenceOperation)expression).Field;
                return string.Equals(field.Name, StringEmptyFieldName, StringComparison.Ordinal) &&
                    field.Type.SpecialType == SpecialType.System_String;
            }

            return false;
        }

        /// <summary>
        /// Checks if the given invocation has an argument that is an empty string.
        /// </summary>
        private static bool HasAnEmptyStringArgument(IInvocationOperation invocation)
        {
            return invocation.Arguments.Any(arg => IsEmptyString(arg.Value));
        }
    }
}
