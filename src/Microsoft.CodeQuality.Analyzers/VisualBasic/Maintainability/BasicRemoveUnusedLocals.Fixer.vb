' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Composition
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    ''' <summary>
    ''' CA1804: Remove unused locals
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicRemoveUnusedLocalsFixer
        Inherits RemoveUnusedLocalsFixer

        Public Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
            Get
                Return ImmutableArray.Create(BasicRemoveUnusedLocalsAnalyzer.RuleId)
            End Get
        End Property

        Protected Overrides Function GetAssignmentStatement(node As SyntaxNode) As SyntaxNode
            node = node.Parent
            If (node.Kind() = SyntaxKind.SimpleAssignmentStatement) Then
                Return node
            End If
            Return Nothing
        End Function

        Public Overrides Function GetFixAllProvider() As FixAllProvider
            Return New BasicRemoveLocalFixAllProvider()
        End Function

        Private Class BasicRemoveLocalFixAllProvider
            Inherits FixAllProvider

            Public Overrides Async Function GetFixAsync(fixAllContext As FixAllContext) As Task(Of CodeAction)
                Dim diagnostics = New List(Of KeyValuePair(Of Document, ImmutableArray(Of Diagnostic)))()
                For Each document In fixAllContext.Project.Documents
                    diagnostics.Add(New KeyValuePair(Of Document, ImmutableArray(Of Diagnostic))(document, Await fixAllContext.GetDocumentDiagnosticsAsync(document)))
                Next

                ' TODO rename/review name
                Return New BasicRemoveLocalFixAllAction("BasicRemoveLocalFixAllAction", fixAllContext.Solution, diagnostics)
            End Function
        End Class

        Friend Class BasicRemoveLocalFixAllAction
            Inherits RemoveLocalFixAllAction

            Public Sub New(title As String, solution As Solution, diagnosticsToFix As List(Of KeyValuePair(Of Document, ImmutableArray(Of Diagnostic))))
                MyBase.New(title, solution, diagnosticsToFix)
            End Sub

            Protected Overrides Sub RemoveAllUnusedLocalDeclarations(nodesToRemove As HashSet(Of SyntaxNode))
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

            Protected Overrides Async Function GetNodeToRemoveAsync(document As Document, diagnostic As Diagnostic, cancellationToken As CancellationToken) As Task(Of SyntaxNode)
                Dim root = Await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)

                Dim diagnosticSpan = diagnostic.Location.SourceSpan

                Dim variableDeclarator = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of VariableDeclaratorSyntax)().First()


                If (variableDeclarator Is Nothing) Then
                    Return Nothing
                End If

                ' Bail out if the initializer Is non-constant (could have side effects if removed).
                If (variableDeclarator.Initializer IsNot Nothing) Then
                    Dim SemanticModel = Await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
                    If (Not SemanticModel.GetConstantValue(variableDeclarator.Initializer.Value).HasValue) Then
                        Return Nothing
                    End If
                End If

                ' Bail out for code with syntax errors - parent of a declaration Is Not a local declaration statement.
                Dim localDeclaration As LocalDeclarationStatementSyntax = CType(variableDeclarator.Parent, LocalDeclarationStatementSyntax)
                If (localDeclaration Is Nothing) Then
                    Return Nothing
                End If

                ' If the statement declares a single variable, the code fix should remove the whole statement.
                ' Otherwise, the code fix should remove only this variable declaration.
                Dim nodeToRemove As SyntaxNode
                If (localDeclaration.Declarators.Count = 1) Then
                    nodeToRemove = localDeclaration

                Else

                    nodeToRemove = variableDeclarator
                End If

                Return nodeToRemove
            End Function

        End Class


    End Class
End Namespace
