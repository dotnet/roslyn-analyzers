// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA2007: Do not directly await a Task in libraries. Append ConfigureAwait(false) to the task.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDirectlyAwaitATaskAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2007";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDirectlyAwaitATaskTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDirectlyAwaitATaskMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.DoNotDirectlyAwaitATaskDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Reliability,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca2007-do-not-directly-await-task",
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(context =>
            {
                if (!context.Options.GetOutputKindsOption(Rule, context.CancellationToken).Contains(context.Compilation.Options.OutputKind))
                {
                    // Configured to skip analysis for the compilation's output kind
                    return;
                }

                ImmutableArray<INamedTypeSymbol> taskTypes = GetTaskTypes(context.Compilation);
                if (taskTypes.Any(t => t == null))
                {
                    return;
                }

                context.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    if (operationBlockStartContext.OwningSymbol is IMethodSymbol method)
                    {
                        if (method.IsAsync &&
                            method.ReturnsVoid &&
                            operationBlockStartContext.Options.GetBoolOptionValue(
                                optionName: EditorConfigOptionNames.ExcludeAsyncVoidMethods,
                                rule: Rule,
                                defaultValue: false,
                                cancellationToken: operationBlockStartContext.CancellationToken))
                        {
                            // Configured to skip this analysis in async void methods.
                            return;
                        }

                        operationBlockStartContext.RegisterOperationAction(oc => AnalyzeOperation(oc, taskTypes), OperationKind.Await);
                    }
                });
            });
        }

        private static void AnalyzeOperation(OperationAnalysisContext context, ImmutableArray<INamedTypeSymbol> taskTypes)
        {
            IAwaitOperation awaitExpression = context.Operation as IAwaitOperation;

            // Get the type of the expression being awaited and check it's a task type.
            ITypeSymbol typeOfAwaitedExpression = awaitExpression?.Operation?.Type;
            if (typeOfAwaitedExpression != null && taskTypes.Contains(typeOfAwaitedExpression.OriginalDefinition))
            {
                context.ReportDiagnostic(awaitExpression.Operation.Syntax.CreateDiagnostic(Rule));
            }
        }

        private static ImmutableArray<INamedTypeSymbol> GetTaskTypes(Compilation compilation)
        {
            INamedTypeSymbol taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            INamedTypeSymbol taskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            return ImmutableArray.Create(taskType, taskOfTType);
        }
    }
}
