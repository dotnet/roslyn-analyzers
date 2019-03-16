// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.PerformanceSensitive.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal sealed class ExplicitAllocationAnalyzer : AbstractAllocationAnalyzer
    {
        public const string NewArrayRuleId = "HAA0501";
        private static readonly LocalizableString s_localizableNewArrayRuleTitleAndMessage = new LocalizableResourceString(nameof(PerformanceSensitiveAnalyzersResources.NewArrayRuleTitleAndMessage), PerformanceSensitiveAnalyzersResources.ResourceManager, typeof(PerformanceSensitiveAnalyzersResources));
        internal static DiagnosticDescriptor NewArrayRule = new DiagnosticDescriptor(
            NewArrayRuleId,
            s_localizableNewArrayRuleTitleAndMessage,
            s_localizableNewArrayRuleTitleAndMessage,
            DiagnosticCategory.Performance,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        private static readonly object[] EmptyMessageArgs = Array.Empty<object>();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NewArrayRule);

        protected override ImmutableArray<OperationKind> Operations => ImmutableArray.Create(
            OperationKind.ArrayCreation);

        protected override void AnalyzeNode(OperationAnalysisContext context, in PerformanceSensitiveInfo info)
        {
            if (context.Operation is IArrayCreationOperation)
            {
                context.ReportDiagnostic(Diagnostic.Create(NewArrayRule, context.Operation.Syntax.GetLocation(), EmptyMessageArgs));
                return;
            }
        }
    }
}