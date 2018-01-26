' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeQuality.Analyzers.Maintainability

Namespace Microsoft.CodeQuality.VisualBasic.Analyzers.Maintainability

    ''' <summary>
    ''' CA1507: Use nameof to express symbol names
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicUseNameofInPlaceOfStringAnalyzer
        Inherits UseNameofInPlaceOfStringAnalyzer

        Friend Overrides Function ApplicableLanguageVersion(options As ParseOptions) As Boolean
            Dim langVersion = CType(options, VisualBasicParseOptions).LanguageVersion
            Return (langVersion > LanguageVersion.VisualBasic12)
        End Function
    End Class
End Namespace