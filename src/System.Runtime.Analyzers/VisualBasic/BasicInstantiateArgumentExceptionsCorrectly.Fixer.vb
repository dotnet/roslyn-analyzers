' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Runtime.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Runtime.VisualBasic.Analyzers
    ''' <summary>
    ''' CA2208: Instantiate argument exceptions correctly
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicInstantiateArgumentExceptionsCorrectlyFixer
        Inherits InstantiateArgumentExceptionsCorrectlyFixer

    End Class
End Namespace
