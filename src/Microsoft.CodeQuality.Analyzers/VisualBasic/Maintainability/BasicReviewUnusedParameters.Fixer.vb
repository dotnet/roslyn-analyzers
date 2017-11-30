' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
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

            Public Overrides Function GetParameterNodeToRemove(editor As DocumentEditor, node As SyntaxNode, name As String) As SyntaxNode
                Throw New NotImplementedException()
            End Function

            Public Overrides Sub RemoveAllUnusedLocalDeclarations(nodesToRemove As HashSet(Of SyntaxNode))
                Throw New NotImplementedException()
            End Sub

            Public Overrides Sub RemoveNode(editor As DocumentEditor, node As SyntaxNode)
                editor.RemoveNode(node)
            End Sub
        End Class
    End Class
End Namespace
