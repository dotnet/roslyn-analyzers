' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.InteropServices.Analyzers     
    ''' <summary>
    ''' CA2101: Specify marshaling for PInvoke string arguments
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicSpecifyMarshalingForPInvokeStringArgumentsFixer
        Inherits SpecifyMarshalingForPInvokeStringArgumentsFixer 

    End Class
End Namespace
