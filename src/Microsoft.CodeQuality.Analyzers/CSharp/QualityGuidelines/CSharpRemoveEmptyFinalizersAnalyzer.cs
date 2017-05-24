using System;
using System.Linq;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.Analyzers.QualityGuidelines;

namespace Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CSharpRemoveEmptyFinalizersAnalyzer : AbstractRemoveEmptyFinalizersAnalyzer
    {
        protected override bool IsEmptyFinalizer(SyntaxNode methodBody, CodeBlockAnalysisContext analysisContext)
        {
            var destructorDeclaration = (DestructorDeclarationSyntax)methodBody;

            if ((destructorDeclaration?.Body?.Statements.Count ?? 0) == 0)
            {
                return true;
            }

            if (destructorDeclaration.Body.Statements.Count == 1)
            {
                var body = destructorDeclaration.Body.Statements[0];

                if (body.Kind() == CodeAnalysis.CSharp.SyntaxKind.ThrowStatement)
                {
                    return true;
                }

                if (body.Kind() == CodeAnalysis.CSharp.SyntaxKind.ExpressionStatement &&
                    body is ExpressionStatementSyntax expr &&
                    expr.Expression.Kind() == CodeAnalysis.CSharp.SyntaxKind.InvocationExpression)
                {
                    var invocation = (InvocationExpressionSyntax)expr.Expression;
                    var invocationSymbol = (IMethodSymbol)analysisContext.SemanticModel.GetSymbolInfo(invocation).Symbol;
                    var conditionalAttributeSymbol = WellKnownTypes.ConditionalAttribute(analysisContext.SemanticModel.Compilation);
                    return InvocationIsConditional(invocationSymbol, conditionalAttributeSymbol);
                }
            }

            return false;
        }
    }
}
