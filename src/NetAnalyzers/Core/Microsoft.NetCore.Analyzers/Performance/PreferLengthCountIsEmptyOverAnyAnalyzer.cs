// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// Prefer .Length/Count/IsEmpty over Any()
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class PreferLengthCountIsEmptyOverAnyAnalyzer : DiagnosticAnalyzer
    {
        private const string AnyText = nameof(Enumerable.Any);

        internal const string IsEmptyText = nameof(ImmutableArray<dynamic>.IsEmpty);
        internal const string LengthText = nameof(Array.Length);
        internal const string CountText = nameof(ICollection.Count);

        internal const string IsEmptyId = "CA1860";
        internal const string LengthId = "CA1861";
        internal const string CountId = "CA1862";

        private const string SourceParameterName = "source";

        internal static readonly DiagnosticDescriptor IsEmptyDescriptor = DiagnosticDescriptorHelper.Create(
            IsEmptyId,
            CreateLocalizableResourceString(nameof(PreferIsEmptyOverAnyTitle)),
            CreateLocalizableResourceString(nameof(PreferIsEmptyOverAnyMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(PreferIsEmptyOverAnyDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false
        );

        internal static readonly DiagnosticDescriptor LengthDescriptor = DiagnosticDescriptorHelper.Create(
            LengthId,
            CreateLocalizableResourceString(nameof(PreferLengthOverAnyTitle)),
            CreateLocalizableResourceString(nameof(PreferLengthOverAnyMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(PreferLengthOverAnyDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false
        );

        internal static readonly DiagnosticDescriptor CountDescriptor = DiagnosticDescriptorHelper.Create(
            CountId,
            CreateLocalizableResourceString(nameof(PreferCountOverAnyTitle)),
            CreateLocalizableResourceString(nameof(PreferCountOverAnyMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(PreferCountOverAnyDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(OnInvocationAnalysis, OperationKind.Invocation);
        }

        private static void OnInvocationAnalysis(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var originalMethod = invocation.TargetMethod.OriginalDefinition;
            if (IsEligibleAnyMethod(originalMethod))
            {
                // The collection will be passed as the instance in VB and as an argument in C#. 
                var firstArgument = invocation.Instance ?? invocation.Arguments[0].Value;
                var type = (firstArgument as IConversionOperation)?.Operand.Type ?? firstArgument.Type;
                if (HasEligibleIsEmptyProperty(type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(IsEmptyDescriptor, invocation.Syntax.GetLocation()));

                    return;
                }

                if (HasEligibleLengthProperty(type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(LengthDescriptor, invocation.Syntax.GetLocation()));

                    return;
                }

                if (HasEligibleCountProperty(type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(CountDescriptor, invocation.Syntax.GetLocation()));

                    return;
                }
            }
        }

        private static bool IsEligibleAnyMethod(IMethodSymbol method)
        {
            return method.Name == AnyText
                   && method.ReturnType.SpecialType == SpecialType.System_Boolean
                   && method.Language == LanguageNames.CSharp && method.Parameters.Length == 1
                   || (method.Language == LanguageNames.VisualBasic && (method.Parameters.Length == 0 || method.Parameters.Length == 1 && method.Parameters[0].Name == SourceParameterName));
        }

        private static bool HasEligibleIsEmptyProperty(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers(IsEmptyText)
                .Any(symbol => symbol is IPropertySymbol property && property.Type.SpecialType == SpecialType.System_Boolean);
        }

        private static bool HasEligibleLengthProperty(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol)
            {
                return true;
            }

            return typeSymbol.GetMembers(LengthText)
                .Any(symbol => symbol is IPropertySymbol property && property.Type.SpecialType == SpecialType.System_Int32);
        }

        private static bool HasEligibleCountProperty(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers(CountText)
                .Any(symbol => symbol is IPropertySymbol property && property.Type.SpecialType == SpecialType.System_Int32);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            LengthDescriptor,
            CountDescriptor,
            IsEmptyDescriptor
        );
    }
}