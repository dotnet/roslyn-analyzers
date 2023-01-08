// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// Prefer using 'IsEmpty' or comparing 'Count' / 'Length' property to 0 rather than using 'Any()', both for clarity and for performance.
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            LengthDescriptor,
            CountDescriptor,
            IsEmptyDescriptor
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
            if (originalMethod.MethodKind == MethodKind.ReducedExtension)
            {
                originalMethod = originalMethod.ReducedFrom;
            }

            if (IsEligibleAnyMethod(originalMethod))
            {
                var type = invocation.GetReceiverType(context.Compilation, beforeConversion: true, context.CancellationToken);
                if (type is null)
                {
                    return;
                }

                if (HasEligibleIsEmptyProperty(type))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(IsEmptyDescriptor));

                    return;
                }

                if (HasEligibleLengthProperty(type))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(LengthDescriptor));

                    return;
                }

                if (HasEligibleCountProperty(type))
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(CountDescriptor));

                    return;
                }
            }
        }

        private static bool IsEligibleAnyMethod(IMethodSymbol method)
        {
            return method is
            {
                Name: AnyText,
                ReturnType.SpecialType: SpecialType.System_Boolean,
                IsExtensionMethod: true,
                Parameters: [_]
            };
        }

        private static bool HasEligibleIsEmptyProperty(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers(IsEmptyText)
                .OfType<IPropertySymbol>()
                .Any(property => property.Type.SpecialType == SpecialType.System_Boolean);
        }

        private static bool HasEligibleLengthProperty(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol)
            {
                return true;
            }

            return typeSymbol.GetMembers(LengthText)
                .OfType<IPropertySymbol>()
                .Any(property => property.Type.SpecialType is SpecialType.System_Int32 or SpecialType.System_UInt32);
        }

        private static bool HasEligibleCountProperty(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers(CountText)
                .OfType<IPropertySymbol>()
                .Any(property => property.Type.SpecialType is SpecialType.System_Int32 or SpecialType.System_UInt32);
        }
    }
}