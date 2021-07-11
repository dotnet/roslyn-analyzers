' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        Protected Overrides Function TryGetCodeAction(document As Document, hashTypeName As String, bufferArgNode As SyntaxNode, computeHashNode As SyntaxNode, nodeToRemove As SyntaxNode, <NotNullWhen(True)> ByRef codeAction As HashDataCodeAction) As Boolean
            Dim usingStatement = TryCast(nodeToRemove, UsingStatementSyntax)
            If usingStatement IsNot Nothing Then
                Dim usingBlock = TryCast(usingStatement.Parent, UsingBlockSyntax)
                If usingBlock IsNot Nothing Then
                    If usingStatement.Variables.Count = 1 Then
                        codeAction = New BasicRemoveUsingBlockHashDataCodeAction(document, hashTypeName, bufferArgNode, computeHashNode, usingBlock)
                        Return True
                    End If
                End If
            End If

            Dim localDeclarationStatement = TryCast(nodeToRemove, LocalDeclarationStatementSyntax)
            Dim variableDeclaratorSyntax = TryCast(nodeToRemove, VariableDeclaratorSyntax)
            If localDeclarationStatement IsNot Nothing Or variableDeclaratorSyntax IsNot Nothing Then
                codeAction = New RemoveNodeHashDataCodeAction(document, hashTypeName, bufferArgNode, computeHashNode, nodeToRemove)
                Return True
            End If
            Return False
        End Function

        Private NotInheritable Class BasicRemoveUsingBlockHashDataCodeAction : Inherits HashDataCodeAction
            Public Sub New(document As Document, hashTypeName As String, bufferArgNode As SyntaxNode, computeHashNode As SyntaxNode, usingBlockToRemove As UsingBlockSyntax)
                MyBase.New(document, hashTypeName, bufferArgNode, computeHashNode)
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
