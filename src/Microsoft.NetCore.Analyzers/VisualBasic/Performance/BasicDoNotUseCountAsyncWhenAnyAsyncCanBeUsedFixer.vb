' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    ''' <summary>
    ''' CA1827: Do not use Count() when Any() can be used.
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotUseCountAsyncWhenAnyAsyncCanBeUsedFixer
        Inherits DoNotUseCountWhenAnyCanBeUsedFixer

        ''' <summary>
        ''' Tries the get a fixer the specified <paramref name="node" />.
        ''' </summary>
        ''' <param name="node">The node to get a fixer for.</param>
        ''' <param name="expression">If this method returns <see langword="true" />, contains the expression to be used to invoke <c>Any</c>.</param>
        ''' <param name="arguments">If this method returns <see langword="true" />, contains the arguments from <c>Any</c> to be used on <c>Count</c>.</param>
        ''' <param name="negate">If this method returns <see langword="true" />, indicates whether to negate the expression.</param>
        ''' <returns><see langword="true" /> if a fixer was found., <see langword="false" /> otherwise.</returns>
        Protected Overrides Function TryGetFixer(node As SyntaxNode, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean) As Boolean

            If node.IsKind(SyntaxKind.InvocationExpression) Then

                GetFixerForEqualsMethod(DirectCast(node, InvocationExpressionSyntax), expression, arguments)
                negate = True
                Return True

            ElseIf (node.IsKind(SyntaxKind.EqualsExpression)) Then

                GetFixerForEqualityExpression(DirectCast(node, BinaryExpressionSyntax), expression, arguments)
                negate = True
                Return True

            ElseIf (node.IsKind(SyntaxKind.NotEqualsExpression)) Then

                GetFixerForEqualityExpression(DirectCast(node, BinaryExpressionSyntax), expression, arguments)
                negate = False
                Return True

            ElseIf (node.IsKind(SyntaxKind.LessThanExpression)) Then

                GetFixerForLessThanExpression(DirectCast(node, BinaryExpressionSyntax), expression, arguments, negate)
                Return True

            ElseIf (node.IsKind(SyntaxKind.LessThanOrEqualExpression)) Then

                GetFixerForLessThanOrEqualExpression(DirectCast(node, BinaryExpressionSyntax), expression, arguments, negate)
                Return True

            ElseIf (node.IsKind(SyntaxKind.GreaterThanExpression)) Then

                GetFixerForGreaterThanExpression(DirectCast(node, BinaryExpressionSyntax), expression, arguments, negate)
                Return True

            ElseIf (node.IsKind(SyntaxKind.GreaterThanOrEqualExpression)) Then

                GetFixerForGreaterThanOrEqualExpression(DirectCast(node, BinaryExpressionSyntax), expression, arguments, negate)
                Return True

            End If

            expression = Nothing
            arguments = Nothing
            negate = Nothing
            Return False

        End Function

        Private Shared Sub GetFixerForEqualsMethod(equalsMethodInvocation As InvocationExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode))

            Dim argument = equalsMethodInvocation.ArgumentList.Arguments.Item(0).GetExpression()

            Dim countInvocation = If(TypeOf argument Is LiteralExpressionSyntax,
                DirectCast(DirectCast(equalsMethodInvocation.Expression, MemberAccessExpressionSyntax).Expression, ParenthesizedExpressionSyntax),
                argument)
            GetExpressionAndInvocationArguments(
                sourceExpression:=countInvocation,
                expression:=expression,
                arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForEqualityExpression(binaryExpression As BinaryExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode))

            GetExpressionAndInvocationArguments(
                sourceExpression:=If(TypeOf binaryExpression.Left Is LiteralExpressionSyntax, binaryExpression.Right, binaryExpression.Left),
                expression:=expression,
                arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForLessThanOrEqualExpression(binaryExpression As BinaryExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            negate = TypeOf binaryExpression.Right Is LiteralExpressionSyntax
            GetExpressionAndInvocationArguments(
                 sourceExpression:=If(Not (negate), binaryExpression.Right, binaryExpression.Left),
                 expression:=expression,
                 arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForLessThanExpression(binaryExpression As BinaryExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            If TypeOf binaryExpression.Left Is LiteralExpressionSyntax Then

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Right,
                    literalExpression:=DirectCast(binaryExpression.Left, LiteralExpressionSyntax),
                    value:=0,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            Else

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Left,
                    literalExpression:=DirectCast(binaryExpression.Right, LiteralExpressionSyntax),
                    value:=1,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            End If

        End Sub

        Private Shared Sub GetFixerForGreaterThanExpression(binaryExpression As BinaryExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            negate = TypeOf binaryExpression.Left Is LiteralExpressionSyntax
            GetExpressionAndInvocationArguments(
                 sourceExpression:=If(negate, binaryExpression.Right, binaryExpression.Left),
                 expression:=expression,
                 arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForGreaterThanOrEqualExpression(binaryExpression As BinaryExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            If TypeOf binaryExpression.Left Is LiteralExpressionSyntax Then

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Right,
                    literalExpression:=DirectCast(binaryExpression.Left, LiteralExpressionSyntax),
                    value:=1,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            Else

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Left,
                    literalExpression:=DirectCast(binaryExpression.Right, LiteralExpressionSyntax),
                    value:=0,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            End If

        End Sub

        Private Shared Sub GetFixerForBinaryExpression(sourceExpression As ExpressionSyntax, literalExpression As LiteralExpressionSyntax, value As Integer, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            GetExpressionAndInvocationArguments(sourceExpression, expression, arguments)
            negate = DirectCast(literalExpression.Token.Value, Integer) = value

        End Sub

        Private Shared Sub GetExpressionAndInvocationArguments(sourceExpression As ExpressionSyntax, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode))

            Dim awaitExpression = TryCast(sourceExpression, AwaitExpressionSyntax)

            If Not awaitExpression Is Nothing Then

                GetExpressionAndInvocationArguments(awaitExpression.Expression, expression, arguments)

            Else

                Dim parenthesizedExpression = TryCast(sourceExpression, ParenthesizedExpressionSyntax)

                If Not parenthesizedExpression Is Nothing Then

                    GetExpressionAndInvocationArguments(parenthesizedExpression.Expression, expression, arguments)

                Else
                    Dim invocationExpression = DirectCast(sourceExpression, InvocationExpressionSyntax)
                    expression = DirectCast(invocationExpression.Expression, MemberAccessExpressionSyntax).Expression
                    arguments = invocationExpression.ArgumentList.ChildNodes()

                End If

            End If

        End Sub

    End Class

End Namespace
