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

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotIgnoreReturnValueAnalyzer : DiagnosticAnalyzer
    {
        internal const string CA2022RuleId = "CA2022";

        private static readonly LocalizableString s_localizableMessage = CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueMessage));

        internal static readonly DiagnosticDescriptor DoNotIgnoreReturnValueRule = DiagnosticDescriptorHelper.Create(
            CA2022RuleId,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueTitle)),
            s_localizableMessage,
            DiagnosticCategory.Reliability,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DoNotIgnoreReturnValueRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationBlockStartAction(context =>
            {
                if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemDiagnosticsCodeAnalysisDoNotIgnoreAttribute, out var doNotIgnoreAttribute))
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var invocation = (IInvocationOperation)context.Operation;

                    if (!invocation.TargetMethod.ReturnsVoid &&
                        invocation.Parent.Kind == OperationKind.ExpressionStatement &&
                        invocation.TargetMethod.GetReturnTypeAttributes().Any(static (a, arg) => a.AttributeClass == arg, doNotIgnoreAttribute))
                    {
                        context.ReportDiagnostic(invocation.CreateDiagnostic(DoNotIgnoreReturnValueRule, invocation.TargetMethod.FormatMemberName()));
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}
