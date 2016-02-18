' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.InteropServices.Analyzers
    ''' <summary>
    ''' CA1414: Mark boolean PInvoke arguments with MarshalAs
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicMarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer
        Inherits MarkBooleanPInvokeArgumentsWithMarshalAsAnalyzer

    End Class
End Namespace