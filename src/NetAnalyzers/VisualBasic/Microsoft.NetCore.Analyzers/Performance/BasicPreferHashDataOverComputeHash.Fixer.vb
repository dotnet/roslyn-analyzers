' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Diagnostics.CodeAnalysis
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicPreferHashDataOverComputeHashFixer : Inherits PreferHashDataOverComputeHashFixer
        Protected Overrides Function TryGetCodeAction(document As Document,
                                                      hashTypeName As String,
                                                      computeHashNode As SyntaxNode,
                                                      computeType As PreferHashDataOverComputeHashAnalyzer.ComputeType,
                                                      argNodes As SyntaxNode(),
                                                      createHashNode As SyntaxNode,
                                                      disposeNodes As SyntaxNode(),
                                                      <NotNullWhen(True)> ByRef codeAction As HashDataCodeAction) As Boolean
            Dim usingStatement = TryCast(createHashNode, UsingStatementSyntax)
            If usingStatement IsNot Nothing Then
                Dim usingBlock = TryCast(usingStatement.Parent, UsingBlockSyntax)
                If usingBlock IsNot Nothing Then
                    If usingStatement.Variables.Count = 1 Then
                        codeAction = New BasicRemoveUsingBlockHashDataCodeAction(document, hashTypeName, computeHashNode, computeType, argNodes, usingBlock)
                        Return True
                    End If
                End If
            End If

            Dim localDeclarationStatement = TryCast(createHashNode, LocalDeclarationStatementSyntax)
            Dim variableDeclaratorSyntax = TryCast(createHashNode, VariableDeclaratorSyntax)
            If localDeclarationStatement IsNot Nothing Or variableDeclaratorSyntax IsNot Nothing Then
                codeAction = New RemoveNodeHashDataCodeAction(document, hashTypeName, computeHashNode, computeType, argNodes, createHashNode, disposeNodes)
                Return True
            End If
            Return False
        End Function

        Private NotInheritable Class BasicRemoveUsingBlockHashDataCodeAction : Inherits HashDataCodeAction
            Public Sub New(document As Document,
                           hashTypeName As String,
                           computeHashNode As SyntaxNode,
                           computeType As PreferHashDataOverComputeHashAnalyzer.ComputeType,
                           argNodes As SyntaxNode(),
                           usingBlockToRemove As UsingBlockSyntax)
                MyBase.New(document, hashTypeName, computeHashNode, computeType, argNodes)
                Me.UsingBlockToRemove = usingBlockToRemove
            End Sub

            Public ReadOnly Property UsingBlockToRemove As UsingBlockSyntax

            Protected Overrides Sub EditNodes(documentEditor As DocumentEditor, hashDataInvoked As SyntaxNode)
                Dim newStatements = UsingBlockToRemove.ReplaceNode(ComputeHashNode, hashDataInvoked).Statements.Select(Function(s) s.WithAdditionalAnnotations(Formatter.Annotation))
                documentEditor.InsertBefore(UsingBlockToRemove, newStatements)
                documentEditor.RemoveNode(UsingBlockToRemove)
            End Sub
        End Class
    End Class
End Namespace
