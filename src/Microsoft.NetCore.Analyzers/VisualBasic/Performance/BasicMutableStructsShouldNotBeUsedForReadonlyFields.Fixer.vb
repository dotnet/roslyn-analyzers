' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers
Imports Microsoft.NetCore.Analyzers.Performance

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicMutableStructsShouldNotBeUsedForReadonlyFieldsFixer
        Inherits MutableStructsShouldNotBeUsedForReadonlyFieldsFixer

        Protected Overrides Sub AnalyzeCodeFix(context As CodeFixContext, targetNode As SyntaxNode)
            Dim fieldDeclarationSyntax = TryCast(targetNode, FieldDeclarationSyntax)

            If fieldDeclarationSyntax Is Nothing
                Return
            End If

            Dim readonlyModifiers = fieldDeclarationSyntax.Modifiers.Where(function(token) token.IsKind(SyntaxKind.ReadOnlyKeyword)).ToArray()

            if Not readonlyModifiers.Any()
                Return
            End If

            Dim removeReadonlyAction = CodeAction.Create(MicrosoftNetCoreAnalyzersResources.MutableStructsShouldNotBeUsedForReadonlyFieldsTitle, Async Function(token) Await RemoveReadonlyKeyword(context, fieldDeclarationSyntax).ConfigureAwait(False), EquivalencyKey)

            context.RegisterCodeFix(removeReadonlyAction, context.Diagnostics)
        End Sub

        Private Shared Async Function RemoveReadonlyKeyword(context As CodeFixContext, fieldDeclarationSyntax As FieldDeclarationSyntax) As Task(Of Document)
            Dim editor = Await DocumentEditor.CreateAsync(context.Document, context.CancellationToken).ConfigureAwait(false)
            Dim withoutReadonly = fieldDeclarationSyntax.WithModifiers(new SyntaxTokenList(fieldDeclarationSyntax.Modifiers.Where(function(token) Not token.IsKind(SyntaxKind.ReadOnlyKeyword))))

            editor.ReplaceNode(fieldDeclarationSyntax, withoutReadonly)

            Return editor.GetChangedDocument()
        End Function
    End Class
End NameSpace