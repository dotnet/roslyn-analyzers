// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// Async005: Propagate CancellationTokens When Possible
    /// </summary>
    public abstract class PropagateCancellationTokensWhenPossibleAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "Async005";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropagateCancellationTokensWhenPossibleTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropagateCancellationTokensWhenPossibleMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.PropagateCancellationTokensWhenPossibleDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Library,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,     // TODO: add MSDN url
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();

            // TODO: Include this?
            // analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            if (InvocationShouldPassCancellationToken(invocation, context.Compilation))
            {
                context.ReportDiagnostic(invocation.Syntax.CreateDiagnostic(Rule));
            }
        }

        private static bool InvocationShouldPassCancellationToken(IInvocationOperation invocation, Compilation compilation)
        {
            var cancellationTokenType = WellKnownTypes.CancellationToken(compilation);

            var targetMethod = invocation.TargetMethod;
            if (targetMethod == null)
            {
                return false;
            }

            if (!AcceptsCancellationTokens(targetMethod))
            {
                // Check if there is a different overload of the method that accepts a CancellationToken.
                var methodName = targetMethod.Name;
                var overloads = targetMethod.ContainingType.GetMembers(methodName).OfType<IMethodSymbol>();
                var targetOverload = overloads.FirstOrDefault(
                    m => AcceptsCancellationTokens(m) && StartsWithSameParameters(m, targetMethod));

                if (targetOverload == null)
                {
                    return false;
                }
            }
            else
            {
                // Check if default(CancellationToken) or CancellationToken.None is being passed in.
                var argument = invocation.Arguments.FirstOrDefault(a => a.Type == cancellationTokenType);
                if (argument != null && !IsCancellationTokenNone(argument))
                {
                    return false;
                }
            }

            return true;

            bool AcceptsCancellationTokens(IMethodSymbol method)
            {
                return method.Parameters.Any(p => p.Type == cancellationTokenType);
            }

            bool IsCancellationTokenNone(IArgumentOperation argument)
            {
                var argumentValue = argument.Value;
                switch (argumentValue.Kind)
                {
                    case OperationKind.DefaultValue:
                        return true;
                    case OperationKind.PropertyReference:
                        var property = ((IPropertyReferenceOperation)argumentValue).Property;
                        if (property.Name == "None" && property.ContainingType == cancellationTokenType)
                        {
                            return true;
                        }
                        break;
                }

                return false;
            }
        }

        private static bool StartsWithSameParameters(IMethodSymbol first, IMethodSymbol second)
        {
            int count = Math.Min(first.Parameters.Length, second.Parameters.Length);

            for (int i = 0; i < count; i++)
            {
                if (first.Parameters[i].Type != second.Parameters[i].Type)
                {
                    return false;
                }
            }

            return true;
        }
    }
}