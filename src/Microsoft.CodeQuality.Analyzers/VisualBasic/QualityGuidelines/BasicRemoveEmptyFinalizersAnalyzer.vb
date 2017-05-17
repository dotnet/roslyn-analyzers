Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.QualityGuidelines

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class BasicRemoveEmptyFinalizersAnalyzer
    Inherits AbstractRemoveEmptyFinalizersAnalyzer

    Protected Overrides Function IsEmptyFinalizer(methodBody As SyntaxNode, analysisContext As SymbolAnalysisContext) As Boolean
        Dim destructorDeclaration = DirectCast(methodBody, MethodBlockSyntax)

    End Function

    Protected Overrides Sub Finalize()

    End Sub
End Class
