// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    using static MicrosoftNetCoreAnalyzersResources;
    /// <summary>
    /// 
    /// </summary>
    public abstract class PreventNumericIntPtrUIntPtrBehavioralChanges : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2020";
        private static readonly string Explicit = nameof(Explicit);

        internal static readonly DiagnosticDescriptor OperatorThrowsRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesTitle)),
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesOperatorThrowsMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor ConversionThrowsRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesTitle)),
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesConversionThrowsMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor NotThrowRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesTitle)),
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesOperatorNotThrowMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            CreateLocalizableResourceString(nameof(PreventNumericIntPtrUIntPtrBehavioralChangesDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(OperatorThrowsRule, NotThrowRule);

        protected abstract bool IsWithinCheckedContext(IOperation operation);

        protected abstract bool NotAlias(ImmutableArray<SyntaxReference> syntaxReferences);

        protected abstract bool NotAlias(SyntaxNode syntax);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesRuntimeFeatureNumericIntPtr, out var _))
                {
                    // Numeric IntPtr feature not available
                    //return;
                }

                context.RegisterOperationAction(context =>
                {
                    if (context.Operation is IBinaryOperation binaryOperation &&
                        binaryOperation.IsAdditionOrSubstractionOperation(out var binaryOperator) &&
                        binaryOperation.IsChecked)
                    {
                        if (binaryOperation.LeftOperand.Type?.SpecialType == SpecialType.System_IntPtr ||
                             binaryOperation.LeftOperand.Type?.SpecialType == SpecialType.System_UIntPtr)
                        {
                            var symbol = GetSymbol(binaryOperation.LeftOperand);
                            if (symbol != null && NotAlias(symbol.DeclaringSyntaxReferences))
                            {
                                context.ReportDiagnostic(binaryOperation.CreateDiagnostic(OperatorThrowsRule, binaryOperator));
                                return;
                            }
                        }
                    }

                    if (context.Operation is IConversionOperation conversionOperation)
                    {
                        var operation = conversionOperation.WalkDownConversion(c => c.IsImplicit); // get innermost converesion
                        if (operation is IConversionOperation explicitConversion &&
                            explicitConversion.OperatorMethod == null) // Built in conversion
                        {
                            if (IsWithinCheckedContext(explicitConversion))
                            {
                                if (IsIntPtrToOrFromVoidPtrConversion(explicitConversion.Type, explicitConversion.Operand.Type))
                                {
                                    var symbol = GetSymbol(explicitConversion.Operand);
                                    if (symbol != null && NotAlias(symbol.DeclaringSyntaxReferences))
                                    {
                                        context.ReportDiagnostic(explicitConversion.CreateDiagnostic(ConversionThrowsRule,
                                            PopulateConversionString(explicitConversion.Type, explicitConversion.Operand.Type)));
                                    }
                                }
                                else if (IsIntPtrToOrFromVoidPtrConversion(explicitConversion.Operand.Type, explicitConversion.Type) &&
                                    NotAlias(explicitConversion.Syntax))
                                {
                                    context.ReportDiagnostic(explicitConversion.CreateDiagnostic(ConversionThrowsRule,
                                        PopulateConversionString(explicitConversion.Type, explicitConversion.Operand.Type)));
                                }
                            }
                            else // unchecked context
                            {
                                if ((IsLongToIntPtrConversion(explicitConversion.Type, explicitConversion.Operand.Type) ||
                                     IsULongToUIntPtrConversion(explicitConversion.Type, explicitConversion.Operand.Type)) &&
                                    NotAlias(explicitConversion.Syntax))
                                {
                                    context.ReportDiagnostic(explicitConversion.CreateDiagnostic(NotThrowRule,
                                        PopulateConversionString(explicitConversion.Type, explicitConversion.Operand.Type)));
                                }
                                else if (IsIntPtrToIntConversion(explicitConversion.Type, explicitConversion.Operand.Type) ||
                                         IsUIntPtrToUIntConversion(explicitConversion.Type, explicitConversion.Operand.Type))
                                {
                                    var symbol = GetSymbol(explicitConversion.Operand);
                                    if (symbol != null && NotAlias(symbol.DeclaringSyntaxReferences))
                                    {
                                        context.ReportDiagnostic(explicitConversion.CreateDiagnostic(NotThrowRule,
                                            PopulateConversionString(explicitConversion.Type, explicitConversion.Operand.Type)));
                                    }
                                }

                            }
                        }
                    }
                },
                OperationKind.Binary, OperationKind.Conversion);
            });

            static string PopulateConversionString(ITypeSymbol type, ITypeSymbol operand)
            {
                string typeName = type.Name;
                string operandName = operand.Name;

                if (type is IPointerTypeSymbol pointer)
                {
                    typeName = $"*{pointer.PointedAtType.Name}";
                }

                if (operand is IPointerTypeSymbol pointerOp)
                {
                    operandName = $"*{pointerOp.PointedAtType.Name}";
                }

                return $"({typeName}){operandName}";
            }

            static ISymbol? GetSymbol(IOperation operation) =>
                operation switch
                {
                    IFieldReferenceOperation fieldReference => fieldReference.Field,
                    IParameterReferenceOperation parameter => parameter.Parameter,
                    ILocalReferenceOperation local => local.Local,
                    _ => null,
                };

            static bool IsIntPtrToOrFromVoidPtrConversion(ITypeSymbol pointerType, ITypeSymbol intPtrType) =>
                intPtrType.SpecialType == SpecialType.System_IntPtr &&
                pointerType is IPointerTypeSymbol pointer && pointer.PointedAtType.SpecialType == SpecialType.System_Void;

            static bool IsLongToIntPtrConversion(ITypeSymbol convertingType, ITypeSymbol operandType) =>
                convertingType.SpecialType == SpecialType.System_IntPtr &&
                operandType.SpecialType == SpecialType.System_Int64;

            static bool IsIntPtrToIntConversion(ITypeSymbol convertingType, ITypeSymbol operandType) =>
                convertingType.SpecialType == SpecialType.System_Int32 &&
                operandType.SpecialType == SpecialType.System_IntPtr;

            static bool IsULongToUIntPtrConversion(ITypeSymbol convertingType, ITypeSymbol operandType) =>
                convertingType.SpecialType == SpecialType.System_UIntPtr &&
                operandType.SpecialType == SpecialType.System_UInt64;

            static bool IsUIntPtrToUIntConversion(ITypeSymbol convertingType, ITypeSymbol operandType) =>
                convertingType.SpecialType == SpecialType.System_UInt32 &&
                operandType.SpecialType == SpecialType.System_UIntPtr;
        }
    }
}

