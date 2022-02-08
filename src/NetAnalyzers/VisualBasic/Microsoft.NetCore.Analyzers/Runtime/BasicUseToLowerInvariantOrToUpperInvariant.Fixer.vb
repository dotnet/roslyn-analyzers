Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public Class BasicUseToLowerInvariantOrToUpperInvariantFixer
        Inherits UseToLowerInvariantOrToUpperInvariantFixerBase

        Protected Overrides Function ShouldFix(node As SyntaxNode) As Boolean
            Return node.IsKind(SyntaxKind.IdentifierName) AndAlso
                Nullable.Equals(node.Parent?.IsKind(SyntaxKind.SimpleMemberAccessExpression), True)
        End Function

        Protected Overrides Function FixInvocationAsync(document As Document, syntaxGenerator As SyntaxGenerator, root As SyntaxNode, node As SyntaxNode) As Task(Of Document)
            If ShouldFix(node) Then
                Dim memberAccess = DirectCast(node.Parent, MemberAccessExpressionSyntax)
                Dim replacementMethodName = GetReplacementMethodName(memberAccess.Name.Identifier.Text)
                Dim newMemberAccess = memberAccess.WithName(DirectCast(syntaxGenerator.IdentifierName(replacementMethodName), SimpleNameSyntax)).WithAdditionalAnnotations(Formatter.Annotation)
                Dim newRoot = root.ReplaceNode(memberAccess, newMemberAccess)
                Return Task.FromResult(document.WithSyntaxRoot(newRoot))
            End If
            Return Task.FromResult(document)
        End Function
    End Class
End Namespace
