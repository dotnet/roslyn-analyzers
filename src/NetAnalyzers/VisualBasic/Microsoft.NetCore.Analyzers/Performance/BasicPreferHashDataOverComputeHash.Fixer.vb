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
        Protected Overrides Function TryGetCodeFixer(computeHashNode As SyntaxNode, bufferArgNode As SyntaxNode, nodeToRemove As SyntaxNode, <NotNullWhen(True)> ByRef codeFixer As ApplyCodeFixAction) As Boolean
            Dim usingStatement = TryCast(nodeToRemove, UsingStatementSyntax)
            If usingStatement IsNot Nothing Then
                Dim usingBlock = TryCast(usingStatement.Parent, UsingBlockSyntax)
                If usingBlock IsNot Nothing Then
                    If usingStatement.Variables.Count = 1 Then
                        codeFixer = Sub(editor As DocumentEditor, hashDataInvoked As SyntaxNode)
                                        Dim newStatements = usingBlock.ReplaceNode(computeHashNode, hashDataInvoked).Statements.Select(Function(s) s.WithAdditionalAnnotations(Formatter.Annotation))
                                        editor.InsertBefore(usingBlock, newStatements)
                                        editor.RemoveNode(usingBlock)
                                    End Sub
                        Return True
                    End If
                End If
            End If

            Dim localDeclarationStatement = TryCast(nodeToRemove, LocalDeclarationStatementSyntax)
            Dim variableDeclaratorSyntax = TryCast(nodeToRemove, VariableDeclaratorSyntax)
            If localDeclarationStatement IsNot Nothing Or variableDeclaratorSyntax IsNot Nothing Then
                codeFixer = Sub(editor As DocumentEditor, hashDataInvoked As SyntaxNode)
                                editor.ReplaceNode(computeHashNode, hashDataInvoked)
                                editor.RemoveNode(nodeToRemove)
                            End Sub
                Return True
            End If
            Return False
        End Function

    End Class
End Namespace
