' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Desktop.Analyzers
    ''' <summary>
    ''' CA2212: Do not mark serviced components with WebMethod
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotMarkServicedComponentsWithWebMethodAnalyzer
        Inherits DoNotMarkServicedComponentsWithWebMethodAnalyzer

    End Class
End Namespace