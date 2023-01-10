' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance

    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicPreferDictionaryTryAddValueOverGuardedAddFixer
        Inherits PreferDictionaryTryAddValueOverGuardedAddFixer

        Public Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
            Dim diagnostic = context.Diagnostics(0)
            Dim dictionaryAddLocation = diagnostic.AdditionalLocations(0)

            Dim document = context.Document
            Dim root = Await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

            Dim dictionaryAddInvocation = TryCast(root.FindNode(dictionaryAddLocation.SourceSpan, getInnermostNodeForTie:=True), InvocationExpressionSyntax)
            Dim containsKeyInvocation = TryCast(root.FindNode(context.Span), InvocationExpressionSyntax)
            Dim containsKeyAccess = TryCast(containsKeyInvocation?.Expression, MemberAccessExpressionSyntax)
            If _
                dictionaryAddInvocation Is Nothing OrElse
                containsKeyInvocation Is Nothing OrElse
                containsKeyAccess Is Nothing Then
                Return
            End If

            Dim replaceFunction = Async Function(ct as CancellationToken) As Task(Of Document)
                                      Dim editor = Await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(False)
                                      Dim generator = editor.Generator

                                      Dim tryAddValueAccess = generator.MemberAccessExpression(containsKeyAccess.Expression, TryAdd)
                                      Dim dictionaryAddArguments = dictionaryAddInvocation.ArgumentList.Arguments
                                      Dim tryAddInvocation = generator.InvocationExpression(tryAddValueAccess, dictionaryAddArguments(0), dictionaryAddArguments(1))

                                      Dim ifStatement = containsKeyInvocation.AncestorsAndSelf().OfType(Of MultiLineIfBlockSyntax).FirstOrDefault()
                                      If ifStatement Is Nothing Then
                                          Return editor.OriginalDocument
                                      End If

                                      Dim unary = TryCast(ifStatement.IfStatement.Condition, UnaryExpressionSyntax)
                                      If unary IsNot Nothing And unary.IsKind(SyntaxKind.NotExpression)
                                          If ifStatement.Statements.Count = 1 Then
                                              If ifStatement.ElseBlock Is Nothing Then
                                                  Dim invocationWithTrivia = tryAddInvocation.WithTriviaFrom(ifStatement)
                                                  editor.ReplaceNode(ifStatement, generator.ExpressionStatement(invocationWithTrivia))
                                              Else
                                                  Dim newIf = ifStatement.WithStatements(ifStatement.ElseBlock.Statements).
                                                          WithElseBlock(Nothing).
                                                          WithIfStatement(ifStatement.IfStatement.ReplaceNode(containsKeyInvocation, tryAddInvocation))
                                                  editor.ReplaceNode(ifStatement, newIf)
                                              End If
                                          Else
                                              editor.RemoveNode(dictionaryAddInvocation.Parent, SyntaxRemoveOptions.KeepNoTrivia)
                                              editor.ReplaceNode(unary, tryAddInvocation)
                                          End If
                                      Else If ifStatement.IfStatement.Condition.IsKind(SyntaxKind.InvocationExpression) And ifStatement.ElseBlock IsNot Nothing
                                          Dim negatedTryAddInvocation = generator.LogicalNotExpression(tryAddInvocation)
                                          editor.ReplaceNode(containsKeyInvocation, negatedTryAddInvocation)
                                          if ifStatement.ElseBlock.Statements.Count = 1 Then
                                              editor.RemoveNode(ifStatement.ElseBlock, SyntaxRemoveOptions.KeepNoTrivia)
                                          Else
                                              editor.RemoveNode(dictionaryAddInvocation.Parent, SyntaxRemoveOptions.KeepNoTrivia)
                                          End If
                                      End If

                                      Return editor.GetChangedDocument()
                                  End Function

            Dim action = CodeAction.Create(CodeFixTitle, replaceFunction, CodeFixTitle)
            context.RegisterCodeFix(action, context.Diagnostics)
        End Function
    End Class
End Namespace