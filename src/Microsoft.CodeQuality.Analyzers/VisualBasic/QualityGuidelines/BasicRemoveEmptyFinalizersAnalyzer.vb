Imports Analyzer.Utilities
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.QualityGuidelines

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines

    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class BasicRemoveEmptyFinalizersAnalyzer
        Inherits AbstractRemoveEmptyFinalizersAnalyzer

        Protected Overrides Function IsEmptyFinalizer(methodBody As SyntaxNode, analysisContext As SymbolAnalysisContext) As Boolean
            Dim destructorStatement = DirectCast(methodBody, MethodStatementSyntax)
            Dim destructorBlock = DirectCast(destructorStatement.Parent, MethodBlockSyntax)

            If (destructorBlock.Statements.Count = 0) Then
                Return True
            ElseIf (destructorBlock.Statements.Count = 1) Then
                If (destructorBlock.Statements(0).Kind() = CodeAnalysis.VisualBasic.SyntaxKind.ThrowStatement) Then
                    Return True
                End If

                If (destructorBlock.Statements(0).Kind() = CodeAnalysis.VisualBasic.SyntaxKind.ExpressionStatement) Then
                        Dim destructorExpression = DirectCast(destructorBlock.Statements(0), ExpressionStatementSyntax)
                        If (destructorExpression.Expression.Kind() = CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression) Then
                            Dim invocationExpression = DirectCast(destructorExpression.Expression, InvocationExpressionSyntax)
                            Dim semanticModel = analysisContext.Compilation.GetSemanticModel(invocationExpression.SyntaxTree)
                            Dim invocationSymbol = DirectCast(semanticModel.GetSymbolInfo(invocationExpression).Symbol, IMethodSymbol)
                            Dim conditionalAttributeSymbol = WellKnownTypes.ConditionalAttribute(analysisContext.Compilation)
                            Return InvocationIsConditional(invocationSymbol, conditionalAttributeSymbol)
                        End If
                    End If
                End If
                Return False
        End Function
    End Class
End Namespace
