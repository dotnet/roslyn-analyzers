' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace Desktop.Analyzers
    ''' <summary>
    ''' CA1058: Types should not extend certain base types
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicTypesShouldNotExtendCertainBaseTypesFixer
        Inherits TypesShouldNotExtendCertainBaseTypesFixer

    End Class
End Namespace
