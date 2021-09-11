' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Diagnostics.CodeAnalysis
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicPreferHashDataOverComputeHashFixer : Inherits PreferHashDataOverComputeHashFixer
        Private Shared ReadOnly s_fixAllProvider As New BasicPreferHashDataOverComputeHashFixAllProvider()
        Public Overrides Function GetFixAllProvider() As FixAllProvider
            Return s_fixAllProvider
        End Function

        Protected Overrides Function TryGetCodeAction(document As Document,
                                                      computeHashNode As SyntaxNode,
                                                      hashDataNode As SyntaxNode,
                                                      createHashNode As SyntaxNode,
                                                      disposeNodes As SyntaxNode(),
                                                      <NotNullWhen(True)> ByRef codeAction As HashDataCodeAction) As Boolean
            Dim usingStatement = TryCast(createHashNode.Parent, UsingStatementSyntax)
            If usingStatement IsNot Nothing Then
                Dim usingBlock = TryCast(usingStatement.Parent, UsingBlockSyntax)
                If usingBlock IsNot Nothing Then
                    If usingStatement.Variables.Count = 1 Then
                        codeAction = New BasicRemoveUsingBlockHashDataCodeAction(document, computeHashNode, hashDataNode, usingBlock)
                        Return True
                    Else
                        codeAction = New RemoveNodeHashDataCodeAction(document, computeHashNode, hashDataNode, createHashNode, disposeNodes)
                        Return True
                    End If
                End If
            End If

            Dim localDeclarationStatement = TryCast(createHashNode.Parent, LocalDeclarationStatementSyntax)
            If localDeclarationStatement IsNot Nothing Then
                codeAction = New RemoveNodeHashDataCodeAction(document, computeHashNode, hashDataNode, localDeclarationStatement, disposeNodes)
                Return True
            End If

            Dim variableDeclaratorSyntax = TryCast(createHashNode, VariableDeclaratorSyntax)
            If variableDeclaratorSyntax IsNot Nothing Then
                codeAction = New RemoveNodeHashDataCodeAction(document, computeHashNode, hashDataNode, createHashNode, disposeNodes)
                Return True
            End If
            Return False
        End Function

        Protected Overrides Function GetHashDataSyntaxNode(computeType As PreferHashDataOverComputeHashAnalyzer.ComputeType, hashTypeName As String, computeHashNode As SyntaxNode) As SyntaxNode
            Dim argumentList = DirectCast(computeHashNode, InvocationExpressionSyntax).ArgumentList
            Return GetHashDataSyntaxNode(computeType, hashTypeName, argumentList)
        End Function

        Private Overloads Shared Function GetHashDataSyntaxNode(computeType As PreferHashDataOverComputeHashAnalyzer.ComputeType, hashTypeName As String, argumentList As ArgumentListSyntax) As SyntaxNode
            Select Case computeType
                Case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHash
                    Dim hashData = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(hashTypeName),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                    SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.HashDataMethodName))
                    Return SyntaxFactory.InvocationExpression(hashData, argumentList)
                Case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHashSection
                    Dim asSpan = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        argumentList.Arguments(0).GetExpression(),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                        SyntaxFactory.IdentifierName("AsSpan"))
                    Dim spanArgs = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(SyntaxFactory.List(argumentList.Arguments.Skip(1))))
                    Dim asSpanInvoked = SyntaxFactory.InvocationExpression(asSpan, spanArgs)
                    Dim hashData = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(hashTypeName),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                        SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.HashDataMethodName))
                    Dim args = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(Of ArgumentSyntax)(SyntaxFactory.SimpleArgument(asSpanInvoked)))
                    Return SyntaxFactory.InvocationExpression(hashData, args)
                Case PreferHashDataOverComputeHashAnalyzer.ComputeType.TryComputeHash
                    Dim hashData = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(hashTypeName),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                    SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.TryHashDataMethodName))
                    Return SyntaxFactory.InvocationExpression(hashData, argumentList)
            End Select
            Return Nothing
        End Function

        Private NotInheritable Class BasicRemoveUsingBlockHashDataCodeAction : Inherits HashDataCodeAction
            Public Sub New(document As Document,
                           computeHashNode As SyntaxNode,
                           hashDataNode As SyntaxNode,
                           usingBlockToRemove As UsingBlockSyntax)
                MyBase.New(document, computeHashNode, hashDataNode)
                Me.UsingBlockToRemove = usingBlockToRemove
            End Sub

            Public ReadOnly Property UsingBlockToRemove As UsingBlockSyntax

            Protected Overrides Sub EditNodes(documentEditor As DocumentEditor)
                Dim newStatements = UsingBlockToRemove.ReplaceNode(ComputeHashNode, HashDataNode).Statements.Select(Function(s) s.WithAdditionalAnnotations(Formatter.Annotation))
                documentEditor.InsertBefore(UsingBlockToRemove, newStatements)
                documentEditor.RemoveNode(UsingBlockToRemove)
            End Sub
        End Class
        Private NotInheritable Class BasicPreferHashDataOverComputeHashFixAllCodeAction : Inherits PreferHashDataOverComputeHashFixAllCodeAction
            Public Sub New(title As String, solution As Solution, diagnosticsToFix As List(Of KeyValuePair(Of Project, ImmutableArray(Of Diagnostic))))
                MyBase.New(title, solution, diagnosticsToFix)
            End Sub

            Friend Overrides Function FixDocumentRoot(root As SyntaxNode, hashInstanceTargets() As HashInstanceTarget) As SyntaxNode
                For Each target In hashInstanceTargets
                    For Each c In target.ComputeHashNodes
                        Dim tracked = root.GetCurrentNode(c.ComputeHashNode)
                        Dim a = GetHashDataSyntaxNode(c.ComputeType, c.HashTypeName, DirectCast(tracked, InvocationExpressionSyntax).ArgumentList)
                        root = root.ReplaceNode(tracked, a)
                    Next
                    If target.CreateNode Is Nothing Then
                        Continue For
                    End If

                    Dim currentCreateNode = root.GetCurrentNode(target.CreateNode)
                    Dim currentCreateNodeParent = currentCreateNode.Parent
                    Dim usingStatement = TryCast(currentCreateNodeParent, UsingStatementSyntax)
                    If usingStatement IsNot Nothing Then
                        Dim usingBlock = TryCast(usingStatement.Parent, UsingBlockSyntax)
                        If usingBlock IsNot Nothing Then
                            If usingStatement.Variables.Count = 1 Then
                                Dim statements = usingBlock.Statements.Select(Function(s) s.WithAdditionalAnnotations(Formatter.Annotation))
                                root = root.TrackNodes(usingBlock)
                                root = root.InsertNodesBefore(root.GetCurrentNode(usingBlock), statements)
                                root = root.RemoveNode(root.GetCurrentNode(usingBlock), SyntaxRemoveOptions.KeepNoTrivia)
                            Else
                                root = root.RemoveNode(currentCreateNode, SyntaxRemoveOptions.KeepNoTrivia)
                            End If
                        End If
                    End If

                    Dim localDeclarationStatement = TryCast(currentCreateNodeParent, LocalDeclarationStatementSyntax)
                    If localDeclarationStatement IsNot Nothing Then
                        root = root.RemoveNode(localDeclarationStatement, SyntaxRemoveOptions.KeepNoTrivia)
                    End If

                    Dim variableDeclaratorSyntax = TryCast(currentCreateNode, VariableDeclaratorSyntax)
                    If variableDeclaratorSyntax IsNot Nothing Then
                        root = root.RemoveNode(variableDeclaratorSyntax, SyntaxRemoveOptions.KeepNoTrivia)
                    End If

                    If target.DisposeNodes Is Nothing Then
                        Continue For
                    End If
                    For Each disposeNode In target.DisposeNodes
                        Dim trackedDisposeNode = root.GetCurrentNode(disposeNode)
                        root = root.RemoveNode(trackedDisposeNode, SyntaxRemoveOptions.KeepNoTrivia)
                    Next
                Next
                Return root
            End Function
        End Class
        Private NotInheritable Class BasicPreferHashDataOverComputeHashFixAllProvider : Inherits PreferHashDataOverComputeHashFixAllProvider
            Protected Overrides Function GetCodeAction(title As String, solution As Solution, diagnosticsToFix As List(Of KeyValuePair(Of Project, Immutable.ImmutableArray(Of Diagnostic)))) As PreferHashDataOverComputeHashFixAllCodeAction
                Return New BasicPreferHashDataOverComputeHashFixAllCodeAction(title, solution, diagnosticsToFix)
            End Function
        End Class
    End Class
End Namespace
