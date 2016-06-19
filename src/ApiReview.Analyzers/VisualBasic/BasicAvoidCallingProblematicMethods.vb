' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports ApiReview.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace ApiReview.VisualBasic.Analyzers
    ''' <summary>
    ''' CA2001: Avoid calling problematic methods
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicAvoidCallingProblematicMethodsAnalyzer
        Inherits AvoidCallingProblematicMethodsAnalyzer

    End Class
End Namespace