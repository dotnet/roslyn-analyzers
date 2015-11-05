' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers     
    ''' <summary>
    ''' CA2002: Do not lock on objects with weak identity
    ''' </summary>
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicDoNotLockOnObjectsWithWeakIdentityFixer
        Inherits DoNotLockOnObjectsWithWeakIdentityFixer 

    End Class
End Namespace
