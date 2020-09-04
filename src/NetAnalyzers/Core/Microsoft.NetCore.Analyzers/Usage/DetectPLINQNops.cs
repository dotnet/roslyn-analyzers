// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DetectPLINQNops : DiagnosticAnalyzer
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
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (!memberAccess.Name.Identifier.ValueText.Equals("AsParallel", StringComparison.Ordinal))
                {
                    if (!(s_knownCalls.Contains(memberAccess.Name.Identifier.ValueText)
                && memberAccess.Expression is InvocationExpressionSyntax nestedInvocation
                && nestedInvocation.Expression is MemberAccessExpressionSyntax possibleAsParallelCall
                && possibleAsParallelCall.Name.Identifier.ValueText.Equals("AsParallel", StringComparison.Ordinal)))
                    {
                        return;
                    }
                }//true when it is the last statement or second last
            }
            if (invocation.Parent is ForEachStatementSyntax parentForEach)
            {
                if (parentForEach.Expression.IsEquivalentTo(invocation) || //Last call is AsParallel
                    parentForEach.Expression is MemberAccessExpressionSyntax mem && s_knownCalls.Contains(mem.Name.Identifier.ValueText) //OrToList and ToValue
                    )
                {
                    var diagnostic = Diagnostic.Create(DefaultRule, invocation.GetLocation(), invocation);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
