' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.ApiDesignGuidelines.Analyzers   
    ''' <summary>
    ''' CA1001: Types that own disposable fields should be disposable
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicTypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer
        Inherits TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer

    End Class
End Namespace