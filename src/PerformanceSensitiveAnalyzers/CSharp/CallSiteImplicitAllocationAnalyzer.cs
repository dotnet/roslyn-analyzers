// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PerformanceSensitiveAnalyzers;

namespace Microsoft.CodeAnalysis.CSharp.PerformanceSensitiveAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CallSiteImplicitAllocationAnalyzer : AbstractAllocationAnalyzer
    {
        public const string ParamsParameterRuleId = "HAA0101";
        public const string ValueTypeNonOverridenCallRuleId = "HAA0102";

        private static readonly LocalizableString s_localizableParamsParameterRuleTitle = new LocalizableResourceString(nameof(AnalyzersResources.ParamsParameterRuleTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString s_localizableParamsParameterRuleMessage = new LocalizableResourceString(nameof(AnalyzersResources.ParamsParameterRuleMessage), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));

        private static readonly LocalizableString s_localizableValueTypeNonOverridenCallRuleTitle = new LocalizableResourceString(nameof(AnalyzersResources.ValueTypeNonOverridenCallRuleTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString s_localizableValueTypeNonOverridenCallRuleMessage = new LocalizableResourceString(nameof(AnalyzersResources.ValueTypeNonOverridenCallRuleMessage), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));


        internal static DiagnosticDescriptor ParamsParameterRule = new DiagnosticDescriptor(
            ParamsParameterRuleId,
            s_localizableParamsParameterRuleTitle,
            s_localizableParamsParameterRuleMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        internal static DiagnosticDescriptor ValueTypeNonOverridenCallRule = new DiagnosticDescriptor(
            ValueTypeNonOverridenCallRuleId,
            s_localizableValueTypeNonOverridenCallRuleTitle,
            s_localizableValueTypeNonOverridenCallRuleMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ParamsParameterRule, ValueTypeNonOverridenCallRule);

        protected override ImmutableArray<OperationKind> Operations => ImmutableArray.Create(OperationKind.Invocation);

        private static readonly object[] EmptyMessageArgs = Array.Empty<object>();

        protected override void AnalyzeNode(OperationAnalysisContext context, in PerformanceSensitiveInfo info)
        {
            if (!(context.Operation is IInvocationOperation invocation))
            {
                return;
            }

            if (invocation.TargetMethod.IsOverride)
            {
                var type = invocation.TargetMethod.ContainingType;
                if (string.Equals(type.Name, "ValueType", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(type.Name, "Enum", StringComparison.OrdinalIgnoreCase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ValueTypeNonOverridenCallRule, invocation.Syntax.GetLocation(), EmptyMessageArgs));
                }
            }

            for (int i = invocation.Arguments.Length - 1; i >= 0; i--)
            {
                if (invocation.Arguments[i].ArgumentKind == ArgumentKind.ParamArray && invocation.Arguments[i].IsImplicit)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ParamsParameterRule, invocation.Syntax.GetLocation(), EmptyMessageArgs));
                    return;
                }
            }
        }
    }
}