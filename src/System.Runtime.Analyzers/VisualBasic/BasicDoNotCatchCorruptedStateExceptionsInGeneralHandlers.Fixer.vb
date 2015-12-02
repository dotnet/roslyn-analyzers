' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers     
    ''' <summary>
    ''' CA2153: Do not catch corrupted state exceptions in general handlers.
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotCatchCorruptedStateExceptionsInGeneralHandlersFixer
        Inherits DoNotCatchCorruptedStateExceptionsInGeneralHandlersFixer 

    End Class
End Namespace
