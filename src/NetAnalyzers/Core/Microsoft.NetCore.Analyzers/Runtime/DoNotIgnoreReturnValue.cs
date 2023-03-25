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

        internal static readonly DiagnosticDescriptor DoNotIgnoreReturnValueRule = DiagnosticDescriptorHelper.Create(
            CA2022RuleId,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueTitle)),
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        internal static readonly DiagnosticDescriptor DoNotIgnoreReturnValueRuleWithMessage = DiagnosticDescriptorHelper.Create(
            CA2022RuleId,
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueTitle)),
            CreateLocalizableResourceString(nameof(DoNotIgnoreReturnValueMessageCustom)),
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

                    if (!invocation.TargetMethod.ReturnsVoid && invocation.Parent.Kind == OperationKind.ExpressionStatement)
                    {
                        var attributeApplied = invocation.TargetMethod.GetReturnTypeAttributes().WhereAsArray(a => a.AttributeClass == doNotIgnoreAttribute);

                        if (!attributeApplied.IsEmpty)
                        {
                            var message = attributeApplied[0].NamedArguments.WhereAsArray(arg => arg.Key == "Message");
                            var messageStr = message.IsEmpty ? null : (string)message[0].Value.Value;

                            if (!string.IsNullOrEmpty(messageStr))
                            {
                                context.ReportDiagnostic(invocation.CreateDiagnostic(DoNotIgnoreReturnValueRuleWithMessage, invocation.TargetMethod.FormatMemberName(), messageStr!));
                            }
                            else
                            {
                                context.ReportDiagnostic(invocation.CreateDiagnostic(DoNotIgnoreReturnValueRule, invocation.TargetMethod.FormatMemberName()));
                            }
                        }
                    }
                }, OperationKind.Invocation);
            });
        }
    }
}
