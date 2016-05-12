' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Desktop.Analyzers
    ''' <summary>
    ''' CA1306: Set locale for data types
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicSetLocaleForDataTypesAnalyzer
        Inherits SetLocaleForDataTypesAnalyzer

    End Class
End Namespace