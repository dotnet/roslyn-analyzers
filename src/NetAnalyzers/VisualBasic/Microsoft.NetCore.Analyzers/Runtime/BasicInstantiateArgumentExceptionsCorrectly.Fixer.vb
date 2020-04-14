' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.NetCore.Analyzers.Runtime
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    ''' <summary>
    ''' CA2208: Instantiate argument exceptions correctly
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicInstantiateArgumentExceptionsCorrectlyFixer
        Inherits InstantiateArgumentExceptionsCorrectlyFixer

        Protected Overrides Sub PopulateCodeFix(context As CodeFixContext, diagnostic As Diagnostic, paramPositionString As String, node As SyntaxNode)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace
