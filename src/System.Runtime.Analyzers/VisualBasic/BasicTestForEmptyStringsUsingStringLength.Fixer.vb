' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Runtime.Analyzers
    ''' <summary>
    ''' CA1820: Test for empty strings using string length
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicTestForEmptyStringsUsingStringLengthFixer
        Inherits TestForEmptyStringsUsingStringLengthFixer

    End Class
End Namespace
