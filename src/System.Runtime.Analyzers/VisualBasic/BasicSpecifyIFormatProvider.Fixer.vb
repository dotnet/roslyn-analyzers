' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Runtime.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Runtime.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1305: Specify IFormatProvider
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicSpecifyIFormatProviderFixer
        Inherits SpecifyIFormatProviderFixer

    End Class
End Namespace
