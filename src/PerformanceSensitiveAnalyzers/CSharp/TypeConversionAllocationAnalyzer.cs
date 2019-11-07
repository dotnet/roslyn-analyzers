// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PerformanceSensitiveAnalyzers;

namespace Microsoft.CodeAnalysis.CSharp.PerformanceSensitiveAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal sealed class TypeConversionAllocationAnalyzer : AbstractAllocationAnalyzer
    {
        public const string ValueTypeToReferenceTypeConversionRuleId = "HAA0601";
        public const string DelegateOnStructInstanceRuleId = "HAA0602";
        public const string MethodGroupAllocationRuleId = "HAA0603";
        // HeapAnalyzerReadonlyMethodGroupAllocationRule [sic] is retired and should not be reused
        public const string LambdaReturnConversionRuleId = "HAA0604";

        private static readonly LocalizableString s_localizableValueTypeToReferenceTypeConversionRuleTitle = new LocalizableResourceString(nameof(AnalyzersResources.ValueTypeToReferenceTypeConversionRuleTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString s_localizableValueTypeToReferenceTypeConversionRuleMessage = new LocalizableResourceString(nameof(AnalyzersResources.ValueTypeToReferenceTypeConversionRuleMessage), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));

        private static readonly LocalizableString s_localizableDelegateOnStructInstanceRuleTitle = new LocalizableResourceString(nameof(AnalyzersResources.DelegateOnStructInstanceRuleTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString s_localizableDelegateOnStructInstanceRuleMessage = new LocalizableResourceString(nameof(AnalyzersResources.DelegateOnStructInstanceRuleMessage), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));

        private static readonly LocalizableString s_localizableMethodGroupAllocationRuleTitle = new LocalizableResourceString(nameof(AnalyzersResources.MethodGroupAllocationRuleTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString s_localizableMethodGroupAllocationRuleMessage = new LocalizableResourceString(nameof(AnalyzersResources.MethodGroupAllocationRuleMessage), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));

        private static readonly LocalizableString s_localizableLambdaReturnConversionRuleTitle = new LocalizableResourceString(nameof(AnalyzersResources.LambdaReturnConversionRuleTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString s_localizableLambdaReturnConversionRuleMessage = new LocalizableResourceString(nameof(AnalyzersResources.LambdaReturnConversionRuleMessage), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));

        internal static DiagnosticDescriptor ValueTypeToReferenceTypeConversionRule = new DiagnosticDescriptor(
            ValueTypeToReferenceTypeConversionRuleId,
            s_localizableValueTypeToReferenceTypeConversionRuleTitle,
            s_localizableValueTypeToReferenceTypeConversionRuleMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        internal static DiagnosticDescriptor DelegateOnStructInstanceRule = new DiagnosticDescriptor(
            DelegateOnStructInstanceRuleId,
            s_localizableDelegateOnStructInstanceRuleTitle,
            s_localizableDelegateOnStructInstanceRuleMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        internal static DiagnosticDescriptor MethodGroupAllocationRule = new DiagnosticDescriptor(
            MethodGroupAllocationRuleId,
            s_localizableMethodGroupAllocationRuleTitle,
            s_localizableMethodGroupAllocationRuleMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        internal static DiagnosticDescriptor LambdaReturnConversionRule = new DiagnosticDescriptor(
            LambdaReturnConversionRuleId,
            s_localizableLambdaReturnConversionRuleTitle,
            s_localizableLambdaReturnConversionRuleMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly object[] EmptyMessageArgs = Array.Empty<object>();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            ValueTypeToReferenceTypeConversionRule,
            MethodGroupAllocationRule,
            DelegateOnStructInstanceRule,
            LambdaReturnConversionRule);

        protected override ImmutableArray<OperationKind> Operations => ImmutableArray.Create(
            OperationKind.Conversion,
            OperationKind.Interpolation,
            OperationKind.DelegateCreation);

        protected override void AnalyzeNode(OperationAnalysisContext context, in PerformanceSensitiveInfo info)
        {
            if (context.Operation is IDelegateCreationOperation delegateCreation)
            {
                if (delegateCreation.IsImplicit && delegateCreation.Target.Kind != OperationKind.AnonymousFunction)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MethodGroupAllocationRule, context.Operation.Syntax.GetLocation(), EmptyMessageArgs));
                }

                if (delegateCreation.Target is IMethodReferenceOperation methodReference &&
                    methodReference.Instance?.Type.IsValueType == true)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DelegateOnStructInstanceRule, methodReference.Syntax.GetLocation(), EmptyMessageArgs));
                }

                return;
            }

            if (context.Operation is IInterpolationOperation interpolation)
            {
                if (interpolation.Expression.Type.IsValueType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, interpolation.Expression.Syntax.GetLocation(), EmptyMessageArgs));
                }

                return;
            }

            if (context.Operation.Type.IsReferenceType && context.Operation is IConversionOperation conversion)
            {
                if (conversion.Operand.Type?.IsValueType == true && conversion.OperatorMethod == null)
                {
                    // No boxing occurs for the special case of string concatenation.
                    if (conversion.Parent.Type?.SpecialType == SpecialType.System_String)
                    {
                        if (conversion.Parent is IBinaryOperation)
                        {
                            return;
                        }
                        if (conversion.Parent is ICompoundAssignmentOperation)
                        {
                            return;
                        }
                    }

                    var parent = conversion.Parent;
                    while (parent != null)
                    {
                        if (parent is IAnonymousFunctionOperation)
                        {
                            string[] messageArgs = { conversion.Operand.Type.ToDisplayString(), conversion.Type.ToDisplayString() };
                            context.ReportDiagnostic(Diagnostic.Create(LambdaReturnConversionRule, conversion.Operand.Syntax.GetLocation(), messageArgs));
                            return;
                        }

                        parent = parent.Parent;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, conversion.Operand.Syntax.GetLocation(), EmptyMessageArgs));
                }
                else if (conversion.Operand.Type?.TypeKind == TypeKind.TypeParameter && !conversion.Operand.Type.IsReferenceType && conversion.OperatorMethod == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ValueTypeToReferenceTypeConversionRule, conversion.Operand.Syntax.GetLocation(), EmptyMessageArgs));
                }

                return;
            }
        }
    }
}