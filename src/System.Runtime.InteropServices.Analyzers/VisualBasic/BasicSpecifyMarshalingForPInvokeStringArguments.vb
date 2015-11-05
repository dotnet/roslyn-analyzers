' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.InteropServices.Analyzers   
    ''' <summary>
    ''' CA2101: Specify marshaling for PInvoke string arguments
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicSpecifyMarshalingForPInvokeStringArgumentsAnalyzer
        Inherits SpecifyMarshalingForPInvokeStringArgumentsAnalyzer

    End Class
End Namespace