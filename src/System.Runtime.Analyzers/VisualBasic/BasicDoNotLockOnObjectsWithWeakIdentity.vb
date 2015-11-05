' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace System.Runtime.Analyzers   
    ''' <summary>
    ''' CA2002: Do not lock on objects with weak identity
    ''' </summary>
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotLockOnObjectsWithWeakIdentityAnalyzer
        Inherits DoNotLockOnObjectsWithWeakIdentityAnalyzer

    End Class
End Namespace