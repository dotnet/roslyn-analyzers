' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    ''' <summary>
    ''' CA1801: Review unused parameters
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicReviewUnusedParametersFixer
        Inherits ReviewUnusedParametersFixer

        Public Sub New()
            MyBase.New(New BasicNodesProvider())
        End Sub

        Private NotInheritable Class BasicNodesProvider
            Inherits NodesProvider

            Protected Overrides Function GetOperationNode(node As SyntaxNode) As SyntaxNode
                If node.Kind() = SyntaxKind.SimpleMemberAccessExpression Then
                    Return node.Parent
                End If

                Return node
            End Function

            Public Overrides Sub RemoveNode(editor As DocumentEditor, node As SyntaxNode)
                editor.RemoveNode(node)
            End Sub

            Protected Overrides Function GetParameterNode(node As SyntaxNode) As SyntaxNode
                Return node.Parent
            End Function
        End Class
    End Class
End Namespace
