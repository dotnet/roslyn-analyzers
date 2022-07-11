﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    using static CodeAnalysisDiagnosticsResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotUseCompilationGetSemanticModelAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.DoNotUseCompilationGetSemanticModelRuleId,
            CreateLocalizableResourceString(nameof(DoNotUseCompilationGetSemanticModelTitle)),
            CreateLocalizableResourceString(nameof(DoNotUseCompilationGetSemanticModelMessage)),
            DiagnosticCategory.MicrosoftCodeAnalysisCorrectness,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(DoNotUseCompilationGetSemanticModelDescription)),
            customTags: WellKnownDiagnosticTagsExtensions.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilationContext.Compilation);

                if (!wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisDiagnosticsDiagnosticAnalyzer, out var diagnosticAnalyzerType) ||
                    !wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisCompilation, out var compilationType))
                {
                    return;
                }

                var csharpCompilation = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisCSharpCSharpCompilation);
                var visualBasicCompilation = wellKnownTypeProvider.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisVisualBasicVisualBasicCompilation);

                compilationContext.RegisterOperationBlockStartAction(operationBlockContext =>
                {
                    if (operationBlockContext.OwningSymbol is IMethodSymbol methodSymbol &&
                        methodSymbol.ContainingType.Inherits(diagnosticAnalyzerType))
                    {
                        operationBlockContext.RegisterOperationAction(operationContext =>
                        {
                            var invocation = (IInvocationOperation)operationContext.Operation;

                            if (invocation.TargetMethod.Name.Equals("GetSemanticModel", StringComparison.Ordinal) &&
                                (
                                    invocation.TargetMethod.ContainingType.Equals(compilationType) ||
                                    invocation.TargetMethod.ContainingType.Equals(csharpCompilation) ||
                                    invocation.TargetMethod.ContainingType.Equals(visualBasicCompilation)
                                ))
                            {
                                operationContext.ReportDiagnostic(invocation.Syntax.CreateDiagnostic(Rule));
                            }
                        }, OperationKind.Invocation);
                    }
                });
            });
        }
    }
}