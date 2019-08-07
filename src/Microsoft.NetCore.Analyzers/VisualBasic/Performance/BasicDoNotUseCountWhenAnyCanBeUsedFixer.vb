' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    ''' <summary>
    ''' CA1827: Do not use Count()/LongCount() when Any() can be used.
    ''' CA1828: Do not use CountAsync()/LongCountAsync() when AnyAsync() can be used.
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotUseCountWhenAnyCanBeUsedFixer
        Inherits DoNotUseCountWhenAnyCanBeUsedFixer

        ''' <summary>
        ''' Tries the get a fixer the specified <paramref name="node" />.
        ''' </summary>
        ''' <param name="node">The node to get a fixer for.</param>
        ''' <param name="isAsync"><see langword="true" /> if it's an asynchronous method; <see langword="false" /> otherwise.</param>
        ''' <param name="expression">If this method returns <see langword="true" />, contains the expression to be used to invoke <c>Any</c>.</param>
        ''' <param name="arguments">If this method returns <see langword="true" />, contains the arguments from <c>Any</c> to be used on <c>Count</c>.</param>
        ''' <param name="negate">If this method returns <see langword="true" />, indicates whether to negate the expression.</param>
        ''' <returns><see langword="true" /> if a fixer was found., <see langword="false" /> otherwise.</returns>
        Protected Overrides Function TryGetFixer(node As SyntaxNode, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean) As Boolean

            If node.IsKind(SyntaxKind.InvocationExpression) Then

                GetFixerForEqualsMethod(DirectCast(node, InvocationExpressionSyntax), isAsync, expression, arguments)
                negate = True
                Return True

            ElseIf (node.IsKind(SyntaxKind.EqualsExpression)) Then

                GetFixerForEqualityExpression(DirectCast(node, BinaryExpressionSyntax), isAsync, expression, arguments)
                negate = True
                Return True

            ElseIf (node.IsKind(SyntaxKind.NotEqualsExpression)) Then

                GetFixerForEqualityExpression(DirectCast(node, BinaryExpressionSyntax), isAsync, expression, arguments)
                negate = False
                Return True

            ElseIf (node.IsKind(SyntaxKind.LessThanExpression)) Then

                GetFixerForLessThanExpression(DirectCast(node, BinaryExpressionSyntax), isAsync, expression, arguments, negate)
                Return True

            ElseIf (node.IsKind(SyntaxKind.LessThanOrEqualExpression)) Then

                GetFixerForLessThanOrEqualExpression(DirectCast(node, BinaryExpressionSyntax), isAsync, expression, arguments, negate)
                Return True

            ElseIf (node.IsKind(SyntaxKind.GreaterThanExpression)) Then

                GetFixerForGreaterThanExpression(DirectCast(node, BinaryExpressionSyntax), isAsync, expression, arguments, negate)
                Return True

            ElseIf (node.IsKind(SyntaxKind.GreaterThanOrEqualExpression)) Then

                GetFixerForGreaterThanOrEqualExpression(DirectCast(node, BinaryExpressionSyntax), isAsync, expression, arguments, negate)
                Return True

            End If

            expression = Nothing
            arguments = Nothing
            negate = Nothing
            Return False

        End Function

        Private Shared Sub GetFixerForEqualsMethod(equalsMethodInvocation As InvocationExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode))

            Dim argument = equalsMethodInvocation.ArgumentList.Arguments.Item(0).GetExpression()

            Dim countInvocation = If(TypeOf argument Is LiteralExpressionSyntax,
                DirectCast(equalsMethodInvocation.Expression, MemberAccessExpressionSyntax).Expression,
                argument)
            GetExpressionAndInvocationArguments(
                sourceExpression:=countInvocation,
                isAsync:=isAsync,
                expression:=expression,
                arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForEqualityExpression(binaryExpression As BinaryExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode))

            GetExpressionAndInvocationArguments(
                sourceExpression:=If(TypeOf binaryExpression.Left Is LiteralExpressionSyntax, binaryExpression.Right, binaryExpression.Left),
                isAsync:=isAsync,
                expression:=expression,
                arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForLessThanOrEqualExpression(binaryExpression As BinaryExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            negate = TypeOf binaryExpression.Right Is LiteralExpressionSyntax
            GetExpressionAndInvocationArguments(
                 sourceExpression:=If(Not (negate), binaryExpression.Right, binaryExpression.Left),
                 isAsync:=isAsync,
                expression:=expression,
                 arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForLessThanExpression(binaryExpression As BinaryExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            If TypeOf binaryExpression.Left Is LiteralExpressionSyntax Then

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Right,
                isAsync:=isAsync,
                    literalExpression:=DirectCast(binaryExpression.Left, LiteralExpressionSyntax),
                    value:=0,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            Else

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Left,
                isAsync:=isAsync,
                    literalExpression:=DirectCast(binaryExpression.Right, LiteralExpressionSyntax),
                    value:=1,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            End If

        End Sub

        Private Shared Sub GetFixerForGreaterThanExpression(binaryExpression As BinaryExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            negate = TypeOf binaryExpression.Left Is LiteralExpressionSyntax
            GetExpressionAndInvocationArguments(
                 sourceExpression:=If(negate, binaryExpression.Right, binaryExpression.Left),
                isAsync:=isAsync,
                 expression:=expression,
                 arguments:=arguments)

        End Sub

        Private Shared Sub GetFixerForGreaterThanOrEqualExpression(binaryExpression As BinaryExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            If TypeOf binaryExpression.Left Is LiteralExpressionSyntax Then

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Right,
                isAsync:=isAsync,
                    literalExpression:=DirectCast(binaryExpression.Left, LiteralExpressionSyntax),
                    value:=1,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            Else

                GetFixerForBinaryExpression(
                    sourceExpression:=binaryExpression.Left,
                isAsync:=isAsync,
                    literalExpression:=DirectCast(binaryExpression.Right, LiteralExpressionSyntax),
                    value:=0,
                    expression:=expression,
                    arguments:=arguments,
                    negate:=negate)

            End If

        End Sub

        Private Shared Sub GetFixerForBinaryExpression(sourceExpression As ExpressionSyntax, isAsync As Boolean, literalExpression As LiteralExpressionSyntax, value As Integer, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode), ByRef negate As Boolean)

            GetExpressionAndInvocationArguments(sourceExpression, isAsync, expression, arguments)
            negate = DirectCast(literalExpression.Token.Value, Integer) = value

        End Sub

        Private Shared Sub GetExpressionAndInvocationArguments(sourceExpression As ExpressionSyntax, isAsync As Boolean, ByRef expression As SyntaxNode, ByRef arguments As IEnumerable(Of SyntaxNode))

            Dim parenthesizedExpression = TryCast(sourceExpression, ParenthesizedExpressionSyntax)

            While Not parenthesizedExpression Is Nothing

                sourceExpression = parenthesizedExpression.Expression
                parenthesizedExpression = TryCast(sourceExpression, ParenthesizedExpressionSyntax)

            End While

            Dim invocationExpression As InvocationExpressionSyntax = Nothing

            If isAsync Then

                Dim awaitExpressionSyntax = TryCast(sourceExpression, AwaitExpressionSyntax)

                If Not awaitExpressionSyntax Is Nothing Then

                    invocationExpression = TryCast(awaitExpressionSyntax.Expression, InvocationExpressionSyntax)

                End If

            Else

                invocationExpression = TryCast(sourceExpression, InvocationExpressionSyntax)

            End If

            If invocationExpression Is Nothing Then

                expression = Nothing
                arguments = Nothing
                Return

            End If

            expression = DirectCast(invocationExpression.Expression, MemberAccessExpressionSyntax).Expression
            arguments = invocationExpression.ArgumentList.ChildNodes()

        End Sub

    End Class

End Namespace
