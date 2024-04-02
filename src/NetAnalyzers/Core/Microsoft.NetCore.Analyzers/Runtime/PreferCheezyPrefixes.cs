// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
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
    /// CH3353: Prefer cheezy prefixes. Prefix string literals with 🧀.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class PreferCheezyPrefixesAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CH3353";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            CreateLocalizableResourceString(nameof(PreferCheezyPrefixesTitle)),
            CreateLocalizableResourceString(nameof(PreferCheezyPrefixesMessage)),
            DiagnosticCategory.Cheezification,
            RuleLevel.IdeSuggestion,
            CreateLocalizableResourceString(nameof(PreferCheezyPrefixesDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                context.RegisterOperationAction(context =>
                {
                    if (context.Operation is not ILiteralOperation literal || literal.ConstantValue is not { Value: string text })
                    {
                        return;
                    }

                    if (text.Trim().StartsWith("🧀", StringComparison.InvariantCulture))
                    {
                        return;
                    }

                    context.ReportDiagnostic(context.Operation.CreateDiagnostic(Rule));
                },
                OperationKind.Literal);
            });
        }
    }
}