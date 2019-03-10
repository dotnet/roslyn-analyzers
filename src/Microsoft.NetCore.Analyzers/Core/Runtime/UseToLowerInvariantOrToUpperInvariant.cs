// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class UseToLowerInvariantOrToUpperInvariantAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA_IHaveNoIdeaHowTheseAreNumbered";

        private static readonly LocalizableString s_localizableMessageAndTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.UseToLowerInvariantOrToUpperInvariantTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.UseToLowerInvariantOrToUpperInvariantDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableMessageAndTitle,
                                                                             s_localizableMessageAndTitle,
                                                                             DiagnosticCategory.Globalization,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription);

        internal const string ToLowerMethodName = "ToLower";
        internal const string ToUpperMethodName = "ToUpper";

        protected abstract Location GetMethodNameLocation(SyntaxNode invocationNode);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(operationContext =>
            {
                var operation = (IInvocationOperation)operationContext.Operation;
                IMethodSymbol methodSymbol = operation.TargetMethod;

                if (methodSymbol != null &&
                    methodSymbol.ContainingType.SpecialType == SpecialType.System_String &&
                    !methodSymbol.IsStatic &&
                    IsToLowerOrToUpper(methodSymbol.Name) &&
                    //picking the correct overload
                    methodSymbol.Parameters.Length == 0)
                {
                    operationContext.ReportDiagnostic(Diagnostic.Create(Rule, GetMethodNameLocation(operation.Syntax)));
                }
            }, OperationKind.Invocation);
        }

        private static bool IsToLowerOrToUpper(string methodName)
        {
            return string.Equals(methodName, ToLowerMethodName, StringComparison.Ordinal) ||
                string.Equals(methodName, ToUpperMethodName, StringComparison.Ordinal);
        }
    }
}
