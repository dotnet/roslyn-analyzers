' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Runtime.Analyzers
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic

Namespace System.Runtime.VisualBasic.Analyzers
    ''' <summary>
    ''' CA2207: Initialize value type static fields inline
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicInitializeStaticFieldsInlineFixer
        Inherits InitializeStaticFieldsInlineFixer(Of SyntaxKind)

    End Class
End Namespace
