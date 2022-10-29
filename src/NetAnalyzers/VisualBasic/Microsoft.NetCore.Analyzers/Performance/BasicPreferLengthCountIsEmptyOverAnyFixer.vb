Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicPreferLengthCountIsEmptyOverAnyFixer
        Inherits PreferLengthCountIsEmptyOverAnyFixer

        Protected Overrides Function ReplaceAnyWithIsEmpty(root As SyntaxNode, node As SyntaxNode) As SyntaxNode
            Dim invocation = TryCast(node, InvocationExpressionSyntax)
            Dim memberAccess = TryCast(invocation?.Expression, MemberAccessExpressionSyntax)
            If invocation Is Nothing Or memberAccess Is Nothing Then
                Return Nothing
            End If

            Dim newMemberAccess = memberAccess.WithName(
                SyntaxFactory.IdentifierName(PreferLengthCountIsEmptyOverAnyAnalyzer.IsEmptyText)
            )
            Dim unaryParent = TryCast(invocation.Parent, UnaryExpressionSyntax)
            If unaryParent IsNot Nothing And unaryParent.IsKind(SyntaxKind.NotExpression) Then
                Return root.ReplaceNode(unaryParent, newMemberAccess.WithTriviaFrom(unaryParent))
            End If

            Dim negatedExpression = SyntaxFactory.UnaryExpression(
                SyntaxKind.NotExpression,
                SyntaxFactory.Token(SyntaxKind.NotKeyword),
                newMemberAccess
            )

            Return root.ReplaceNode(invocation, negatedExpression.WithTriviaFrom(invocation))
        End Function

        Protected Overrides Function ReplaceAnyWithLength(root As SyntaxNode, node As SyntaxNode) As SyntaxNode
            Dim invocation = TryCast(node, InvocationExpressionSyntax)
            Dim memberAccess = TryCast(invocation?.Expression, MemberAccessExpressionSyntax)
            If invocation Is Nothing Or memberAccess Is Nothing Then
                Return Nothing
            End If

            Const lengthMemberName As String = PreferLengthCountIsEmptyOverAnyAnalyzer.LengthText
            Dim unaryParent = TryCast(invocation.Parent, UnaryExpressionSyntax)
            If unaryParent IsNot Nothing And unaryParent.IsKind(SyntaxKind.NotExpression) Then
                Dim binaryExpression = GetBinaryExpression(memberAccess, lengthMemberName, SyntaxKind.EqualsExpression)

                Return root.ReplaceNode(unaryParent, binaryExpression.WithTriviaFrom(unaryParent))
            End If

            Return root.ReplaceNode(invocation, GetBinaryExpression(memberAccess, lengthMemberName, SyntaxKind.NotEqualsExpression).WithTriviaFrom(invocation))
        End Function

        Protected Overrides Function ReplaceAnyWithCount(root As SyntaxNode, node As SyntaxNode) As SyntaxNode
            Dim invocation = TryCast(node, InvocationExpressionSyntax)
            Dim memberAccess = TryCast(invocation?.Expression, MemberAccessExpressionSyntax)
            If invocation Is Nothing Or memberAccess Is Nothing Then
                Return Nothing
            End If

            Const countMemberName As String = PreferLengthCountIsEmptyOverAnyAnalyzer.CountText
            Dim unaryParent = TryCast(invocation.Parent, UnaryExpressionSyntax)
            If unaryParent IsNot Nothing And unaryParent.IsKind(SyntaxKind.NotExpression) Then
                Dim binaryExpression = GetBinaryExpression(memberAccess, countMemberName, SyntaxKind.EqualsExpression)

                Return root.ReplaceNode(unaryParent, binaryExpression.WithTriviaFrom(unaryParent))
            End If

            Return root.ReplaceNode(invocation, GetBinaryExpression(memberAccess, countMemberName, SyntaxKind.NotEqualsExpression).WithTriviaFrom(invocation))
        End Function

        Private Shared Function GetBinaryExpression(originalMemberAccess As MemberAccessExpressionSyntax, member As String, expressionKind As SyntaxKind) As BinaryExpressionSyntax
            Dim tokenKind = If(expressionKind = SyntaxKind.EqualsExpression, SyntaxKind.EqualsToken, SyntaxKind.LessThanGreaterThanToken)
            return SyntaxFactory.BinaryExpression(
                expressionKind,
                originalMemberAccess.WithName(
                    SyntaxFactory.IdentifierName(member)
                    ),
                SyntaxFactory.Token(tokenKind),
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    SyntaxFactory.Literal(0)
                    )
                )
        End Function
    End Class
End Namespace