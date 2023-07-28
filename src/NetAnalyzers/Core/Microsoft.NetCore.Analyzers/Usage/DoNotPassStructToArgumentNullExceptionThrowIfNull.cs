// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.NetCore.Analyzers.MicrosoftNetCoreAnalyzersResources;

namespace Microsoft.NetCore.Analyzers.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotPassStructToArgumentNullExceptionThrowIfNullAnalyzer : DiagnosticAnalyzer
    {
        internal const string NonNullableStructRuleId = "CA2262";
        internal const string NullableStructRuleId = "CA1865";

        internal static readonly DiagnosticDescriptor DoNotPassNonNullableStructDiagnostic = DiagnosticDescriptorHelper.Create(
            NonNullableStructRuleId,
            CreateLocalizableResourceString(nameof(DoNotPassNonNullableStructToArgumentNullExceptionThrowIfNullTitle)),
            CreateLocalizableResourceString(nameof(DoNotPassNonNullableStructToArgumentNullExceptionThrowIfNullMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            CreateLocalizableResourceString(nameof(DoNotPassNonNullableStructToArgumentNullExceptionThrowIfNullDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor DoNotPassNullableStructDiagnostic = DiagnosticDescriptorHelper.Create(
            NullableStructRuleId,
            CreateLocalizableResourceString(nameof(DoNotPassNullableStructToArgumentNullExceptionThrowIfNullTitle)),
            CreateLocalizableResourceString(nameof(DoNotPassNullableStructToArgumentNullExceptionThrowIfNullMessage)),
            DiagnosticCategory.Performance,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(DoNotPassNullableStructToArgumentNullExceptionThrowIfNullDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(static context =>
            {
                var typeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                var throwIfNullMethod = typeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemArgumentNullException)
                    ?.GetMembers("ThrowIfNull")
                    .FirstOrDefault(m => m is IMethodSymbol method && method.Parameters[0].Type.SpecialType == SpecialType.System_Object);
                if (throwIfNullMethod is null)
                {
                    return;
                }

                context.RegisterOperationAction(ctx => AnalyzeInvocation(ctx, (IMethodSymbol)throwIfNullMethod), OperationKind.Invocation);
            });
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, IMethodSymbol throwIfNullMethod)
        {
            var invocation = (IInvocationOperation)context.Operation;
            if (invocation.TargetMethod.Equals(throwIfNullMethod))
            {
                if (invocation.Arguments[0].Value.WalkDownConversion().Type.IsNonNullableValueType())
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(DoNotPassNonNullableStructDiagnostic));
                }

                if (invocation.Arguments[0].Value.WalkDownConversion().Type.IsNullableValueType())
                {
                    context.ReportDiagnostic(invocation.CreateDiagnostic(DoNotPassNullableStructDiagnostic));
                }
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotPassNonNullableStructDiagnostic, DoNotPassNullableStructDiagnostic);
    }
}