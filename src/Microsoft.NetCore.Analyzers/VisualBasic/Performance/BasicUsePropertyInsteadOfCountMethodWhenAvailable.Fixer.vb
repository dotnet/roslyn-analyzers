' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance

    ''' <summary>
    ''' CA1829: Use property instead of <see cref="Enumerable.Count(Of TSource)(IEnumerable(Of TSource))"/>, when available.
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUsePropertyInsteadOfCountMethodWhenAvailableFixer
        Inherits UsePropertyInsteadOfCountMethodWhenAvailableFixer

        ''' <summary>
        ''' Gets the expression from the specified <paramref name="node" /> where to replace the invocation of the
        ''' <see cref="Enumerable.Count(Of TSource)(IEnumerable(Of TSource))"/> method with a property invocation.
        ''' </summary>
        ''' <param name="node">The node to get a fixer for.</param>
        ''' <returns>The expression from the specified <paramref name="node" /> where to replace the invocation of the
        ''' <see cref="Enumerable.Count(Of TSource)(IEnumerable(Of TSource))"/> method with a property invocation
        ''' if found; <see langword="null" /> otherwise.</returns>
        ''' <exception cref="NotImplementedException"></exception>
        Protected Overrides Function GetExpression(node As SyntaxNode) As SyntaxNode

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Return DirectCast(invocationExpression.Expression, MemberAccessExpressionSyntax).Expression

            End If

            Return Nothing

        End Function
    End Class

End Namespace
