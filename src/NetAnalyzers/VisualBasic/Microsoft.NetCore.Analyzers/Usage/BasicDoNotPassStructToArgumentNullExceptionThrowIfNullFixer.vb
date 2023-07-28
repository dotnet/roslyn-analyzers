﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Usage

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Usage
    <ExportCodeFixProvider(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotPassStructToArgumentNullExceptionThrowIfNullFixer
        Inherits DoNotPassStructToArgumentNullExceptionThrowIfNullFixer(Of InvocationExpressionSyntax)

        Protected Overrides Async Function GetNewRootForNullableStructAsync(document As Document, invocation As InvocationExpressionSyntax, cancellationToken As CancellationToken) As Task(Of SyntaxNode)
            Dim editor = Await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(False)
            Dim generator = editor.Generator
            Dim nullableStructExpression = invocation.ArgumentList.Arguments(0).GetExpression()
            Dim condition = generator.LogicalNotExpression(generator.MemberAccessExpression(nullableStructExpression, HasValue))
            Dim nameOfExpression = generator.NameOfExpression(nullableStructExpression)
            Dim argumentNullEx = generator.ObjectCreationExpression(generator.IdentifierName(ArgumentNullException), nameOfExpression)
            Dim throwExpression = generator.ThrowStatement(argumentNullEx)
            Dim ifStatement = editor.Generator.IfStatement(condition,New SyntaxNode() {throwExpression})
            editor.ReplaceNode(invocation.Parent, ifStatement)

            Return editor.GetChangedRoot()
        End Function
    End Class
End Namespace