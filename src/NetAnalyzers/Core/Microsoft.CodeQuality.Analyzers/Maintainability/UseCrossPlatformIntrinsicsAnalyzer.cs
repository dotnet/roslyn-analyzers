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

        internal static readonly ImmutableArray<ImmutableDictionary<string, string?>> Properties = ImmutableArray.CreateRange(
            Enumerable.Range(0, (int)RuleKind.Count)
                      .Select(i =>
                      {
                          ImmutableDictionary<string, string?>.Builder builder = ImmutableDictionary.CreateBuilder<string, string?>();
                          builder[nameof(RuleKind)] = ((RuleKind)i).ToString();
                          return builder.ToImmutable();
                      })
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        protected abstract bool IsSupported(IInvocationOperation invocation, RuleKind ruleKind);

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;

            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsVector64, out var _) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsVector128, out var _) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsVector256, out var _) ||
                !compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsVector512, out var _))
            {
                // The core vector types are not available in the compilation, so we cannot register any operators.
                // This may exclude out of support versions of .NET, such as .NET 6, which only have some of the vector types
                //
                // Notably, this is still not an exact check. There may be custom runtimes or edge case scenarios where a given
                // operator is not available on a given type but the platform specific API is available. In such a case, we will
                // report a diagnostic and the fixer will be reported. If the user applies the fixer, the code would produce an
                // error. This is considered an acceptable tradeoff given there would need to be hundreds of checks to exactly
                // cover the potential scenarios, which would make the analyzer too complex and slow. There will be no diagnostic
                // or fixer reported for in support versions of .NET, such as .NET Standard and .NET Framework; and the diagnostic
                // and fixer reported for .NET 8+ will be correct.
                return;
            }

            // We need to find the platform specific intrinsics that we support replacing with the cross-platform intrinsics. To do
            // this, we need to find the methods under each class by name and signature. In most cases, the methods support "all"
            // types, but in some cases they do not and so we will pass the exact types that we support.

            ImmutableArray<HashSet<IMethodSymbol>> methodSymbolSets = ImmutableArray.CreateRange(
                Enumerable.Range(0, (int)RuleKind.Count)
                          .Select(_ => new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default))
            );

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsArmAdvSimd, out var armAdvSimdTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", armAdvSimdTypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "AddScalar", armAdvSimdTypeSymbol, RuleKind.op_Addition, [SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Double]);
                AddBinaryOperatorMethods(methodSymbolSets, "And", armAdvSimdTypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "DivideScalar", armAdvSimdTypeSymbol, RuleKind.op_Division, [SpecialType.System_Double]);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", armAdvSimdTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyScalar", armAdvSimdTypeSymbol, RuleKind.op_Multiply, [SpecialType.System_Double]);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", armAdvSimdTypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", armAdvSimdTypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "SubtractScalar", armAdvSimdTypeSymbol, RuleKind.op_Subtraction, [SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Double]);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", armAdvSimdTypeSymbol, RuleKind.op_ExclusiveOr);

                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeftLogical", armAdvSimdTypeSymbol, RuleKind.op_LeftShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeftLogicalScalar", armAdvSimdTypeSymbol, RuleKind.op_LeftShift, [SpecialType.System_Int64, SpecialType.System_UInt64]);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", armAdvSimdTypeSymbol, RuleKind.op_RightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmeticScalar", armAdvSimdTypeSymbol, RuleKind.op_RightShift, [SpecialType.System_Int64, SpecialType.System_UInt64]);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogical", armAdvSimdTypeSymbol, RuleKind.op_UnsignedRightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogicalScalar", armAdvSimdTypeSymbol, RuleKind.op_UnsignedRightShift, [SpecialType.System_Int64, SpecialType.System_UInt64]);

                AddUnaryOperatorMethods(methodSymbolSets, "Negate", armAdvSimdTypeSymbol, RuleKind.op_UnaryNegation);
                AddUnaryOperatorMethods(methodSymbolSets, "NegateScalar", armAdvSimdTypeSymbol, RuleKind.op_UnaryNegation, [SpecialType.System_Double]);
                AddUnaryOperatorMethods(methodSymbolSets, "Not", armAdvSimdTypeSymbol, RuleKind.op_OnesComplement);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsArmAdvSimdArm64, out var armAdvSimdArm64TypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", armAdvSimdArm64TypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "Divide", armAdvSimdArm64TypeSymbol, RuleKind.op_Division);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", armAdvSimdArm64TypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", armAdvSimdArm64TypeSymbol, RuleKind.op_Subtraction);

                AddUnaryOperatorMethods(methodSymbolSets, "Negate", armAdvSimdArm64TypeSymbol, RuleKind.op_UnaryNegation);
                AddUnaryOperatorMethods(methodSymbolSets, "NegateScalar", armAdvSimdArm64TypeSymbol, RuleKind.op_UnaryNegation, [SpecialType.System_Int64]);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsWasmPackedSimd, out var wasmPackedSimdTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", wasmPackedSimdTypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "And", wasmPackedSimdTypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "Divide", wasmPackedSimdTypeSymbol, RuleKind.op_Division);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", wasmPackedSimdTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", wasmPackedSimdTypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", wasmPackedSimdTypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", wasmPackedSimdTypeSymbol, RuleKind.op_ExclusiveOr);

                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeft", wasmPackedSimdTypeSymbol, RuleKind.op_LeftShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", wasmPackedSimdTypeSymbol, RuleKind.op_RightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogical", wasmPackedSimdTypeSymbol, RuleKind.op_UnsignedRightShift);

                AddUnaryOperatorMethods(methodSymbolSets, "Negate", wasmPackedSimdTypeSymbol, RuleKind.op_UnaryNegation);
                AddUnaryOperatorMethods(methodSymbolSets, "Not", wasmPackedSimdTypeSymbol, RuleKind.op_OnesComplement);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx, out var x86AvxTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", x86AvxTypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "And", x86AvxTypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "Divide", x86AvxTypeSymbol, RuleKind.op_Division);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", x86AvxTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", x86AvxTypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", x86AvxTypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", x86AvxTypeSymbol, RuleKind.op_ExclusiveOr);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx2, out var x86Avx2TypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", x86Avx2TypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "And", x86Avx2TypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Avx2TypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", x86Avx2TypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", x86Avx2TypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", x86Avx2TypeSymbol, RuleKind.op_ExclusiveOr);

                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeftLogical", x86Avx2TypeSymbol, RuleKind.op_LeftShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", x86Avx2TypeSymbol, RuleKind.op_RightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogical", x86Avx2TypeSymbol, RuleKind.op_UnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512BW, out var x86Avx512BWTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", x86Avx512BWTypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Avx512BWTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", x86Avx512BWTypeSymbol, RuleKind.op_Subtraction);

                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeftLogical", x86Avx512BWTypeSymbol, RuleKind.op_LeftShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", x86Avx512BWTypeSymbol, RuleKind.op_RightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogical", x86Avx512BWTypeSymbol, RuleKind.op_UnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512DQ, out var x86Avx512DQTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "And", x86Avx512DQTypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Avx512DQTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", x86Avx512DQTypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", x86Avx512DQTypeSymbol, RuleKind.op_ExclusiveOr);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512DQVL, out var x86Avx512DQVLTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Avx512DQVLTypeSymbol, RuleKind.op_Multiply);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512F, out var x86Avx512FTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", x86Avx512FTypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "And", x86Avx512FTypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "Divide", x86Avx512FTypeSymbol, RuleKind.op_Division);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", x86Avx512FTypeSymbol, RuleKind.op_Multiply, [SpecialType.System_Single, SpecialType.System_Double]);
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Avx512FTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", x86Avx512FTypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", x86Avx512FTypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", x86Avx512FTypeSymbol, RuleKind.op_ExclusiveOr);

                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeftLogical", x86Avx512FTypeSymbol, RuleKind.op_LeftShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", x86Avx512FTypeSymbol, RuleKind.op_RightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogical", x86Avx512FTypeSymbol, RuleKind.op_UnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Avx512FVL, out var x86Avx512FVLTypeSymbol))
            {
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", x86Avx512FVLTypeSymbol, RuleKind.op_RightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Sse, out var x86SseTypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", x86SseTypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "And", x86SseTypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "Divide", x86SseTypeSymbol, RuleKind.op_Division);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", x86SseTypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", x86SseTypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", x86SseTypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", x86SseTypeSymbol, RuleKind.op_ExclusiveOr);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Sse2, out var x86Sse2TypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "Add", x86Sse2TypeSymbol, RuleKind.op_Addition);
                AddBinaryOperatorMethods(methodSymbolSets, "And", x86Sse2TypeSymbol, RuleKind.op_BitwiseAnd);
                AddBinaryOperatorMethods(methodSymbolSets, "Divide", x86Sse2TypeSymbol, RuleKind.op_Division);
                AddBinaryOperatorMethods(methodSymbolSets, "Multiply", x86Sse2TypeSymbol, RuleKind.op_Multiply, [SpecialType.System_Double]);
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Sse2TypeSymbol, RuleKind.op_Multiply);
                AddBinaryOperatorMethods(methodSymbolSets, "Or", x86Sse2TypeSymbol, RuleKind.op_BitwiseOr);
                AddBinaryOperatorMethods(methodSymbolSets, "Subtract", x86Sse2TypeSymbol, RuleKind.op_Subtraction);
                AddBinaryOperatorMethods(methodSymbolSets, "Xor", x86Sse2TypeSymbol, RuleKind.op_ExclusiveOr);

                AddShiftOperatorMethods(methodSymbolSets, "ShiftLeftLogical", x86Sse2TypeSymbol, RuleKind.op_LeftShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightArithmetic", x86Sse2TypeSymbol, RuleKind.op_RightShift);
                AddShiftOperatorMethods(methodSymbolSets, "ShiftRightLogical", x86Sse2TypeSymbol, RuleKind.op_UnsignedRightShift);
            }

            if (compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeIntrinsicsX86Sse41, out var x86Sse41TypeSymbol))
            {
                AddBinaryOperatorMethods(methodSymbolSets, "MultiplyLow", x86Sse41TypeSymbol, RuleKind.op_Multiply);
            }

            if (methodSymbolSets.Any((methodSymbols) => methodSymbols.Any()))
            {
                context.RegisterOperationAction((context) => AnalyzeInvocation(context, methodSymbolSets), OperationKind.Invocation);
            }

            static void AddBinaryOperatorMethods(ImmutableArray<HashSet<IMethodSymbol>> methodSymbolSets, string name, INamedTypeSymbol typeSymbol, RuleKind ruleKind, params SpecialType[] supportedTypes)
            {
                HashSet<IMethodSymbol> methodSymbols = methodSymbolSets[(int)ruleKind];

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

            static void AddShiftOperatorMethods(ImmutableArray<HashSet<IMethodSymbol>> methodSymbolSets, string name, INamedTypeSymbol typeSymbol, RuleKind ruleKind, params SpecialType[] supportedTypes)
            {
                HashSet<IMethodSymbol> methodSymbols = methodSymbolSets[(int)ruleKind];

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

            static void AddUnaryOperatorMethods(ImmutableArray<HashSet<IMethodSymbol>> methodSymbolSets, string name, INamedTypeSymbol typeSymbol, RuleKind ruleKind, params SpecialType[] supportedTypes)
            {
                HashSet<IMethodSymbol> methodSymbols = methodSymbolSets[(int)ruleKind];

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
        }

        private void AnalyzeInvocation(OperationAnalysisContext context, ImmutableArray<HashSet<IMethodSymbol>> methodSymbolSets)
        {
            if (context.Operation is not IInvocationOperation invocation)
            {
                return;
            }

            IMethodSymbol targetMethod = invocation.TargetMethod;

            for (int i = 0; i < methodSymbolSets.Length; i++)
            {
                HashSet<IMethodSymbol> methodSymbols = methodSymbolSets[i];

                if (!methodSymbols.Contains(targetMethod, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                if (IsSupported(invocation, (RuleKind)i))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(Rules[i], Properties[i]));
                    break;
                }
            }
        }

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(RuleKind ruleKind) => DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            CreateLocalizableResourceString($"UseCrossPlatformIntrinsicsMessage_{ruleKind}"),
            DiagnosticCategory.Maintainability,
            RuleLevel.IdeSuggestion,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false
        );

        public enum RuleKind
        {
            // These names match the underlying IL names for the cross-platform API that will be used in the fixer.

            op_Addition,
            op_BitwiseAnd,
            op_BitwiseOr,
            op_Division,
            op_ExclusiveOr,
            op_LeftShift,
            op_Multiply,
            op_OnesComplement,
            op_RightShift,
            op_Subtraction,
            op_UnaryNegation,
            op_UnsignedRightShift,

            Count,
        }
    }
}
