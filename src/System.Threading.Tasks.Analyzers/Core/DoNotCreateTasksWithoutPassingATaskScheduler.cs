// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Threading.Tasks.Analyzers
{
    /// <summary>
    /// RS0018: Do not create tasks without passing a TaskScheduler
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCreateTasksWithoutPassingATaskSchedulerAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0018";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemThreadingTasksAnalyzersResources.DoNotCreateTasksWithoutPassingATaskSchedulerTitle), SystemThreadingTasksAnalyzersResources.ResourceManager, typeof(SystemThreadingTasksAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemThreadingTasksAnalyzersResources.DoNotCreateTasksWithoutPassingATaskSchedulerMessage), SystemThreadingTasksAnalyzersResources.ResourceManager, typeof(SystemThreadingTasksAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemThreadingTasksAnalyzersResources.DoNotCreateTasksWithoutPassingATaskSchedulerDescription), SystemThreadingTasksAnalyzersResources.ResourceManager, typeof(SystemThreadingTasksAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Check if TPL is available before actually doing the searches
                var taskType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var taskFactoryType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.TaskFactory");
                var taskSchedulerType = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.TaskScheduler");
                if (taskType == null || taskFactoryType == null || taskSchedulerType == null)
                {
                    return;
                }

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationExpression)operationContext.Operation;
                    if (invocation.IsInvalid)
                    {
                        return;
                    }

                    if (!IsMethodOfInterest(invocation.TargetMethod, taskType, taskFactoryType))
                    {
                        return;
                    }

                    // We want to ensure that all overloads called are explicitly taking a task scheduler
                    if (invocation.TargetMethod.Parameters.Any(p => p.Type.Equals(taskSchedulerType)))
                    {
                        return;
                    }

                    operationContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name));
                }, OperationKind.InvocationExpression);
            });
        }

        private bool IsMethodOfInterest(IMethodSymbol methodSymbol, INamedTypeSymbol taskType, INamedTypeSymbol taskFactoryType)
        {
            // Check if it's a method of Task or a derived type (for Task<T>)
            if ((taskType.Equals(methodSymbol.ContainingType) ||
                 taskType.Equals(methodSymbol.ContainingType.BaseType)) &&
                methodSymbol.Name == "ContinueWith")
            {
                return true;
            }

            if (methodSymbol.ContainingType.Equals(taskFactoryType) &&
                methodSymbol.Name == "StartNew")
            {
                return true;
            }

            return false;
        }
    }
}