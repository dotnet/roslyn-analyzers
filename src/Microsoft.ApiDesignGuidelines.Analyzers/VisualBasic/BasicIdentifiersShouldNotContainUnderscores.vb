' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.ApiDesignGuidelines.Analyzers
    ''' <summary>
    ''' CA1707: Identifiers should not contain underscores
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicIdentifiersShouldNotContainUnderscoresAnalyzer
        Inherits IdentifiersShouldNotContainUnderscoresAnalyzer(Of SyntaxKind)

        Private Shared s_syntaxKinds As SyntaxKind() = {SyntaxKind.Parameter, SyntaxKind.TypeParameter}

        Public Overrides ReadOnly Property SyntaxKinds As SyntaxKind()
            Get
                Return s_syntaxKinds
            End Get
        End Property
    End Class
End Namespace