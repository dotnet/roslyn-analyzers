' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeQuality.Analyzers.Maintainability
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    ''' <summary>
    '''
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUseNameofInPlaceOfStringFixer
        Inherits UseNameOfInPlaceOfStringFixer

        Friend Overrides Function GetNameOfExpression(stringText As String) As SyntaxNode
            ' TODO how to create a nameof expression in VB?
            'Dim argument = SyntaxFactory.LiteralExpression(stringText);
            'Return SyntaxFactory.NameOfExpression(SyntaxFactory.Token(SyntaxKind.NameOfKeyword), SyntaxFactory.Token(SyntaxKind.OpenParenToken), argument, SyntaxFactory.Token(SyntaxKind.CloseParenToken))
            Return Nothing
        End Function
    End Class
End Namespace
