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
using System.Collections.Generic;
using System.Threading;

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
            var candidateTokens = GetCandidateCancellationTokensToPass(invocation, context.Compilation);
            if (candidateTokens.Any())
            {
                string candidateTokensText = FormatCandidateTokens(candidateTokens);
                context.ReportDiagnostic(invocation.Syntax.CreateDiagnostic(Rule, candidateTokensText));
            }
        }

        private static string FormatCandidateTokens(IEnumerable<ISymbol> candidateTokens)
        {
            // [ct1, ct2, ct3] => "'ct1', 'ct2', 'ct3'"
            return string.Join(", ", candidateTokens.Select(s => $"'{s.Name}'"));
        }

        /// <summary>
        /// Returns the candidate <see cref="CancellationToken"/>s to pass at an invocation.
        /// Returns an empty collection when no <see cref="CancellationToken"/>s need to be propagated,
        /// or no candidate <see cref="CancellationToken"/>s are in scope.
        /// </summary>
        private static IEnumerable<ISymbol> GetCandidateCancellationTokensToPass(
            IInvocationOperation invocation,
            Compilation compilation)
        {
            var cancellationTokenType = WellKnownTypes.CancellationToken(compilation);

            var targetMethod = invocation.TargetMethod;
            if (targetMethod == null)
            {
                return Enumerable.Empty<ISymbol>();
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
                    return Enumerable.Empty<ISymbol>();
                }
            }
            else
            {
                // The target method accepts a CancellationToken. Check if default(CancellationToken) or CancellationToken.None is being passed.
                var argumentValue = invocation.Arguments
                    .Select(argument => argument.Value)
                    .FirstOrDefault(value => value.Type == cancellationTokenType);

                // All valid code should result in 'argumentValue' being non-null. Even if the CancellationToken is an optional
                // parameter and the caller does not explicitly pass it, 'argumentValue' will still correspond to a compiler-generated
                // CancellationToken value. However, check for null to ensure we don't crash on invalid code.
                if (argumentValue == null)
                {
                    // The code is invalid. The compiler error will be enough warning for the developer.
                    return Enumerable.Empty<ISymbol>();
                }

                if (!IsCancellationTokenNone(argumentValue, cancellationTokenType))
                {
                    // A non-default CancellationToken is being passed.
                    return Enumerable.Empty<ISymbol>();
                }
            }

            return GetCancellationTokenVariablesInScope(invocation, cancellationTokenType, compilation);
        }

        private static bool IsCancellationTokenNone(IOperation value, INamedTypeSymbol cancellationTokenType)
        {
            // This method returns true for CancellationToken.None, explicitly-passed default(CancellationToken),
            // and compiler-generated default(CancellationToken) (e.g. as the value of an optional parameter).
            // Note that default(CancellationToken) is equivalent to CancellationToken.None.

            Debug.Assert(value.Type == cancellationTokenType);

            switch (value.Kind)
            {
                case OperationKind.DefaultValue:
                    return true;
                case OperationKind.PropertyReference:
                    var property = ((IPropertyReferenceOperation)value).Property;
                    if (property.Name == "None" && property.ContainingType == cancellationTokenType)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        private static IEnumerable<ISymbol> GetCancellationTokenVariablesInScope(
            IInvocationOperation invocation,
            INamedTypeSymbol cancellationTokenType,
            Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(invocation.Syntax.SyntaxTree);
            int position = invocation.Syntax.SpanStart;
            return semanticModel.LookupSymbols(position).Where(IsUsableCancellationTokenVariable);

            bool IsUsableCancellationTokenVariable(ISymbol symbol)
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.Local:
                        var local = (ILocalSymbol)symbol;
                        return local.Type == cancellationTokenType && !local.IsInaccessibleLocal(position);
                    case SymbolKind.Parameter:
                        var parameter = (IParameterSymbol)symbol;
                        return parameter.Type == cancellationTokenType;
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