' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability
    ''' <summary>
    '''
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUseNameofInPlaceOfStringFixer
        Inherits UseNameOfInPlaceOfStringFixer

        Friend Overrides Function GetNameOfExpression(stringText As String, document As Document) As SyntaxNode
            Dim generator = SyntaxGenerator.GetGenerator(document)
            Return generator.NameOfExpression(generator.IdentifierName(stringText))
        End Function
    End Class
End Namespace
