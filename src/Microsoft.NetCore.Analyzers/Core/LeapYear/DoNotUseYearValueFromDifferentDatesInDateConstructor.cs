// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetCore.Analyzers.LeapYear
{
    /// <summary>
    /// CA2261: Do not increment or decrement year parameter in DateTime constructor.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseYearValueFromDifferentDatesInDateConstructor : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2261";

        private static readonly LocalizableString _localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseDatePartOverflowPatternTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString _localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotUseDatePartOverflowPatternMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            RuleId,
            _localizableTitle,
            _localizableMessage,
            DiagnosticCategory.Usage,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: false,
            helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(semanticModelAnalysisContext =>
            {
                var semanticModel = new DateKindSemanticModel(semanticModelAnalysisContext.SemanticModel);
                var walker = new DateKindWalker(semanticModel);

                walker.Visit(semanticModelAnalysisContext.SemanticModel.SyntaxTree.GetRoot(semanticModelAnalysisContext.CancellationToken));
                foreach (DateKindContext dateKindContext in walker.Dates)
                {
                    // If there was a reason to ignore this LeapYear issue, skip over this context.
                    if (!dateKindContext.IgnoreDiagnostic)
                    {
                        // Diagnostic should not be raised if we can determine that
                        // the date won't be a possible leap year.
                        if (!dateKindContext.AreMonthOrDayValuesSafe())
                        {
                            // TODO leverage semantic analysis to try to determine if yearIncrementInitializer
                            // variable actually uses same or different data source as month argument.
                            DatePartOverflowAnalysisResult datePartOverflowResult = this.AnalyzeForDatePartOverflowIssue(dateKindContext);
                            if (datePartOverflowResult.IssueDetected)
                            {
                                semanticModelAnalysisContext.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Rule,
                                        dateKindContext.ObjectCreationExpression.GetLocation(),
                                        datePartOverflowResult.YearArgumentExpression,
                                        datePartOverflowResult.MonthArgumentExpression));
                            }
                        }
                    }
                }
            });
        }

        private DatePartOverflowAnalysisResult AnalyzeForDatePartOverflowIssue(DateKindContext context)
        {
            foreach (ExpressionSyntax yearArgumentExpression in context.YearArgumentExpressions)
            {
                foreach (ExpressionSyntax monthArgumentExpression in context.MonthArgumentExpressions)
                {
                    if (yearArgumentExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    && monthArgumentExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        // Both the year and month arguments are member access expressions.
                        if (yearArgumentExpression is MemberAccessExpressionSyntax yearArgumentMemberAccess
                            && monthArgumentExpression is MemberAccessExpressionSyntax monthArgumentMemberAccess
                            && !yearArgumentMemberAccess.Expression.IsEquivalentTo(monthArgumentMemberAccess.Expression, topLevel: true))
                        {
                            // We have detected the two expressions are accessing upon different identifiers.
                            return new DatePartOverflowAnalysisResult()
                            {
                                IssueDetected = true,
                                YearArgumentExpression = yearArgumentExpression,
                                MonthArgumentExpression = monthArgumentExpression,
                            };
                        }
                    }
                    else if ((yearArgumentExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                            && monthArgumentExpression.IsKind(SyntaxKind.IdentifierName))
                        || (yearArgumentExpression.IsKind(SyntaxKind.IdentifierName)
                            && monthArgumentExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression)))
                    {
                        // One of the year or month arguments is a member access and the other is an identifier.
                        return new DatePartOverflowAnalysisResult()
                        {
                            IssueDetected = true,
                            YearArgumentExpression = yearArgumentExpression,
                            MonthArgumentExpression = monthArgumentExpression,
                        };
                    }
                }
            }

            return new DatePartOverflowAnalysisResult()
            {
                IssueDetected = false
            };
        }

        private sealed class DatePartOverflowAnalysisResult
        {
            public bool IssueDetected { get; set; }

            [DisallowNull]
            public ExpressionSyntax? YearArgumentExpression { get; set; }

            [DisallowNull]
            public ExpressionSyntax? MonthArgumentExpression { get; set; }
        }
    }
}
