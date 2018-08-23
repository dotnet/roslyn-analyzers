// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NetCore.Analyzers.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Analyzer.Utilities;
    using Analyzer.Utilities.Extensions;
    using Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ReviewCodeForSqlInjectionVulnerabilities : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA3001";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesTitle),
            MicrosoftNetCoreSecurityResources.ResourceManager,
            typeof(MicrosoftNetCoreSecurityResources));

        private static readonly LocalizableString Message = new LocalizableResourceString(
            nameof(MicrosoftNetCoreSecurityResources.ReviewCodeForSqlInjectionVulnerabilitiesMessage),
            MicrosoftNetCoreSecurityResources.ResourceManager,
            typeof(MicrosoftNetCoreSecurityResources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            Title,
            Message,
            DiagnosticCategory.Security,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            //context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                compilationContext.RegisterOperationBlockAction(operationBlockContext =>
                {
                    // Analyze methods.
                    if (!(operationBlockContext.OwningSymbol is IMethodSymbol containingMethod))
                    {
                        return;
                    }

                    // Perform analysis of all direct/indirect parameter usages in the method to get all non-validated usages that can cause a null dereference.
                    TaintedDataCfgAnalysisResult cfgAnalysisResult = null;
                    foreach (IOperation operationBlock in operationBlockContext.OperationBlocks)
                    {
                        if (operationBlock is IBlockOperation topmostBlock)
                        {
                            cfgAnalysisResult = TaintedDataAnalysis.GetOrComputeResult(topmostBlock, operationBlockContext.Compilation, containingMethod);
                            break;
                        }
                    }
                });
            });
        }
    }
}
