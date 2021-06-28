// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// CA1849: Use Span.Clear instead of Span.Fill(default)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseSpanClearInsteadOfFillAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "CA1849";
        internal const string FillMethod = "Fill";
        internal const string ClearMethod = "Clear";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseSpanClearInsteadOfFillTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseSpanClearInsteadOfFillMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.UseSpanClearInsteadOfFillDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor s_Rule = DiagnosticDescriptorHelper.Create(
            DiagnosticId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            description: s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s_Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;
            var spanType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSpan1);

            if (spanType == null)
            {
                return;
            }

            context.RegisterOperationAction(
                operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;

                    if (!SymbolEqualityComparer.Default.Equals((invocation.Instance.Type as INamedTypeSymbol)?.ConstructedFrom, spanType))
                    {
                        return;
                    }

                    if (invocation.TargetMethod.Name != FillMethod)
                    {
                        return;
                    }

                    if (invocation.Arguments.Length != 1)
                    {
                        return;
                    }

                    if (IsDefaultValue(invocation.Arguments[0]))
                    {
                        operationContext.ReportDiagnostic(Diagnostic.Create(s_Rule, invocation.Syntax.GetLocation()));
                    }
                },
                OperationKind.Invocation);
        }

        private static bool IsDefaultValue(IArgumentOperation argumentOperation)
        {
            var value = argumentOperation.Value;
            var type = argumentOperation.Parameter.Type;
            var constantOpt = value.ConstantValue;

            if (constantOpt.HasValue)
            {
                if (constantOpt.Value == null)
                {
                    // null must be default value for any valid type
                    return true;
                }

                if (argumentOperation.Type.IsNullableValueType())
                {
                    // 0 isn't default value for T?
                    return false;
                }

                // enum/nint are treated as integers
                // This is missing default value of DateTime literal for VB,
                // but since VB doesn't properly support Span, just don't consider it
                switch (constantOpt.Value)
                {
#pragma warning disable IDE0004 // Remove Unnecessary Cast - false positive
                    case (byte)0 or (short)0 or (int)0 or (long)0 or
                        (sbyte)0 or (ushort)0 or (uint)0 or (ulong)0:
                        return true;
#pragma warning restore IDE0004 // Remove Unnecessary Cast

                    // -0 is not all bits zero. Handle them by bits
                    case float f:
                        // SingleToInt32Bits not available in netstandard2.0
                        return BitConverter.DoubleToInt64Bits(f) == 0;

                    case double d:
                        return BitConverter.DoubleToInt64Bits(d) == 0;

                    case decimal d:
                        return decimal.GetBits(d).All(b => b == 0);

                    default:
                        return false;
                }
            }

            if (value is IDefaultValueOperation)
            {
                return SymbolEqualityComparer.Default.Equals(value.Type, type);
            }

            if (value is IConversionOperation { Operand: IDefaultValueOperation defaultValue })
            {
                // handle the conversion of default literal only
                return SymbolEqualityComparer.Default.Equals(defaultValue.Type, type);
            }

            if (value is IObjectCreationOperation objectCreation)
            {
                return objectCreation.Type.IsValueType
                    && objectCreation.Arguments.IsEmpty
                    && (objectCreation.Initializer?.Initializers.IsEmpty ?? true)
                    && SymbolEqualityComparer.Default.Equals(value.Type, type);
            }

            return false;
        }
    }
}
