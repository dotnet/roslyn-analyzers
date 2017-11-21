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
    public abstract class UseOrdinalStringComparisonAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1309";

        private static readonly LocalizableString s_localizableMessageAndTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.UseOrdinalStringComparisonTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.UseOrdinalStringComparisonDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableMessageAndTitle,
                                                                             s_localizableMessageAndTitle,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "http://msdn.microsoft.com/library/bb385972.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal const string CompareMethodName = "Compare";
        internal const string EqualsMethodName = "Equals";
        internal const string OrdinalText = "Ordinal";
        internal const string OrdinalIgnoreCaseText = "OrdinalIgnoreCase";
        internal const string StringComparisonTypeName = "System.StringComparison";
        internal const string IgnoreCaseText = "IgnoreCase";

        protected abstract Location GetMethodNameLocation(SyntaxNode invocationNode);
        protected abstract Location GetOperatorTokenLocation(SyntaxNode binaryOperationNode);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    INamedTypeSymbol stringComparisonType = context.Compilation.GetTypeByMetadataName(StringComparisonTypeName);
                    if (stringComparisonType != null)
                    {
                        context.RegisterOperationAction(operationContext => AnalyzeOperation(operationContext, stringComparisonType),
                                                        OperationKind.Invocation,
                                                        OperationKind.BinaryOperator);
                    }
                });
        }

        private void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol stringComparisonType)
        {
            OperationKind kind = context.Operation.Kind;
            if (kind == OperationKind.Invocation)
            {
                AnalyzeInvocationExpression((IInvocationOperation)context.Operation, stringComparisonType, context.ReportDiagnostic, GetMethodNameLocation);
            }
            else
            {
                AnalyzeBinaryExpression((IBinaryOperation)context.Operation, context.ReportDiagnostic, GetOperatorTokenLocation);
            }
        }

        private static void AnalyzeInvocationExpression(IInvocationOperation operation, INamedTypeSymbol stringComparisonType, Action<Diagnostic> reportDiagnostic, Func<SyntaxNode, Location> getMethodNameLocation)
        {
            IMethodSymbol methodSymbol = operation.TargetMethod;
            if (methodSymbol != null &&
                methodSymbol.ContainingType.SpecialType == SpecialType.System_String &&
                IsEqualsOrCompare(methodSymbol.Name))
            {
                if (!IsAcceptableOverload(methodSymbol, stringComparisonType))
                {
                    // wrong overload
                    reportDiagnostic(Diagnostic.Create(Rule, getMethodNameLocation(operation.Syntax)));
                }
                else
                {
                    IArgumentOperation lastArgument = operation.Arguments.Last();
                    if (lastArgument.Value.Kind == OperationKind.FieldReference)
                    {
                        IFieldSymbol fieldSymbol = ((IFieldReferenceOperation)lastArgument.Value).Field;
                        if (fieldSymbol != null &&
                            fieldSymbol.ContainingType.Equals(stringComparisonType) &&
                            !IsOrdinalOrOrdinalIgnoreCase(fieldSymbol.Name))
                        {
                            // right overload, wrong value
                            reportDiagnostic(lastArgument.Syntax.CreateDiagnostic(Rule));
                        }
                    }
                }
            }
        }

        private static void AnalyzeBinaryExpression(IBinaryOperation operation, Action<Diagnostic> reportDiagnostic, Func<SyntaxNode, Location> getOperatorTokenLocation)
        {
            if (operation.OperatorKind == BinaryOperatorKind.Equals || operation.OperatorKind == BinaryOperatorKind.NotEquals)
            {
                // If either of the operands is not of string type, we shouldn't report a diagnostic.
                if (operation.LeftOperand.Type?.SpecialType != SpecialType.System_String ||
                    operation.RightOperand.Type?.SpecialType != SpecialType.System_String)
                {
                    return;
                }

                // If either of the operands is null, we shouldn't report a diagnostic.
                if (operation.LeftOperand.HasNullConstantValue() || operation.RightOperand.HasNullConstantValue())
                {
                    return;
                }

                reportDiagnostic(Diagnostic.Create(Rule, getOperatorTokenLocation(operation.Syntax)));
            }
        }

        private static bool IsEqualsOrCompare(string methodName)
        {
            return string.Equals(methodName, EqualsMethodName, StringComparison.Ordinal) ||
                string.Equals(methodName, CompareMethodName, StringComparison.Ordinal);
        }

        private static bool IsAcceptableOverload(IMethodSymbol methodSymbol, INamedTypeSymbol stringComparisonType)
        {
            return methodSymbol.IsStatic
                ? IsAcceptableStaticOverload(methodSymbol, stringComparisonType)
                : IsAcceptableInstanceOverload(methodSymbol, stringComparisonType);
        }

        private static bool IsAcceptableInstanceOverload(IMethodSymbol methodSymbol, INamedTypeSymbol stringComparisonType)
        {
            if (string.Equals(methodSymbol.Name, EqualsMethodName, StringComparison.Ordinal))
            {
                switch (methodSymbol.Parameters.Length)
                {
                    case 1:
                        // the instance method .Equals(object) is acceptable
                        return methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_Object;
                    case 2:
                        // .Equals(string, System.StringComparison) is acceptable
                        return methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[1].Type.Equals(stringComparisonType);
                }
            }

            // all other overloads are unacceptable
            return false;
        }

        private static bool IsAcceptableStaticOverload(IMethodSymbol methodSymbol, INamedTypeSymbol stringComparisonType)
        {
            if (string.Equals(methodSymbol.Name, CompareMethodName, StringComparison.Ordinal))
            {
                switch (methodSymbol.Parameters.Length)
                {
                    case 3:
                        // (string, string, StringComparison) is acceptable
                        return methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[1].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[2].Type.Equals(stringComparisonType);
                    case 6:
                        // (string, int, string, int, int, StringComparison) is acceptable
                        return methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[1].Type.SpecialType == SpecialType.System_Int32 &&
                            methodSymbol.Parameters[2].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[3].Type.SpecialType == SpecialType.System_Int32 &&
                            methodSymbol.Parameters[4].Type.SpecialType == SpecialType.System_Int32 &&
                            methodSymbol.Parameters[5].Type.Equals(stringComparisonType);
                }
            }
            else if (string.Equals(methodSymbol.Name, EqualsMethodName, StringComparison.Ordinal))
            {
                switch (methodSymbol.Parameters.Length)
                {
                    case 2:
                        // (object, object) is acceptable
                        return methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                            methodSymbol.Parameters[1].Type.SpecialType == SpecialType.System_Object;
                    case 3:
                        // (string, string, StringComparison) is acceptable
                        return methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[1].Type.SpecialType == SpecialType.System_String &&
                            methodSymbol.Parameters[2].Type.Equals(stringComparisonType);
                }
            }

            // all other overloads are unacceptable
            return false;
        }

        private static bool IsOrdinalOrOrdinalIgnoreCase(string name)
        {
            return string.Compare(name, OrdinalText, StringComparison.Ordinal) == 0 ||
                string.Compare(name, OrdinalIgnoreCaseText, StringComparison.Ordinal) == 0;
        }
    }
}
