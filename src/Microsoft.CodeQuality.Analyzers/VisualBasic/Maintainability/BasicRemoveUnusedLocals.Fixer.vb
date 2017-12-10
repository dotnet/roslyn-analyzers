' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Collections.Generic
Imports System.Linq
Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    ''' <summary>
    ''' CA1804: Remove unused locals
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=BasicRemoveUnusedLocalsFixer.RuleId), [Shared]>
    Public NotInheritable Class BasicRemoveUnusedLocalsFixer
        Inherits RemoveUnusedLocalsFixer

        Public Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
            Get
                Return ImmutableArray.Create(BasicRemoveUnusedLocalsAnalyzer.RuleId)
            End Get
        End Property

        Public Sub New()
            MyBase.New(New BasicNodesProvider())
        End Sub

        Private Class BasicNodesProvider
            Inherits NodesProvider

            Public Overrides Function GetNodeToRemoveOrReplace(node As SyntaxNode) As SyntaxNode
                node = node.Parent
                If (node.Kind() = SyntaxKind.SimpleAssignmentStatement) Then
                    Return node
                End If
                Return Nothing
            End Function

            Public Overrides Sub RemoveAllUnusedLocalDeclarations(nodesToRemove As HashSet(Of SyntaxNode))
                Dim candidateLocalDeclarationsToRemove = New HashSet(Of LocalDeclarationStatementSyntax)()
                For Each variableDeclarator In nodesToRemove.OfType(Of VariableDeclaratorSyntax)()
                    Dim localDeclaration = DirectCast(variableDeclarator.Parent.Parent, LocalDeclarationStatementSyntax)
                    candidateLocalDeclarationsToRemove.Add(localDeclaration)
                Next

                For Each candidate In candidateLocalDeclarationsToRemove
                    Dim hasUsedLocal = False
                    For Each variable In candidate.Declarators
                        If Not nodesToRemove.Contains(variable) Then
                            hasUsedLocal = True
                            Exit For
                        End If
                    Next

                    If Not hasUsedLocal Then
                        nodesToRemove.Add(candidate)
                        For Each variable In candidate.Declarators
                            nodesToRemove.Remove(variable)
                        Next
                    End If
                Next
            End Sub

            Public Overrides Sub RemoveNode(editor As DocumentEditor, node As SyntaxNode)
                editor.RemoveNode(node)
            End Sub
        End Class
    End Class
End Namespace
