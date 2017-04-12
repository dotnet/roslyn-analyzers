' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Runtime.InteropServices.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

Namespace System.Runtime.InteropServices.VisualBasic.Analyzers
    ''' <summary>
    ''' CA2205: Use managed equivalents of win32 api
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicUseManagedEquivalentsOfWin32ApiAnalyzer
        Inherits UseManagedEquivalentsOfWin32ApiAnalyzer

    End Class
End Namespace