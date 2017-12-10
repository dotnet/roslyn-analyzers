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
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
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
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

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

            if (!targetMethod.Parameters.Any(p => p.Type == cancellationTokenType))
            {
                // The target method does not accept a CancellationToken. Check if it has a suitable overload that does.
                var methodName = targetMethod.Name;
                var overloads = targetMethod.ContainingType.GetMembers(methodName).OfType<IMethodSymbol>();
                var targetOverload = overloads.FirstOrDefault(
                    m => IsOverloadWithCancellationToken(m, targetMethod, cancellationTokenType));

                if (targetOverload == null)
                {
                    // No suitable overload is available.
                    return false;
                }
            }
            else
            {
                // The target method accepts a CancellationToken. Check if default(CancellationToken) or CancellationToken.None is being passed.
                var argument = invocation.Arguments.FirstOrDefault(a => a.Type == cancellationTokenType);

                // All valid code should result in 'argument' being non-null. Even if the CancellationToken is an optional
                // parameter and the caller does not explicitly pass it, 'argument' will still correspond to a compiler-generated
                // CancellationToken value. However, check for null to ensure we don't crash on invalid code.
                if (argument != null && !IsCancellationTokenNone(argument, cancellationTokenType))
                {
                    // A non-default CancellationToken is being passed.
                    return false;
                }
            }

            // Check that a suitable variable of type CancellationToken is in scope.
            if (!IsCancellationTokenVariableInScope(invocation, cancellationTokenType, compilation))
            {
                return false;
            }

            return true;
        }

        private static bool IsCancellationTokenNone(IArgumentOperation argument, INamedTypeSymbol cancellationTokenType)
        {
            // This method returns true for CancellationToken.None, explicitly-passed default(CancellationToken),
            // and compiler-generated default(CancellationToken) (e.g. as the value of an optional parameter).
            // Note that default(CancellationToken) is equivalent to CancellationToken.None.

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

        private static bool IsCancellationTokenVariableInScope(IInvocationOperation invocation, INamedTypeSymbol cancellationTokenType, Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(invocation.Syntax.SyntaxTree);
            int position = invocation.Syntax.SpanStart;
            return semanticModel.LookupSymbols(position).Any(IsAccessibleCancellationTokenVariable);

            bool IsAccessibleCancellationTokenVariable(ISymbol symbol)
            {
                switch (symbol)
                {
                    case IFieldSymbol field:
                        return field.Type == cancellationTokenType;
                    case ILocalSymbol local:
                        return local.Type == cancellationTokenType && !local.IsInaccessibleLocal(position);
                    default:
                        return false;
                }
            }
        }

        private static bool IsOverloadWithCancellationToken(
            IMethodSymbol overload,
            IMethodSymbol original,
            INamedTypeSymbol cancellationTokenType)
        {
            var overloadParameters = overload.Parameters;
            var originalParameters = original.Parameters;

            if (overloadParameters.Length != originalParameters.Length + 1)
            {
                return false;
            }

            // Check whether they start with the same parameters.
            int count = originalParameters.Length;
            for (int i = 0; i < count; i++)
            {
                if (overloadParameters[i].Type != originalParameters[i].Type)
                {
                    return false;
                }
            }

            // Check whether the overload's last parameter has type CancellationToken.
            return overloadParameters.Last().Type == cancellationTokenType;
        }
    }
}