' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Runtime.Analyzers
    ''' <summary>
    ''' CA2242: Test for NaN correctly
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicTestForNaNCorrectlyFixer
        Inherits TestForNaNCorrectlyFixer

    End Class
End Namespace
