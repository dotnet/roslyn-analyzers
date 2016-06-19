' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Desktop.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace Desktop.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1301: Avoid duplicate accelerators
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicAvoidDuplicateAcceleratorsAnalyzer
        Inherits AvoidDuplicateAcceleratorsAnalyzer

    End Class
End Namespace