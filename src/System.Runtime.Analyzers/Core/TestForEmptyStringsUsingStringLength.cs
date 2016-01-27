// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA1820: Test for empty strings using string length.
    /// <para>
    /// Comparing strings using the <see cref="string.Length"/> property or the <see cref="string.IsNullOrEmpty"/> method is significantly faster than using <see cref="string.Equals(string)"/>.
    /// This is because Equals executes significantly more MSIL instructions than either IsNullOrEmpty or the number of instructions executed to retrieve the Length property value and compare it to zero.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class TestForEmptyStringsUsingStringLengthAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1820";
        private const string StringEmptyFieldName = "Empty";

        private static LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForEmptyStringsUsingStringLengthTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForEmptyStringsUsingStringLengthMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForEmptyStringsUsingStringLengthDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Performance,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/library/ms182279.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);


        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context) => context.RegisterOperationAction(AnalyzeNode, OperationKind.InvocationExpression, OperationKind.BinaryOperatorExpression);

        private static void AnalyzeNode(OperationAnalysisContext context)
        {
            switch (context.Operation.Kind)
            {
                case OperationKind.InvocationExpression:
                    AnalyzeInvocationExpression(context);
                    break;

                default:
                    AnalyzeBinaryExpression(context);
                    break;
            }
        }

        private static void AnalyzeInvocationExpression(OperationAnalysisContext context)
        {
            var invocationOperation = (IInvocationExpression)context.Operation;
            if (invocationOperation.ArgumentsInSourceOrder.Length > 0)
            {
                var methodSymbol = invocationOperation.TargetMethod;
                if (methodSymbol != null &&
                    IsStringEqualsMethod(methodSymbol) &&
                    HasAnEmptyStringArgument(invocationOperation))
                {
                    context.ReportDiagnostic(invocationOperation.Syntax.CreateDiagnostic(Rule));
                }
            }
        }

        private static void AnalyzeBinaryExpression(OperationAnalysisContext context)
        {
            var binaryOperation = (IBinaryOperatorExpression)context.Operation;

            if (binaryOperation.BinaryOperationKind != BinaryOperationKind.StringEquals &&
                binaryOperation.BinaryOperationKind != BinaryOperationKind.StringNotEquals)
            {
                return;
            }
            
            if (IsEmptyString(binaryOperation.Left) || IsEmptyString(binaryOperation.Right))
            {
                context.ReportDiagnostic(binaryOperation.Syntax.CreateDiagnostic(Rule));
            }
        }

        private static bool IsStringEqualsMethod(IMethodSymbol methodSymbol)
        {
            return string.Equals(methodSymbol.Name, WellKnownMemberNames.ObjectEquals, StringComparison.Ordinal) && 
                   methodSymbol.ContainingType.SpecialType == SpecialType.System_String;
        }

        private static bool IsEmptyString(IExpression expression)
        {
            if (expression == null)
            {
                return false;
            }

            var constantValueOpt = expression.ConstantValue;
            if (constantValueOpt.HasValue)
            {
                return (constantValueOpt.Value as string)?.Length == 0;
            }

            if (expression.Kind == OperationKind.FieldReferenceExpression)
            {
                var field = ((IFieldReferenceExpression)expression).Field;
                return string.Equals(field.Name, StringEmptyFieldName) &&
                    field.Type.SpecialType == SpecialType.System_String;
            }

            return false;
        }

        private static bool HasAnEmptyStringArgument(IInvocationExpression invocation)
        {
            foreach (var argument in invocation.ArgumentsInSourceOrder)
            {
                if (IsEmptyString(argument.Value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
