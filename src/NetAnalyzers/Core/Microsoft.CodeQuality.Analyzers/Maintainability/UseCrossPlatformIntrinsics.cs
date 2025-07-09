// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Maintainability
{
    using static MicrosoftCodeQualityAnalyzersResources;

    /// <summary>
    /// CA1516: <inheritdoc cref="UseCrossPlatformIntrinsicsTitle"/>
    /// </summary>
    public abstract class UseCrossPlatformIntrinsicsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1516";

        private static readonly LocalizableString s_localizableTitle = CreateLocalizableResourceString(nameof(UseCrossPlatformIntrinsicsTitle));
        private static readonly LocalizableString s_localizableDescription = CreateLocalizableResourceString(nameof(UseCrossPlatformIntrinsicsDescription));

        internal static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.CreateRange(
            Enumerable.Range(0, (int)RuleKind.Count)
                      .Select(i => CreateDiagnosticDescriptor((RuleKind)i))
        );

        private ImmutableArray<HashSet<IMethodSymbol>> _methodSymbols;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;

            _methodSymbols = ImmutableArray.CreateRange(
                Enumerable.Range(0, (int)RuleKind.Count)
                          .Select(_ => new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default))
            );

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsArmAdvSimd, out var armAdvSimdTypeSymbol))
            {
                AddBinaryOperatorMethods("Add", armAdvSimdTypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("AddScalar", armAdvSimdTypeSymbol, RuleKind.opAddition, [SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Double]);
                AddBinaryOperatorMethods("And", armAdvSimdTypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("DivideScalar", armAdvSimdTypeSymbol, RuleKind.opDivision, [SpecialType.System_Double]);
                AddBinaryOperatorMethods("Multiply", armAdvSimdTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("MultiplyScalar", armAdvSimdTypeSymbol, RuleKind.opMultiply, [SpecialType.System_Double]);
                AddBinaryOperatorMethods("Or", armAdvSimdTypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", armAdvSimdTypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("SubtractScalar", armAdvSimdTypeSymbol, RuleKind.opSubtraction, [SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Double]);
                AddBinaryOperatorMethods("Xor", armAdvSimdTypeSymbol, RuleKind.opExclusiveOr);

                AddShiftOperatorMethods("ShiftLeftLogical", armAdvSimdTypeSymbol, RuleKind.opLeftShift);
                AddShiftOperatorMethods("ShiftLeftLogicalScalar", armAdvSimdTypeSymbol, RuleKind.opLeftShift, [SpecialType.System_Int64, SpecialType.System_UInt64]);
                AddShiftOperatorMethods("ShiftRightArithmetic", armAdvSimdTypeSymbol, RuleKind.opRightShift);
                AddShiftOperatorMethods("ShiftRightArithmeticScalar", armAdvSimdTypeSymbol, RuleKind.opRightShift, [SpecialType.System_Int64, SpecialType.System_UInt64]);
                AddShiftOperatorMethods("ShiftRightLogical", armAdvSimdTypeSymbol, RuleKind.opUnsignedRightShift);
                AddShiftOperatorMethods("ShiftRightLogicalScalar", armAdvSimdTypeSymbol, RuleKind.opUnsignedRightShift, [SpecialType.System_Int64, SpecialType.System_UInt64]);

                AddUnaryOperatorMethods("Negate", armAdvSimdTypeSymbol, RuleKind.opUnaryNegation);
                AddUnaryOperatorMethods("NegateScalar", armAdvSimdTypeSymbol, RuleKind.opUnaryNegation, [SpecialType.System_Double]);
                AddUnaryOperatorMethods("Not", armAdvSimdTypeSymbol, RuleKind.opOnesComplement);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsArmAdvSimdArm64, out var armAdvSimdArm64TypeSymbol))
            {
                AddBinaryOperatorMethods("Add", armAdvSimdArm64TypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("Divide", armAdvSimdArm64TypeSymbol, RuleKind.opDivision);
                AddBinaryOperatorMethods("Multiply", armAdvSimdArm64TypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Subtract", armAdvSimdArm64TypeSymbol, RuleKind.opSubtraction);

                AddUnaryOperatorMethods("Negate", armAdvSimdArm64TypeSymbol, RuleKind.opUnaryNegation);
                AddUnaryOperatorMethods("NegateScalar", armAdvSimdArm64TypeSymbol, RuleKind.opUnaryNegation, [SpecialType.System_Int64]);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsWasmPackedSimd, out var wasmPackedSimdTypeSymbol))
            {
                AddBinaryOperatorMethods("Add", wasmPackedSimdTypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("And", wasmPackedSimdTypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("Divide", wasmPackedSimdTypeSymbol, RuleKind.opDivision);
                AddBinaryOperatorMethods("Multiply", wasmPackedSimdTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", wasmPackedSimdTypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", wasmPackedSimdTypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("Xor", wasmPackedSimdTypeSymbol, RuleKind.opExclusiveOr);

                AddShiftOperatorMethods("ShiftLeft", wasmPackedSimdTypeSymbol, RuleKind.opLeftShift);
                AddShiftOperatorMethods("ShiftRightArithmetic", wasmPackedSimdTypeSymbol, RuleKind.opRightShift);
                AddShiftOperatorMethods("ShiftRightLogical", wasmPackedSimdTypeSymbol, RuleKind.opUnsignedRightShift);

                AddUnaryOperatorMethods("Negate", wasmPackedSimdTypeSymbol, RuleKind.opUnaryNegation);
                AddUnaryOperatorMethods("Not", wasmPackedSimdTypeSymbol, RuleKind.opOnesComplement);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx, out var x86AvxTypeSymbol))
            {
                AddBinaryOperatorMethods("Add", x86AvxTypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("And", x86AvxTypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("Divide", x86AvxTypeSymbol, RuleKind.opDivision);
                AddBinaryOperatorMethods("Multiply", x86AvxTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", x86AvxTypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", x86AvxTypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("Xor", x86AvxTypeSymbol, RuleKind.opExclusiveOr);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx2, out var x86Avx2TypeSymbol))
            {
                AddBinaryOperatorMethods("Add", x86Avx2TypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("And", x86Avx2TypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("MultiplyLow", x86Avx2TypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", x86Avx2TypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", x86Avx2TypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("Xor", x86Avx2TypeSymbol, RuleKind.opExclusiveOr);

                AddShiftOperatorMethods("ShiftLeftLogical", x86Avx2TypeSymbol, RuleKind.opLeftShift);
                AddShiftOperatorMethods("ShiftRightArithmetic", x86Avx2TypeSymbol, RuleKind.opRightShift);
                AddShiftOperatorMethods("ShiftRightLogical", x86Avx2TypeSymbol, RuleKind.opUnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512BW, out var x86Avx512BWTypeSymbol))
            {
                AddBinaryOperatorMethods("Add", x86Avx512BWTypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("MultiplyLow", x86Avx512BWTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Subtract", x86Avx512BWTypeSymbol, RuleKind.opSubtraction);

                AddShiftOperatorMethods("ShiftLeftLogical", x86Avx512BWTypeSymbol, RuleKind.opLeftShift);
                AddShiftOperatorMethods("ShiftRightArithmetic", x86Avx512BWTypeSymbol, RuleKind.opRightShift);
                AddShiftOperatorMethods("ShiftRightLogical", x86Avx512BWTypeSymbol, RuleKind.opUnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512DQ, out var x86Avx512DQTypeSymbol))
            {
                AddBinaryOperatorMethods("And", x86Avx512DQTypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("MultiplyLow", x86Avx512DQTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", x86Avx512DQTypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Xor", x86Avx512DQTypeSymbol, RuleKind.opExclusiveOr);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512DQVL, out var x86Avx512DQVLTypeSymbol))
            {
                AddBinaryOperatorMethods("MultiplyLow", x86Avx512DQVLTypeSymbol, RuleKind.opMultiply);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512F, out var x86Avx512FTypeSymbol))
            {
                AddBinaryOperatorMethods("Add", x86Avx512FTypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("And", x86Avx512FTypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("Divide", x86Avx512FTypeSymbol, RuleKind.opDivision);
                AddBinaryOperatorMethods("Multiply", x86Avx512FTypeSymbol, RuleKind.opMultiply, [SpecialType.System_Single, SpecialType.System_Double]);
                AddBinaryOperatorMethods("MultiplyLow", x86Avx512FTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", x86Avx512FTypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", x86Avx512FTypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("Xor", x86Avx512FTypeSymbol, RuleKind.opExclusiveOr);

                AddShiftOperatorMethods("ShiftLeftLogical", x86Avx512FTypeSymbol, RuleKind.opLeftShift);
                AddShiftOperatorMethods("ShiftRightArithmetic", x86Avx512FTypeSymbol, RuleKind.opRightShift);
                AddShiftOperatorMethods("ShiftRightLogical", x86Avx512FTypeSymbol, RuleKind.opUnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512FVL, out var x86Avx512FVLTypeSymbol))
            {
                AddShiftOperatorMethods("ShiftRightArithmetic", x86Avx512FVLTypeSymbol, RuleKind.opRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Sse, out var x86SseTypeSymbol))
            {
                AddBinaryOperatorMethods("Add", x86SseTypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("And", x86SseTypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("Divide", x86SseTypeSymbol, RuleKind.opDivision);
                AddBinaryOperatorMethods("Multiply", x86SseTypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", x86SseTypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", x86SseTypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("Xor", x86SseTypeSymbol, RuleKind.opExclusiveOr);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Sse2, out var x86Sse2TypeSymbol))
            {
                AddBinaryOperatorMethods("Add", x86Sse2TypeSymbol, RuleKind.opAddition);
                AddBinaryOperatorMethods("And", x86Sse2TypeSymbol, RuleKind.opBitwiseAnd);
                AddBinaryOperatorMethods("Divide", x86Sse2TypeSymbol, RuleKind.opDivision);
                AddBinaryOperatorMethods("Multiply", x86Sse2TypeSymbol, RuleKind.opMultiply, [SpecialType.System_Double]);
                AddBinaryOperatorMethods("MultiplyLow", x86Sse2TypeSymbol, RuleKind.opMultiply);
                AddBinaryOperatorMethods("Or", x86Sse2TypeSymbol, RuleKind.opBitwiseOr);
                AddBinaryOperatorMethods("Subtract", x86Sse2TypeSymbol, RuleKind.opSubtraction);
                AddBinaryOperatorMethods("Xor", x86Sse2TypeSymbol, RuleKind.opExclusiveOr);

                AddShiftOperatorMethods("ShiftLeftLogical", x86Sse2TypeSymbol, RuleKind.opLeftShift);
                AddShiftOperatorMethods("ShiftRightArithmetic", x86Sse2TypeSymbol, RuleKind.opRightShift);
                AddShiftOperatorMethods("ShiftRightLogical", x86Sse2TypeSymbol, RuleKind.opUnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Sse41, out var x86Sse41TypeSymbol))
            {
                AddBinaryOperatorMethods("MultiplyLow", x86Sse41TypeSymbol, RuleKind.opMultiply);
            }

            if (_methodSymbols.Any((methodSymbols) => methodSymbols.Any()))
            {
                context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            }
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            for (int i = 0; i < _methodSymbols.Length; i++)
            {
                HashSet<IMethodSymbol> methodSymbols = _methodSymbols[i];

                if (methodSymbols.Contains(invocation.TargetMethod, SymbolEqualityComparer.Default))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(Rules[i]));
                    return;
                }
            }
        }

        private void AddBinaryOperatorMethods(string name, INamedTypeSymbol typeSymbol, RuleKind ruleKind, params SpecialType[] supportedTypes)
        {
            HashSet<IMethodSymbol> methodSymbols = _methodSymbols[(int)ruleKind];

            IEnumerable<IMethodSymbol> members =
                typeSymbol.GetMembers(name)
                          .OfType<IMethodSymbol>()
                          .Where((m) => m.Parameters.Length == 2 &&
                                        m.ReturnType is INamedTypeSymbol namedReturnTypeSymbol &&
                                        namedReturnTypeSymbol.Arity == 1 &&
                                        ((supportedTypes.Length == 0) || supportedTypes.Contains(namedReturnTypeSymbol.TypeArguments[0].SpecialType)) &&
                                        SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, m.Parameters[1].Type) &&
                                        SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, namedReturnTypeSymbol));

            methodSymbols.AddRange(members);
        }

        private void AddShiftOperatorMethods(string name, INamedTypeSymbol typeSymbol, RuleKind ruleKind, params SpecialType[] supportedTypes)
        {
            HashSet<IMethodSymbol> methodSymbols = _methodSymbols[(int)ruleKind];

            IEnumerable<IMethodSymbol> members =
                typeSymbol.GetMembers(name)
                          .OfType<IMethodSymbol>()
                          .Where((m) => m.Parameters.Length == 2 &&
                                        m.ReturnType is INamedTypeSymbol namedReturnTypeSymbol &&
                                        namedReturnTypeSymbol.Arity == 1 &&
                                        ((supportedTypes.Length == 0) || supportedTypes.Contains(namedReturnTypeSymbol.TypeArguments[0].SpecialType)) &&
                                        (m.Parameters[1].Type.SpecialType is SpecialType.System_Byte or SpecialType.System_Int32) &&
                                        SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, namedReturnTypeSymbol));

            methodSymbols.AddRange(members);
        }

        private void AddUnaryOperatorMethods(string name, INamedTypeSymbol typeSymbol, RuleKind ruleKind, params SpecialType[] supportedTypes)
        {
            HashSet<IMethodSymbol> methodSymbols = _methodSymbols[(int)ruleKind];

            IEnumerable<IMethodSymbol> members =
                typeSymbol.GetMembers(name)
                          .OfType<IMethodSymbol>()
                          .Where((m) => m.Parameters.Length == 1 &&
                                        m.ReturnType is INamedTypeSymbol namedReturnTypeSymbol &&
                                        namedReturnTypeSymbol.Arity == 1 &&
                                        ((supportedTypes.Length == 0) || supportedTypes.Contains(namedReturnTypeSymbol.TypeArguments[0].SpecialType)) &&
                                        SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, namedReturnTypeSymbol));

            methodSymbols.AddRange(members);
        }

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(RuleKind ruleKind) => DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            CreateLocalizableResourceString($"UseCrossPlatformIntrinsicsMessage_{ruleKind}"),
            DiagnosticCategory.Maintainability,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false,
            additionalCustomTags: ruleKind.ToString()
        );

        internal enum RuleKind
        {
            opAddition,
            opBitwiseAnd,
            opBitwiseOr,
            opDivision,
            opExclusiveOr,
            opLeftShift,
            opMultiply,
            opOnesComplement,
            opRightShift,
            opSubtraction,
            opUnaryNegation,
            opUnsignedRightShift,
            Count,
        }
    }
}
