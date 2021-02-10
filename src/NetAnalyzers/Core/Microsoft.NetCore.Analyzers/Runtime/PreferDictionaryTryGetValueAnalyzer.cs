// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class PreferDictionaryTryGetValueAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1840";
        
        private const string ContainsKeyMethodName = nameof(IDictionary<dynamic, dynamic>.ContainsKey);
        
        internal static readonly DiagnosticDescriptor ContainsKeyRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            "s_localizableTitle",
            "s_localizableContainsKeyMessage",
            DiagnosticCategory.Performance,
            RuleLevel.BuildWarning,
            "s_localizableContainsKeyDescription",
            isPortedFxCopRule: false,
            isDataflowRule: false);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ContainsKeyRule);
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext compilationContext)
        {
            var compilation = compilationContext.Compilation;

            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericICollection1, out _))
                return;
            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIDictionary2, out var dictionaryType))
                return;
            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemCollectionsGenericIEnumerable1, out _))
                return;

            compilationContext.RegisterOperationAction(OnOperationAction, OperationKind.Conditional);
        }

        private void OnOperationAction(OperationAnalysisContext context)
        {
            var invocation = (IConditionalOperation)context.Operation;
            if (invocation.Condition is IInvocationOperation invocationOperation && invocationOperation.TargetMethod.Name == ContainsKeyMethodName)
            {
                Console.WriteLine(true);
            }
            
            Console.WriteLine(invocation.Kind);
        }
    }
}