// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Roslyn.Diagnostics.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class CodeMayHaveMainThreadDependency : AbstractThreadDependencyAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyTitle), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyMessage), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(RoslynDiagnosticsAnalyzersResources.CodeMayHaveMainThreadDependencyDescription), RoslynDiagnosticsAnalyzersResources.ResourceManager, typeof(RoslynDiagnosticsAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RoslynDiagnosticIds.CodeMayHaveMainThreadDependencyRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.RoslyDiagnosticsReliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void HandleCompilationStart(CompilationStartAnalysisContext context, INamedTypeSymbol noMainThreadDependencyAttribute)
        {
            context.RegisterOperationBlockStartAction(HandleOperationBlockStart);
        }

        private void HandleOperationBlockStart(OperationBlockStartAnalysisContext context)
        {
            var threadDependencyInfo = GetThreadDependencyInfo(context.OwningSymbol);
            if (!threadDependencyInfo.IsExplicit || !threadDependencyInfo.Verified)
            {
                return;
            }

            context.RegisterOperationAction(ctx => HandleAwaitOperation(ctx, threadDependencyInfo), OperationKind.Await);
        }

        private void HandleAwaitOperation(OperationAnalysisContext context, ThreadDependencyInfo threadDependencyInfo)
        {
            var awaitOperation = (IAwaitOperation)context.Operation;
            var valueThreadDependencyInfo = GetThreadDependencyInfo(awaitOperation.Operation, captureContextUnlessConfigured: true);

            if (valueThreadDependencyInfo.AlwaysCompleted)
            {
                return;
            }

            if (!valueThreadDependencyInfo.AlwaysCompleted)
            {
                if (threadDependencyInfo.AlwaysCompleted)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
                    return;
                }

                if (valueThreadDependencyInfo.CapturesContext && !threadDependencyInfo.CapturesContext)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
                    return;
                }
            }

            if (valueThreadDependencyInfo.PerInstance && !threadDependencyInfo.PerInstance)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
                return;
            }
        }

        private ThreadDependencyInfo GetThreadDependencyInfo(IOperation operation, bool captureContextUnlessConfigured)
        {
            while (operation is IConversionOperation conversion)
            {
                if (conversion.OperatorMethod is object)
                {
                    return GetThreadDependencyInfoForReturn(conversion.OperatorMethod);
                }

                operation = conversion.Operand;
            }

            if (operation is IInvocationOperation invocation)
            {
                if (invocation.TargetMethod?.Name == nameof(Task.ConfigureAwait))
                {
                    var instanceDependencyInfo = GetThreadDependencyInfo(invocation.Instance, captureContextUnlessConfigured: false);
                    if (!instanceDependencyInfo.CapturesContext
                        && invocation.Arguments.Length == 1
                        && invocation.Arguments[0].Value.TryGetBoolConstantValue(out var continueOnCapturedContext)
                        && continueOnCapturedContext)
                    {
                        instanceDependencyInfo = instanceDependencyInfo.WithCapturesContext(true);
                    }

                    return instanceDependencyInfo;
                }
                else
                {
                    var targetDependencyInfo = GetThreadDependencyInfoForReturn(invocation.TargetMethod);
                    if (targetDependencyInfo.PerInstance)
                    {
                        var instanceDependencyInfo = GetThreadDependencyInfo(invocation.Instance, captureContextUnlessConfigured: false);
                        if (instanceDependencyInfo.IsExplicit && !instanceDependencyInfo.MayHaveMainThreadDependency)
                        {
                            targetDependencyInfo = targetDependencyInfo.WithPerInstance(false);
                        }
                    }

                    if (captureContextUnlessConfigured)
                    {
                        targetDependencyInfo = targetDependencyInfo.WithCapturesContext(true);
                    }

                    return targetDependencyInfo;
                }
            }

            if (operation is IParameterReferenceOperation parameterReference)
            {
                var parameterDependencyInfo = GetThreadDependencyInfo(parameterReference.Parameter);
                if (captureContextUnlessConfigured)
                {
                    parameterDependencyInfo = parameterDependencyInfo.WithCapturesContext(true);
                }

                return parameterDependencyInfo;
            }

            return ThreadDependencyInfo.DefaultAsynchronous;
        }
    }
}
