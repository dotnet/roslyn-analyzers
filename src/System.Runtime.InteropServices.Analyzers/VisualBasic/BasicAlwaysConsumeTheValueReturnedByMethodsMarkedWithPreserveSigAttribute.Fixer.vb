' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.InteropServices.Analyzers     
    ''' <summary>
    ''' RS0015: Always consume the value returned by methods marked with PreserveSigAttribute
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicAlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeFixer
        Inherits AlwaysConsumeTheValueReturnedByMethodsMarkedWithPreserveSigAttributeFixer 

    End Class
End Namespace
