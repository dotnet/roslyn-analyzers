' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

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
        Protected Overrides Function TryGetThreadVolatileReadWriteArguments(invocation As SyntaxNode, methodName As String, <Out> ByRef arguments As IEnumerable(Of SyntaxNode)) As Boolean
            arguments = Nothing
            Dim invocationExpression = TryCast(invocation, InvocationExpressionSyntax)
            Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)
            If memberAccessExpression IsNot Nothing AndAlso memberAccessExpression.Name.Identifier.Text = methodName
                arguments = invocationExpression.ArgumentList.Arguments.Select(Function(a) DirectCast(a, SimpleArgumentSyntax).WithNameColonEquals(Nothing))

                Return True
            End If

            Return False
        End Function
    End Class

End Namespace