Imports System.Collections.Immutable
Imports Analyzer.Utilities.Extensions
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicMakeTypesInternal
        Inherits MakeTypesInternal(Of SyntaxKind)

        Protected Overrides ReadOnly Property TypeKinds As ImmutableArray(Of SyntaxKind) = ImmutableArray.Create(SyntaxKind.ClassStatement, SyntaxKind.StructureStatement, SyntaxKind.InterfaceStatement)
        Protected Overrides ReadOnly Property EnumKind As SyntaxKind = SyntaxKind.EnumStatement

        Protected Overrides Sub AnalyzeTypeDeclaration(context As SyntaxNodeAnalysisContext)
            Dim type = DirectCast(context.Node, TypeStatementSyntax)
            If type.Modifiers.Any(SyntaxKind.PublicKeyword) Then
                context.ReportDiagnostic(type.Identifier.CreateDiagnostic(Rule))
            End If
        End Sub

        Protected Overrides Sub AnalyzeEnumDeclaration(context As SyntaxNodeAnalysisContext)
            Dim enumStatement = DirectCast(context.Node, EnumStatementSyntax)
            If enumStatement.Modifiers.Any(SyntaxKind.PublicKeyword) Then
                context.ReportDiagnostic(enumStatement.Identifier.CreateDiagnostic(Rule))
            End If
        End Sub
    End Class
End Namespace