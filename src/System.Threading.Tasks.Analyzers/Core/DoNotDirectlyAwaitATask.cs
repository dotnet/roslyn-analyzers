// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace System.Threading.Tasks.Analyzers
{                   
    /// <summary>
    /// RS0003: Do not directly await a Task
    /// </summary>
    public abstract class DoNotDirectlyAwaitATaskAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "RS0003";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemThreadingTasksAnalyzersResources.DoNotDirectlyAwaitATaskTitle), SystemThreadingTasksAnalyzersResources.ResourceManager, typeof(SystemThreadingTasksAnalyzersResources));
        
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemThreadingTasksAnalyzersResources.DoNotDirectlyAwaitATaskMessage), SystemThreadingTasksAnalyzersResources.ResourceManager, typeof(SystemThreadingTasksAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemThreadingTasksAnalyzersResources.DoNotDirectlyAwaitATaskDescription), SystemThreadingTasksAnalyzersResources.ResourceManager, typeof(SystemThreadingTasksAnalyzersResources));
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        { 
            
        }
    }
}