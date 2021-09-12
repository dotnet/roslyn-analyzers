' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Usage

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Usage
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCallOrderByMultipleTimesFixer
        Inherits DoNotCallOrderByMultipleTimesFixer

        Protected Overrides Function ReplaceOrderByWithThenBy(document As Document, root As SyntaxNode, node As SyntaxNode) As Document
            Dim invocation = TryCast(node, InvocationExpressionSyntax)
            Dim memberAccessExpresion = TryCast(invocation?.Expression, MemberAccessExpressionSyntax)

            If invocation Is Nothing OrElse memberAccessExpresion Is Nothing Then
                Return document
            End If

            Dim newMember As String

            Select Case memberAccessExpresion.Name.ToString()
                Case "OrderBy"
                    newMember = "ThenBy"
                Case "OrderByDescending"
                    newMember = "ThenByDescending"
                Case Else
                    Return document ' should we throw NotSupported at this point?
            End Select

            Dim generatedSyntax = SyntaxGenerator.GetGenerator(document).IdentifierName(newMember)

            Dim newRoot = root.ReplaceNode(memberAccessExpresion.Name, generatedSyntax)

            Return document.WithSyntaxRoot(newRoot)
        End Function
    End Class
End Namespace
