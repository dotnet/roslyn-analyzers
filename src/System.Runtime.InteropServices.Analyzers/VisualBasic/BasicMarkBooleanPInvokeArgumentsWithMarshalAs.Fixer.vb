' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Runtime.InteropServices.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes

Namespace System.Runtime.InteropServices.VisualBasic.Analyzers
    ''' <summary>
    ''' CA1414: Mark boolean PInvoke arguments with MarshalAs
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicMarkBooleanPInvokeArgumentsWithMarshalAsFixer
        Inherits MarkBooleanPInvokeArgumentsWithMarshalAsFixer

    End Class
End Namespace
