Imports System.Composition
Imports System.Runtime.InteropServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Usage

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Usage

    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUseVolatileReadWriteFixer
        Inherits UseVolatileReadWriteFixer
        Protected Overrides Function TryGetThreadVolatileReadWriteMemberAccess(invocation As SyntaxNode, methodName As String, <Out> ByRef memberAccess As SyntaxNode) As Boolean
            memberAccess = Nothing
            Dim invocationExpression = TryCast(invocation, InvocationExpressionSyntax)
            Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)
            If memberAccessExpression IsNot Nothing AndAlso memberAccessExpression.Name.Identifier.Text = methodName
                memberAccess = memberAccessExpression

                Return True
            End If

            Return False
        End Function
    End Class

End Namespace