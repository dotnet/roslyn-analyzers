// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DetectPLINQNopsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2250";
        private static readonly string[] s_knownCalls = new string[] { "ToList", "ToArray" };
        internal static readonly LocalizableString localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPLINQNopsTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPLINQNopsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPLINQNopsDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor DefaultRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Usage,
                                                                             RuleLevel.BuildWarning,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false,
                                                                             isEnabledByDefaultInFxCopAnalyzers: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!ctx.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqParallelEnumerable, out var parallelEnumerable))
                {
                    return;
                }

                var asParallelSymbols = parallelEnumerable.GetMembers("AsParallel").ToImmutableHashSet();
                var toArraySymbols = parallelEnumerable.GetMembers("ToArray").ToImmutableHashSet();
                var toListSymbols = parallelEnumerable.GetMembers("ToList").ToImmutableHashSet();

                ctx.RegisterSyntaxNodeAction(x => AnalyzeSymbol(x, asParallelSymbols, toArraySymbols, toListSymbols), SyntaxKind.InvocationExpression);
            });
        }
        private abstract class C
        {
            protected abstract void Call2(params string[] arr);
        }
        private class D : C
        {
            protected override void Call2(string[] arr)
            {
                throw new NotImplementedException();
            }
        }
        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context, ImmutableHashSet<ISymbol> asParallelSymbols, ImmutableHashSet<ISymbol> toArraySymbols, ImmutableHashSet<ISymbol> toListSymbols)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) // we are only interested in calls on a member
            {
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol is not IMethodSymbol methodSymbol)
            {
                return;
            }

            if (methodSymbol.ReducedFrom is null) //if we have no reduction it can not match the symbol definition
            {
                return;
            }

            var reducedSymbol = methodSymbol.ReducedFrom;
            if (!asParallelSymbols.Contains(reducedSymbol))
            {
                if (!(toArraySymbols.Contains(reducedSymbol) || toListSymbols.Contains(reducedSymbol))) //Not toList or ToArray call nor AsParallel
                {
                    return;
                }

                if (memberAccess.Expression is InvocationExpressionSyntax nestedInvocation && nestedInvocation.Expression is MemberAccessExpressionSyntax) //AsParallel may precede this call, making it a no-op as well
                {
                    if (context.SemanticModel.GetSymbolInfo(nestedInvocation.Expression).Symbol is not IMethodSymbol nestedSymbol || nestedSymbol.ReducedFrom is null)
                    {
                        return;
                    }

                    if (!asParallelSymbols.Contains(nestedSymbol.ReducedFrom))
                    {
                        return;
                    }
                }
                else
                {
                    return;//true when it is not the last statement or second last
                }
            }

            if (invocation.Parent is not ForEachStatementSyntax parentForEach)
            {
                return;
            }

            if (!parentForEach.Expression.IsEquivalentTo(invocation) && //Last call is AsParallel
                (!(parentForEach.Expression is MemberAccessExpressionSyntax mem) || !s_knownCalls.Contains(mem.Name.Identifier.ValueText))) //OrToList and ToValue. Compare by string is safe as we compare by type earlier
            {
                return;
            }

            var diagnostic = invocation.CreateDiagnostic(DefaultRule, invocation);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
