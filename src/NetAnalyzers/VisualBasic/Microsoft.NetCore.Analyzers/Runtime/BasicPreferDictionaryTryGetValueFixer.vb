Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicPreferDictionaryTryGetValueFixer
        Inherits PreferDictionaryTryGetValueFixer

        Public Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
            Dim diagnostic = context.Diagnostics.FirstOrDefault()
            Dim dictionaryAccessLocation = diagnostic?.AdditionalLocations(0)
            If dictionaryAccessLocation Is Nothing
                Return
            End If

            Dim document = context.Document
            Dim root = Await document.GetSyntaxRootAsync().ConfigureAwait(False)

            Dim dictionaryAccess = root.FindNode(dictionaryAccessLocation.SourceSpan, getInnermostNodeForTie := true)
            Dim containsKeyInvocation = TryCast(root.FindNode(context.Span), InvocationExpressionSyntax)
            Dim containsKeyAccess = TryCast(containsKeyInvocation?.Expression, MemberAccessExpressionSyntax)
            If TryCast(dictionaryAccess, InvocationExpressionSyntax) Is Nothing Or containsKeyInvocation Is Nothing Or containsKeyAccess Is Nothing Then
                Return
            End If
            
            Dim replaceFunction = Async Function(ct as CancellationToken) As Task(Of Document)
                Dim editor = Await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(False)
                Dim generator = editor.Generator
                
                Dim tryGetValueAccess = generator.MemberAccessExpression(containsKeyAccess.Expression, "TryGetValue")
                Dim keyArgument = containsKeyInvocation.ArgumentList.Arguments.FirstOrDefault()
                
                
                Dim valueAssignment = generator.LocalDeclarationStatement(IdentifierName("Dim"), "value")
                Dim tryGetValueInvocation = generator.InvocationExpression(tryGetValueAccess, keyArgument, generator.Argument(generator.IdentifierName("value")))
                
                Dim ifStatement = containsKeyInvocation.AncestorsAndSelf().OfType(Of IfStatementSyntax).FirstOrDefault()
                editor.InsertBefore(ifStatement, valueAssignment)
                editor.ReplaceNode(containsKeyInvocation, tryGetValueInvocation)
                editor.ReplaceNode(dictionaryAccess, generator.IdentifierName("value"))
                
                Return editor.GetChangedDocument()
            End Function
            
            Dim action = CodeAction.Create(PreferDictionaryTryGetValueCodeFixTitle, replaceFunction, PreferDictionaryTryGetValueCodeFixTitle)
            context.RegisterCodeFix(action, context.Diagnostics)
        End Function
    End Class
End Namespace