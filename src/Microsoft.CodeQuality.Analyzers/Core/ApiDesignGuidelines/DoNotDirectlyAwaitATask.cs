﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

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
            isEnabledByDefault: true,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(context =>
            {
                ImmutableArray<INamedTypeSymbol> taskTypes = GetTaskTypes(context.Compilation);
                if (taskTypes.Any(t => t == null))
                {
                    return;
                }

                context.RegisterOperationActionInternal(oc => AnalyzeOperation(oc, taskTypes), OperationKind.AwaitExpression);
            });
        }

        private static void AnalyzeOperation(OperationAnalysisContext context, ImmutableArray<INamedTypeSymbol> taskTypes)
        {
            IAwaitExpression awaitExpression = context.Operation as IAwaitExpression;

            // Get the type of the expression being awaited and check it's a task type.
            ITypeSymbol typeOfAwaitedExpression = awaitExpression?.AwaitedValue?.Type;
            if (typeOfAwaitedExpression != null && taskTypes.Contains(typeOfAwaitedExpression.OriginalDefinition))
            {
                context.ReportDiagnostic(awaitExpression.AwaitedValue.Syntax.CreateDiagnostic(Rule));
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
