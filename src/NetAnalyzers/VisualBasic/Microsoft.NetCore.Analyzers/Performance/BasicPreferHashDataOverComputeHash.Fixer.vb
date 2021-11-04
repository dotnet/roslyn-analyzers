' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Formatting
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicPreferHashDataOverComputeHashFixer : Inherits PreferHashDataOverComputeHashFixer
        Private Shared ReadOnly s_fixAllProvider As New BasicPreferHashDataOverComputeHashFixAllProvider()
        Private Shared ReadOnly s_helper As New BasicPreferHashDataOverComputeHashFixHelper()

        Public Overrides Function GetFixAllProvider() As FixAllProvider
            Return s_fixAllProvider
        End Function

        Protected Overrides ReadOnly Property Helper As PreferHashDataOverComputeHashFixHelper
            Get
                Return s_helper
            End Get
        End Property

        Private NotInheritable Class BasicPreferHashDataOverComputeHashFixAllProvider : Inherits PreferHashDataOverComputeHashFixAllProvider
            Protected Overrides ReadOnly Property Helper As PreferHashDataOverComputeHashFixHelper
                Get
                    Return s_helper
                End Get
            End Property
        End Class

        Private NotInheritable Class BasicPreferHashDataOverComputeHashFixHelper : Inherits PreferHashDataOverComputeHashFixHelper
            Protected Overrides Function FixHashCreateNode(root As SyntaxNode, createNode As SyntaxNode) As SyntaxNode
                Dim currentCreateNode = root.GetCurrentNode(createNode)
                Dim currentCreateNodeParent = currentCreateNode.Parent

                If TypeOf currentCreateNodeParent Is UsingStatementSyntax Then
                    Dim usingStatement = DirectCast(currentCreateNodeParent, UsingStatementSyntax)
                    Dim usingBlock = TryCast(usingStatement.Parent, UsingBlockSyntax)
                    If usingBlock IsNot Nothing Then
                        If usingStatement.Variables.Count = 1 Then
                            root = MoveStatementsOutOfUsingBlockWithFormatting(root, usingBlock)
                        Else
                            root = RemoveNodeWithFormatting(root, currentCreateNode)
                        End If
                    End If
                ElseIf TypeOf currentCreateNodeParent Is LocalDeclarationStatementSyntax Then
                    Dim localDeclarationStatement = DirectCast(currentCreateNodeParent, LocalDeclarationStatementSyntax)
                    root = RemoveNodeWithFormatting(root, localDeclarationStatement)
                ElseIf TypeOf currentCreateNode Is VariableDeclaratorSyntax Then
                    Dim variableDeclaratorSyntax = DirectCast(currentCreateNode, VariableDeclaratorSyntax)
                    root = RemoveNodeWithFormatting(root, variableDeclaratorSyntax)
                End If
                Return root
            End Function

            Private Function MoveStatementsOutOfUsingBlockWithFormatting(root As SyntaxNode, usingBlock As UsingBlockSyntax) As SyntaxNode
                Dim statements = usingBlock.Statements.Select(Function(s, i)
                                                                  Dim statement = s
                                                                  If i = 0 Then
                                                                      Dim newTrivia = New SyntaxTriviaList()
                                                                      newTrivia = AddRangeIfInteresting(newTrivia, usingBlock.GetLeadingTrivia())
                                                                      newTrivia = AddRangeIfInteresting(newTrivia, usingBlock.UsingStatement.GetTrailingTrivia())
                                                                      newTrivia = AddRangeIfInteresting(newTrivia, statement.GetLeadingTrivia())
                                                                      statement = statement.WithLeadingTrivia(newTrivia)
                                                                  ElseIf i = usingBlock.Statements.Count - 1 Then
                                                                      Dim newTrivia = statement.GetTrailingTrivia()
                                                                      newTrivia = AddRangeIfInteresting(newTrivia, usingBlock.EndUsingStatement.GetTrailingTrivia())
                                                                      statement = statement.WithTrailingTrivia(newTrivia)
                                                                  End If
                                                                  Return statement
                                                              End Function)
                Dim parent = usingBlock.Parent
                root = root.TrackNodes(parent)
                Dim newParent = parent.TrackNodes(usingBlock)
                newParent = newParent.InsertNodesBefore(newParent.GetCurrentNode(usingBlock), statements)
                newParent = newParent.RemoveNode(newParent.GetCurrentNode(usingBlock), SyntaxRemoveOptions.KeepNoTrivia).WithAdditionalAnnotations(Formatter.Annotation)
                root = root.ReplaceNode(root.GetCurrentNode(parent), newParent)
                Return root
            End Function

            Protected Overrides Function IsInterestingTrivia(triviaList As SyntaxTriviaList) As Boolean
                Return triviaList.Any(Function(t)
                                          Return Not t.IsKind(SyntaxKind.WhitespaceTrivia) And Not t.IsKind(SyntaxKind.EndOfLineTrivia)
                                      End Function)
            End Function

            Protected Overrides Function GetHashDataSyntaxNode(computeType As PreferHashDataOverComputeHashAnalyzer.ComputeType, hashTypeName As String, computeHashNode As SyntaxNode) As SyntaxNode
                Dim argumentList = DirectCast(computeHashNode, InvocationExpressionSyntax).ArgumentList
                Select Case computeType
                    Case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHash
                        Dim hashData = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(hashTypeName),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                    SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.HashDataMethodName))
                        Dim arg = argumentList.Arguments(0)
                        If arg.IsNamed Then
                            arg = DirectCast(arg, SimpleArgumentSyntax).WithNameColonEquals(SyntaxFactory.NameColonEquals(SyntaxFactory.IdentifierName("source")))
                        End If
                        Dim args = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(Of ArgumentSyntax)(arg))
                        Return SyntaxFactory.InvocationExpression(hashData, args)
                    Case PreferHashDataOverComputeHashAnalyzer.ComputeType.ComputeHashSection
                        Dim list = argumentList.Arguments.ToList()
                        Dim firstArg = list.Find(Function(a) (Not a.IsNamed) OrElse DirectCast(a, SimpleArgumentSyntax).NameColonEquals.Name.Identifier.Text.Equals("buffer", StringComparison.OrdinalIgnoreCase))
                        list.Remove(firstArg)
                        Dim secondArgIndex = list.FindIndex(Function(a) (Not a.IsNamed) OrElse DirectCast(a, SimpleArgumentSyntax).NameColonEquals.Name.Identifier.Text.Equals("offset", StringComparison.OrdinalIgnoreCase))
                        Dim thirdArgIndex = If(secondArgIndex = 0, 1, 0) ' second And third can only be 0 Or 1
                        Dim secondArg = DirectCast(list(secondArgIndex), SimpleArgumentSyntax)
                        If secondArg.IsNamed Then
                            list(secondArgIndex) = secondArg.WithNameColonEquals(SyntaxFactory.NameColonEquals(SyntaxFactory.IdentifierName("start")))
                        End If
                        Dim thirdArg = DirectCast(list(thirdArgIndex), SimpleArgumentSyntax)
                        If thirdArg.IsNamed Then
                            list(thirdArgIndex) = thirdArg.WithNameColonEquals(SyntaxFactory.NameColonEquals(SyntaxFactory.IdentifierName("length")))
                        End If

                        Dim asSpan = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        firstArg.GetExpression(),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                        SyntaxFactory.IdentifierName("AsSpan"))
                        Dim spanArgs = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(list))
                        Dim asSpanInvoked = SyntaxFactory.InvocationExpression(asSpan, spanArgs)
                        Dim hashData = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(hashTypeName),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                        SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.HashDataMethodName))

                        Dim arg = SyntaxFactory.SimpleArgument(asSpanInvoked)
                        If firstArg.IsNamed Then
                            arg = arg.WithNameColonEquals(SyntaxFactory.NameColonEquals(SyntaxFactory.IdentifierName("source")))
                        End If
                        Dim args = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(Of ArgumentSyntax)(arg))
                        Return SyntaxFactory.InvocationExpression(hashData, args)
                    Case PreferHashDataOverComputeHashAnalyzer.ComputeType.TryComputeHash
                        ' method has same parameter names
                        Dim hashData = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(hashTypeName),
                        SyntaxFactory.Token(SyntaxKind.DotToken),
                    SyntaxFactory.IdentifierName(PreferHashDataOverComputeHashAnalyzer.TryHashDataMethodName))
                        Return SyntaxFactory.InvocationExpression(hashData, argumentList)
                End Select
                Debug.Fail("there is only 3 type of ComputeHash")
                Throw New InvalidOperationException("there is only 3 type of ComputeHash")
            End Function
        End Class
    End Class
End Namespace
